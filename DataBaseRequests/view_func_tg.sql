-- =========================================================
-- ПРЕДСТАВЛЕНИЯ
-- =========================================================

CREATE OR REPLACE VIEW vw_order_cost_breakdown AS
WITH service_cost AS (
    SELECT
        os.order_id,
        COALESCE(SUM(os.quantity * os.price_at_moment), 0)::NUMERIC(10,2) AS service_sum
    FROM order_service os
    GROUP BY os.order_id
),
part_cost AS (
    SELECT
        osp.order_id,
        COALESCE(SUM(osp.quantity * osp.price_at_moment), 0)::NUMERIC(10,2) AS part_sum
    FROM order_service_part osp
    GROUP BY osp.order_id
)
SELECT
    ro.order_id,
    ro.order_number,
    COALESCE(sc.service_sum, 0)::NUMERIC(10,2) AS service_sum,
    COALESCE(pc.part_sum, 0)::NUMERIC(10,2) AS part_sum,
    (COALESCE(sc.service_sum, 0) + COALESCE(pc.part_sum, 0))::NUMERIC(10,2) AS calculated_total,
    ro.total_cost::NUMERIC(10,2) AS stored_total,
    (ro.total_cost - (COALESCE(sc.service_sum, 0) + COALESCE(pc.part_sum, 0)))::NUMERIC(10,2) AS difference
FROM repair_order ro
LEFT JOIN service_cost sc ON sc.order_id = ro.order_id
LEFT JOIN part_cost pc ON pc.order_id = ro.order_id;


CREATE OR REPLACE VIEW vw_order_payments AS
SELECT
    ro.order_id,
    ro.order_number,
    ro.is_warranty_repair,
    ro.total_cost::NUMERIC(10,2) AS total_cost,
    COALESCE(SUM(p.amount), 0)::NUMERIC(10,2) AS paid_amount,
    (ro.total_cost - COALESCE(SUM(p.amount), 0))::NUMERIC(10,2) AS remaining_amount,
    CASE
        WHEN ro.is_warranty_repair THEN 0::NUMERIC(10,2)
        ELSE (ro.total_cost - COALESCE(SUM(p.amount), 0))::NUMERIC(10,2)
    END AS required_to_close_amount,
    COUNT(p.payment_id) AS payments_count,
    MAX(p.payment_date) AS last_payment_date
FROM repair_order ro
LEFT JOIN payment p ON p.order_id = ro.order_id
GROUP BY ro.order_id, ro.order_number, ro.is_warranty_repair, ro.total_cost;


CREATE OR REPLACE VIEW vw_order_status_timestamps AS
SELECT
    osh.order_id,
    MIN(osh.changed_at) FILTER (WHERE os.status_name = 'Created')  AS created_at,
    MIN(osh.changed_at) FILTER (WHERE os.status_name = 'Accepted') AS accepted_at,
    MIN(osh.changed_at) FILTER (WHERE os.status_name = 'InRepair') AS repair_started_at,
    MIN(osh.changed_at) FILTER (WHERE os.status_name = 'Ready')    AS completed_at,
    MIN(osh.changed_at) FILTER (WHERE os.status_name = 'Closed')   AS issued_at,
    MIN(osh.changed_at) FILTER (WHERE os.status_name = 'Canceled') AS canceled_at,
    MAX(osh.changed_at) AS last_status_changed_at
FROM order_status_history osh
JOIN order_status os ON os.status_id = osh.status_id
GROUP BY osh.order_id;


CREATE OR REPLACE VIEW vw_order_full_info AS
SELECT
    ro.order_id,
    ro.order_number,
    ost.created_at,
    ost.accepted_at,
    ost.repair_started_at,
    ost.completed_at,
    ost.issued_at,
    ost.canceled_at,
    ost.last_status_changed_at,
    ro.problem_description,
    ro.diagnostic_result,
    ro.estimated_cost::NUMERIC(10,2) AS estimated_cost,
    ro.total_cost::NUMERIC(10,2) AS total_cost,
    ro.is_warranty_repair,
    ro.warranty_months,
    ro.notes AS order_notes,

    st.status_name AS order_status,

    c.client_id,
    c.first_name AS client_first_name,
    c.last_name AS client_last_name,
    c.phone AS client_phone,
    c.email AS client_email,

    d.device_id,
    dt.type_name AS device_type,
    d.brand,
    d.model,
    d.serial_number,
    d.equipment_description,
    d.external_condition,

    cb.service_sum,
    cb.part_sum,
    cb.calculated_total,

    op.paid_amount,
    op.remaining_amount,
    op.required_to_close_amount,
    op.payments_count
