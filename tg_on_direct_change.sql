-- =========================================================
-- ДОПОЛНИТЕЛЬНЫЕ ЗАЩИТЫ ДАННЫХ
-- =========================================================

-- ---------------------------------------------------------
-- 1. Обновляем функцию записи истории статусов:
--    теперь комментарий берётся из session setting,
--    чтобы history можно было сделать append-only
-- ---------------------------------------------------------

CREATE OR REPLACE FUNCTION trgfn_repair_order_status_history()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_comment TEXT;
BEGIN
    IF NEW.status_id IS DISTINCT FROM OLD.status_id THEN
        v_comment := NULLIF(current_setting('app.status_change_comment', true), '');

        INSERT INTO order_status_history (
            order_id,
            status_id,
            changed_at,
            comment
        )
        VALUES (
            NEW.order_id,
            NEW.status_id,
            CURRENT_TIMESTAMP,
            v_comment
        );
    END IF;

    RETURN NEW;
END;
$$;


-- ---------------------------------------------------------
-- 2. Защита системных полей REPAIR_ORDER
--    order_number и created_at нельзя менять вообще
--    status_id / accepted_at / completed_at / issued_at
--    можно менять только через change_repair_order_status(...)
-- ---------------------------------------------------------

CREATE OR REPLACE FUNCTION trgfn_protect_repair_order_system_fields()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.order_number IS DISTINCT FROM OLD.order_number THEN
        RAISE EXCEPTION 'order_number cannot be changed';
    END IF;

    IF NEW.created_at IS DISTINCT FROM OLD.created_at THEN
        RAISE EXCEPTION 'created_at cannot be changed';
    END IF;

    IF
        NEW.status_id    IS DISTINCT FROM OLD.status_id OR
        NEW.accepted_at  IS DISTINCT FROM OLD.accepted_at OR
        NEW.completed_at IS DISTINCT FROM OLD.completed_at OR
        NEW.issued_at    IS DISTINCT FROM OLD.issued_at
    THEN
        IF current_setting('app.allow_status_change', true) IS DISTINCT FROM 'on' THEN
            RAISE EXCEPTION
                'status_id / accepted_at / completed_at / issued_at can be changed only via change_repair_order_status()';
        END IF;
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_protect_repair_order_system_fields ON repair_order;

CREATE TRIGGER trg_protect_repair_order_system_fields
BEFORE UPDATE ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_protect_repair_order_system_fields();


-- ---------------------------------------------------------
-- 3. ORDER_STATUS_HISTORY = append-only
--    UPDATE и DELETE запрещены
-- ---------------------------------------------------------

CREATE OR REPLACE FUNCTION trgfn_order_status_history_append_only()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE EXCEPTION 'order_status_history is append-only: UPDATE and DELETE are not allowed';
END;
$$;

DROP TRIGGER IF EXISTS trg_order_status_history_no_update ON order_status_history;
DROP TRIGGER IF EXISTS trg_order_status_history_no_delete ON order_status_history;

CREATE TRIGGER trg_order_status_history_no_update
BEFORE UPDATE ON order_status_history
FOR EACH ROW
EXECUTE FUNCTION trgfn_order_status_history_append_only();

CREATE TRIGGER trg_order_status_history_no_delete
BEFORE DELETE ON order_status_history
FOR EACH ROW
EXECUTE FUNCTION trgfn_order_status_history_append_only();


-- ---------------------------------------------------------
-- 4. PAYMENT = immutable
--    после вставки запись нельзя менять или удалять
-- ---------------------------------------------------------

CREATE OR REPLACE FUNCTION trgfn_payment_no_update_delete()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE EXCEPTION 'payment records cannot be updated or deleted';
END;
$$;

DROP TRIGGER IF EXISTS trg_payment_no_update ON payment;
DROP TRIGGER IF EXISTS trg_payment_no_delete ON payment;

CREATE TRIGGER trg_payment_no_update
BEFORE UPDATE ON payment
FOR EACH ROW
EXECUTE FUNCTION trgfn_payment_no_update_delete();