FROM repair_order ro
JOIN order_status st ON st.status_id = ro.status_id
JOIN device d ON d.device_id = ro.device_id
JOIN client c ON c.client_id = d.client_id
JOIN device_type dt ON dt.device_type_id = d.device_type_id
LEFT JOIN vw_order_status_timestamps ost ON ost.order_id = ro.order_id
LEFT JOIN vw_order_cost_breakdown cb ON cb.order_id = ro.order_id
LEFT JOIN vw_order_payments op ON op.order_id = ro.order_id;


CREATE VIEW vw_technician_workload AS
SELECT
    t.technician_id,
    t.first_name,
    t.last_name,
    t.specialization,
    t.is_active,

    COUNT(os.order_id) AS assigned_service_rows_count,

    COALESCE(SUM(os.quantity), 0) AS assigned_service_quantity,

    COALESCE(
        SUM(os.quantity) FILTER (WHERE os.completed_at IS NOT NULL),
        0
    ) AS completed_service_quantity,

    COALESCE(
        SUM(os.quantity * os.price_at_moment),
        0
    )::NUMERIC(10,2) AS assigned_service_amount,

    COALESCE(
        SUM(os.quantity * os.price_at_moment) FILTER (WHERE os.completed_at IS NOT NULL),
        0
    )::NUMERIC(10,2) AS completed_service_amount,

    MIN(os.completed_at) AS first_completed_at,
    MAX(os.completed_at) AS last_completed_at
FROM technician t
LEFT JOIN order_service os ON os.technician_id = t.technician_id
GROUP BY
    t.technician_id,
    t.first_name,
    t.last_name,
    t.specialization,
    t.is_active;


CREATE OR REPLACE VIEW vw_service_statistics AS
SELECT
    s.service_id,
    s.service_name,
    s.is_active,
    COUNT(os.order_id) AS usage_count,
    COALESCE(SUM(os.quantity), 0) AS total_quantity,
    COALESCE(SUM(os.quantity * os.price_at_moment), 0)::NUMERIC(10,2) AS total_revenue,
    AVG(os.price_at_moment)::NUMERIC(10,2) AS avg_price_at_moment
FROM service s
LEFT JOIN order_service os ON os.service_id = s.service_id
GROUP BY s.service_id, s.service_name, s.is_active;


CREATE OR REPLACE VIEW vw_part_usage_statistics AS
SELECT
    p.part_id,
    p.part_number,
    p.part_name,
    p.manufacturer,
    p.is_active,
    COUNT(osp.part_id) AS usage_count,
    COALESCE(SUM(osp.quantity), 0) AS total_quantity,
    COALESCE(SUM(osp.quantity * osp.price_at_moment), 0)::NUMERIC(10,2) AS total_amount
FROM part p
LEFT JOIN order_service_part osp ON osp.part_id = p.part_id
GROUP BY p.part_id, p.part_number, p.part_name, p.manufacturer, p.is_active;


CREATE OR REPLACE VIEW vw_order_service_part_details AS
SELECT
    ro.order_id,
    ro.order_number,
    os.service_id,
    s.service_name,
    osp.part_id,
    p.part_number,
    p.part_name,
    p.manufacturer,
    osp.quantity,
    osp.price_at_moment::NUMERIC(10,2) AS price_at_moment,
    (osp.quantity * osp.price_at_moment)::NUMERIC(10,2) AS total_amount
FROM order_service_part osp
JOIN order_service os
    ON os.order_id = osp.order_id
   AND os.service_id = osp.service_id
JOIN repair_order ro ON ro.order_id = osp.order_id
JOIN service s ON s.service_id = osp.service_id
JOIN part p ON p.part_id = osp.part_id;


CREATE OR REPLACE VIEW vw_repair_duration AS
SELECT
    ro.order_id,
    ro.order_number,
    ost.created_at,
    ost.accepted_at,
    ost.repair_started_at,
    ost.completed_at,
    ost.issued_at,
    ost.canceled_at,
    os.status_name AS order_status,
    CASE
        WHEN ost.completed_at IS NOT NULL AND ost.repair_started_at IS NOT NULL
            THEN ost.completed_at - ost.repair_started_at
        ELSE NULL
    END AS repair_duration_interval,
    CASE
        WHEN ost.completed_at IS NOT NULL AND ost.repair_started_at IS NOT NULL
            THEN ROUND(EXTRACT(EPOCH FROM (ost.completed_at - ost.repair_started_at)) / 3600.0, 2)
        ELSE NULL
    END AS repair_duration_hours,
    CASE
        WHEN ost.completed_at IS NOT NULL AND ost.repair_started_at IS NOT NULL
            THEN ROUND(EXTRACT(EPOCH FROM (ost.completed_at - ost.repair_started_at)) / 86400.0, 2)
        ELSE NULL
    END AS repair_duration_days,
    CASE
        WHEN ost.issued_at IS NOT NULL AND ost.created_at IS NOT NULL
            THEN ost.issued_at - ost.created_at
        ELSE NULL
    END AS order_lifetime_interval
FROM repair_order ro
JOIN order_status os ON os.status_id = ro.status_id
LEFT JOIN vw_order_status_timestamps ost ON ost.order_id = ro.order_id;


CREATE OR REPLACE VIEW vw_client_order_history AS
SELECT
    c.client_id,
    c.first_name,
    c.last_name,
    c.phone,
    c.email,

    d.device_id,
    dt.type_name AS device_type,
    d.brand,
    d.model,
    d.serial_number,

    ro.order_id,
    ro.order_number,
    ost.created_at,
    ost.completed_at,
    ost.issued_at,
    ost.canceled_at,
    os.status_name AS order_status,
    ro.problem_description,
    ro.total_cost::NUMERIC(10,2) AS total_cost,
    ro.is_warranty_repair,

    COALESCE(SUM(p.amount), 0)::NUMERIC(10,2) AS paid_amount,
    (ro.total_cost - COALESCE(SUM(p.amount), 0))::NUMERIC(10,2) AS remaining_amount
FROM client c
JOIN device d ON d.client_id = c.client_id
JOIN device_type dt ON dt.device_type_id = d.device_type_id
LEFT JOIN repair_order ro ON ro.device_id = d.device_id
LEFT JOIN order_status os ON os.status_id = ro.status_id
LEFT JOIN vw_order_status_timestamps ost ON ost.order_id = ro.order_id
LEFT JOIN payment p ON p.order_id = ro.order_id
GROUP BY
    c.client_id, c.first_name, c.last_name, c.phone, c.email,
    d.device_id, dt.type_name, d.brand, d.model, d.serial_number,
    ro.order_id, ro.order_number, ost.created_at, ost.completed_at, ost.issued_at, ost.canceled_at,
    os.status_name, ro.problem_description, ro.total_cost, ro.is_warranty_repair;

-- =========================================================
-- ФУНКЦИИ
-- =========================================================

CREATE OR REPLACE FUNCTION recalculate_order_total(p_order_id INT)
RETURNS NUMERIC(10,2)
LANGUAGE plpgsql
AS $$
DECLARE
    v_service_sum NUMERIC(10,2);
    v_part_sum NUMERIC(10,2);
    v_total NUMERIC(10,2);
BEGIN
    SELECT COALESCE(SUM(quantity * price_at_moment), 0)::NUMERIC(10,2)
    INTO v_service_sum
    FROM order_service
    WHERE order_id = p_order_id;

    SELECT COALESCE(SUM(quantity * price_at_moment), 0)::NUMERIC(10,2)
    INTO v_part_sum
    FROM order_service_part
    WHERE order_id = p_order_id;

    v_total := COALESCE(v_service_sum, 0) + COALESCE(v_part_sum, 0);

    UPDATE repair_order
    SET total_cost = v_total
    WHERE order_id = p_order_id;

    RETURN v_total;
END;
$$;