CREATE TRIGGER trg_payment_no_delete
BEFORE DELETE ON payment
FOR EACH ROW
EXECUTE FUNCTION trgfn_payment_no_update_delete();


-- ---------------------------------------------------------
-- 5. Переписываем change_repair_order_status(...)
--    теперь:
--    - включает служебный флаг для изменения статуса
--    - кладёт комментарий в session setting
--    - не делает UPDATE order_status_history
-- ---------------------------------------------------------

CREATE OR REPLACE FUNCTION change_repair_order_status(
    p_order_id INT,
    p_new_status_name VARCHAR,
    p_comment TEXT DEFAULT NULL
)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_current_status_name VARCHAR(50);
    v_new_status_id SMALLINT;
    v_now TIMESTAMP := CURRENT_TIMESTAMP;
BEGIN
    SELECT os.status_name
    INTO v_current_status_name
    FROM repair_order ro
    JOIN order_status os ON os.status_id = ro.status_id
    WHERE ro.order_id = p_order_id
    FOR UPDATE;

    IF v_current_status_name IS NULL THEN
        RAISE EXCEPTION 'Repair order % not found', p_order_id;
    END IF;

    SELECT status_id
    INTO v_new_status_id
    FROM order_status
    WHERE status_name = p_new_status_name;

    IF v_new_status_id IS NULL THEN
        RAISE EXCEPTION 'Status "%" not found in order_status', p_new_status_name;
    END IF;

    IF v_current_status_name = p_new_status_name THEN
        RETURN;
    END IF;

    IF v_current_status_name IN ('Closed', 'Canceled') THEN
        RAISE EXCEPTION
            'Repair order % is already in final status: %',
            p_order_id, v_current_status_name;
    END IF;

    IF NOT (
        (v_current_status_name = 'Created'  AND p_new_status_name IN ('Accepted', 'Canceled')) OR
        (v_current_status_name = 'Accepted' AND p_new_status_name IN ('InRepair', 'Canceled')) OR
        (v_current_status_name = 'InRepair' AND p_new_status_name IN ('Ready', 'Canceled')) OR
        (v_current_status_name = 'Ready'    AND p_new_status_name = 'Closed')
    ) THEN
        RAISE EXCEPTION
            'Invalid status transition: % -> % for order %',
            v_current_status_name, p_new_status_name, p_order_id;
    END IF;

    IF p_new_status_name = 'InRepair' THEN
        IF NOT has_order_services(p_order_id) THEN
            RAISE EXCEPTION
                'Order % cannot be moved to InRepair because it has no services',
                p_order_id;
        END IF;
    END IF;

    IF p_new_status_name = 'Ready' THEN
        IF NOT are_all_order_services_completed(p_order_id) THEN
            RAISE EXCEPTION
                'Order % cannot be moved to Ready because not all services are completed',
                p_order_id;
        END IF;
    END IF;

    IF p_new_status_name = 'Closed' THEN
        IF NOT are_all_order_services_completed(p_order_id) THEN
            RAISE EXCEPTION
                'Order % cannot be closed because not all services are completed',
                p_order_id;
        END IF;

        IF NOT is_order_fully_paid(p_order_id) THEN
            RAISE EXCEPTION
                'Order % cannot be closed because it is not fully paid',
                p_order_id;
        END IF;
    END IF;

    -- Разрешаем триггеру понять, что смена статуса легальная
    PERFORM set_config('app.allow_status_change', 'on', true);

    -- Передаём комментарий в AFTER-trigger истории статусов
    PERFORM set_config('app.status_change_comment', COALESCE(p_comment, ''), true);

    UPDATE repair_order
    SET
        status_id = v_new_status_id,
        accepted_at = CASE
            WHEN p_new_status_name = 'Accepted' AND accepted_at IS NULL
                THEN v_now
            ELSE accepted_at
        END,
        completed_at = CASE
            WHEN p_new_status_name = 'Ready' AND completed_at IS NULL
                THEN v_now
            ELSE completed_at
        END,
        issued_at = CASE
            WHEN p_new_status_name = 'Closed' AND issued_at IS NULL
                THEN v_now
            ELSE issued_at
        END
    WHERE order_id = p_order_id;
END;
$$;