-- =========================================================
-- ТРИГГЕР 1. Пересчёт total_cost после изменения услуг
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_recalculate_total_from_order_service()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_id INT;
BEGIN
    v_order_id := COALESCE(NEW.order_id, OLD.order_id);
    PERFORM recalculate_order_total(v_order_id);
    RETURN NULL;
END;
$$;

CREATE TRIGGER trg_recalculate_total_after_order_service_ins
AFTER INSERT ON order_service
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_service();

CREATE TRIGGER trg_recalculate_total_after_order_service_upd
AFTER UPDATE ON order_service
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_service();

CREATE TRIGGER trg_recalculate_total_after_order_service_del
AFTER DELETE ON order_service
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_service();


-- =========================================================
-- ТРИГГЕР 2. Пересчёт total_cost после изменения деталей услуги
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_recalculate_total_from_order_service_part()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_id INT;
BEGIN
    v_order_id := COALESCE(NEW.order_id, OLD.order_id);
    PERFORM recalculate_order_total(v_order_id);
    RETURN NULL;
END;
$$;

CREATE TRIGGER trg_recalculate_total_after_order_service_part_ins
AFTER INSERT ON order_service_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_service_part();

CREATE TRIGGER trg_recalculate_total_after_order_service_part_upd
AFTER UPDATE ON order_service_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_service_part();

CREATE TRIGGER trg_recalculate_total_after_order_service_part_del
AFTER DELETE ON order_service_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_service_part();


-- =========================================================
-- ТРИГГЕР 3. Запрет переплаты
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_prevent_overpayment()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_cost NUMERIC(10,2);
    v_already_paid NUMERIC(10,2);
BEGIN
    SELECT total_cost
    INTO v_total_cost
    FROM repair_order
    WHERE order_id = NEW.order_id;

    IF v_total_cost IS NULL THEN
        RAISE EXCEPTION 'Repair order % not found', NEW.order_id;
    END IF;

    SELECT COALESCE(SUM(amount), 0)
    INTO v_already_paid
    FROM payment
    WHERE order_id = NEW.order_id;

    IF v_already_paid + NEW.amount > v_total_cost THEN
        RAISE EXCEPTION
            'Overpayment is not allowed. Order %, already paid: %, new payment: %, total cost: %',
            NEW.order_id, v_already_paid, NEW.amount, v_total_cost;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_prevent_overpayment
BEFORE INSERT ON payment
FOR EACH ROW
EXECUTE FUNCTION trgfn_prevent_overpayment();


-- =========================================================
-- ТРИГГЕР 4. Запрет изменения услуг/деталей закрытого или отменённого заказа
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_prevent_modify_final_order_lines()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_id INT;
    v_status_name VARCHAR(50);
BEGIN
    v_order_id := COALESCE(NEW.order_id, OLD.order_id);

    SELECT os.status_name
    INTO v_status_name
    FROM repair_order ro
    JOIN order_status os ON os.status_id = ro.status_id
    WHERE ro.order_id = v_order_id;

    IF v_status_name IN ('Closed', 'Canceled') THEN
        RAISE EXCEPTION
            'Cannot modify order lines for order % because its status is %',
            v_order_id, v_status_name;
    END IF;

    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE TRIGGER trg_prevent_modify_final_order_service
BEFORE INSERT OR UPDATE OR DELETE ON order_service
FOR EACH ROW
EXECUTE FUNCTION trgfn_prevent_modify_final_order_lines();

CREATE TRIGGER trg_prevent_modify_final_order_service_part
BEFORE INSERT OR UPDATE OR DELETE ON order_service_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_prevent_modify_final_order_lines();


-- =========================================================
-- ТРИГГЕР 5. Запрет изменения самого закрытого/отменённого заказа
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_prevent_modify_final_repair_order()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_status_name VARCHAR(50);
BEGIN
    SELECT status_name
    INTO v_status_name
    FROM order_status
    WHERE status_id = OLD.status_id;

    IF v_status_name IN ('Closed', 'Canceled') THEN
        RAISE EXCEPTION
            'Cannot modify repair order % because it is in final status %',
            OLD.order_id, v_status_name;
    END IF;

    RETURN COALESCE(NEW, OLD);
END;
$$;

CREATE TRIGGER trg_prevent_modify_final_repair_order
BEFORE UPDATE OR DELETE ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_prevent_modify_final_repair_order();


-- =========================================================
-- ТРИГГЕР 6. Проверка начального статуса заказа
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_validate_initial_repair_order_status()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_status_name VARCHAR(50);
BEGIN
    SELECT status_name
    INTO v_status_name
    FROM order_status
    WHERE status_id = NEW.status_id;

    IF v_status_name IS NULL THEN
        RAISE EXCEPTION 'Unknown initial repair order status id: %', NEW.status_id;
    END IF;

    IF v_status_name <> 'Created' THEN
        RAISE EXCEPTION 'Initial repair order status must be Created, actual status is %', v_status_name;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_validate_initial_repair_order_status
BEFORE INSERT ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_validate_initial_repair_order_status();


-- =========================================================
-- ТРИГГЕР 7. Проверка допустимых переходов статуса заказа
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_validate_repair_order_status_transition()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_old_status VARCHAR(50);
    v_new_status VARCHAR(50);
    v_paid_amount NUMERIC(10,2);
BEGIN
    IF NEW.status_id = OLD.status_id THEN
        RETURN NEW;
    END IF;

    SELECT status_name
    INTO v_old_status
    FROM order_status
    WHERE status_id = OLD.status_id;

    SELECT status_name
    INTO v_new_status
    FROM order_status
    WHERE status_id = NEW.status_id;

    IF v_old_status IS NULL OR v_new_status IS NULL THEN
        RAISE EXCEPTION 'Unknown order status transition: % -> %', OLD.status_id, NEW.status_id;
    END IF;

    IF NOT (
        (v_old_status = 'Created'  AND v_new_status IN ('Accepted', 'Canceled')) OR
        (v_old_status = 'Accepted' AND v_new_status IN ('InRepair', 'Canceled')) OR
        (v_old_status = 'InRepair' AND v_new_status IN ('Ready', 'Canceled')) OR
        (v_old_status = 'Ready'    AND v_new_status IN ('Closed', 'Canceled'))
    ) THEN
        RAISE EXCEPTION 'Invalid repair order status transition: % -> %', v_old_status, v_new_status;
    END IF;

    IF v_new_status = 'Closed' AND NOT NEW.is_warranty_repair THEN
        SELECT COALESCE(SUM(amount), 0)::NUMERIC(10,2)
        INTO v_paid_amount
        FROM payment
        WHERE order_id = NEW.order_id;

        IF v_paid_amount < NEW.total_cost THEN
            RAISE EXCEPTION
                'Cannot close non-warranty order %. Paid: %, total cost: %',
                NEW.order_id, v_paid_amount, NEW.total_cost;
        END IF;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_validate_repair_order_status_transition
BEFORE UPDATE OF status_id ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_validate_repair_order_status_transition();


-- =========================================================
-- ТРИГГЕР 8. Запись истории статусов заказа
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_write_order_status_history()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO order_status_history (order_id, status_id, comment)
        VALUES (NEW.order_id, NEW.status_id, 'Initial status');
        RETURN NEW;
    END IF;

    IF TG_OP = 'UPDATE' AND NEW.status_id <> OLD.status_id THEN
        INSERT INTO order_status_history (order_id, status_id)
        VALUES (NEW.order_id, NEW.status_id);
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_write_initial_order_status_history
AFTER INSERT ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_write_order_status_history();

CREATE TRIGGER trg_write_order_status_history_after_update
AFTER UPDATE OF status_id ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_write_order_status_history();


-- =========================================================
-- ТРИГГЕР 9. PAYMENT = immutable: после вставки запись нельзя менять или удалять
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_payment_no_update_delete()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    RAISE EXCEPTION 'payment records cannot be updated or deleted';
END;
$$;


CREATE TRIGGER trg_payment_no_update
BEFORE UPDATE ON payment
FOR EACH ROW
EXECUTE FUNCTION trgfn_payment_no_update_delete();

CREATE TRIGGER trg_payment_no_delete
BEFORE DELETE ON payment
FOR EACH ROW
EXECUTE FUNCTION trgfn_payment_no_update_delete();
