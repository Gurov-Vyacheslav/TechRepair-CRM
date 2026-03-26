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
        op.order_id,
        COALESCE(SUM(op.quantity * op.price_at_moment), 0)::NUMERIC(10,2) AS part_sum
    FROM order_part op
    GROUP BY op.order_id
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
    ro.total_cost::NUMERIC(10,2) AS total_cost,
    COALESCE(SUM(p.amount), 0)::NUMERIC(10,2) AS paid_amount,
    (ro.total_cost - COALESCE(SUM(p.amount), 0))::NUMERIC(10,2) AS remaining_amount,
    COUNT(p.payment_id) AS payments_count,
    MAX(p.payment_date) AS last_payment_date
FROM repair_order ro
LEFT JOIN payment p ON p.order_id = ro.order_id
GROUP BY ro.order_id, ro.order_number, ro.total_cost;


CREATE OR REPLACE VIEW vw_order_full_info AS
SELECT
    ro.order_id,
    ro.order_number,
    ro.created_at,
    ro.accepted_at,
    ro.completed_at,
    ro.issued_at,
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
    op.payments_count
FROM repair_order ro
JOIN order_status st ON st.status_id = ro.status_id
JOIN device d ON d.device_id = ro.device_id
JOIN client c ON c.client_id = d.client_id
JOIN device_type dt ON dt.device_type_id = d.device_type_id
LEFT JOIN vw_order_cost_breakdown cb ON cb.order_id = ro.order_id
LEFT JOIN vw_order_payments op ON op.order_id = ro.order_id;


CREATE OR REPLACE VIEW vw_active_orders AS
SELECT *
FROM vw_order_full_info
WHERE order_status NOT IN ('Closed', 'Canceled');


CREATE OR REPLACE VIEW vw_technician_workload AS
SELECT
    t.technician_id,
    t.first_name,
    t.last_name,
    t.specialization,
    t.is_active,
    COUNT(os.order_id) AS assigned_services_count,
    COUNT(os.order_id) FILTER (WHERE os.completed_at IS NOT NULL) AS completed_services_count,
    COALESCE(SUM(os.quantity), 0) AS total_service_quantity,
    COALESCE(SUM(os.quantity * os.price_at_moment), 0)::NUMERIC(10,2) AS total_service_amount,
    MIN(os.completed_at) AS first_completed_at,
    MAX(os.completed_at) AS last_completed_at
FROM technician t
LEFT JOIN order_service os ON os.technician_id = t.technician_id
GROUP BY
    t.technician_id, t.first_name, t.last_name, t.specialization, t.is_active;


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
    COUNT(op.order_id) AS usage_count,
    COALESCE(SUM(op.quantity), 0) AS total_quantity,
    COALESCE(SUM(op.quantity * op.price_at_moment), 0)::NUMERIC(10,2) AS total_amount
FROM part p
LEFT JOIN order_part op ON op.part_id = p.part_id
GROUP BY p.part_id, p.part_number, p.part_name, p.manufacturer, p.is_active;

CREATE OR REPLACE VIEW vw_repair_duration AS
SELECT
    ro.order_id,
    ro.order_number,
    ro.created_at,
    ro.accepted_at,
    ro.completed_at,
    ro.issued_at,
    os.status_name AS order_status,
    CASE
        WHEN ro.completed_at IS NOT NULL
            THEN ro.completed_at - ro.created_at
        ELSE NULL
    END AS repair_duration_interval,
    CASE
        WHEN ro.completed_at IS NOT NULL
            THEN ROUND(EXTRACT(EPOCH FROM (ro.completed_at - ro.created_at)) / 3600.0, 2)
        ELSE NULL
    END AS repair_duration_hours,
    CASE
        WHEN ro.completed_at IS NOT NULL
            THEN ROUND(EXTRACT(EPOCH FROM (ro.completed_at - ro.created_at)) / 86400.0, 2)
        ELSE NULL
    END AS repair_duration_days
FROM repair_order ro
JOIN order_status os ON os.status_id = ro.status_id;


CREATE OR REPLACE VIEW vw_unpaid_orders AS
SELECT
    ro.order_id,
    ro.order_number,
    ro.created_at,
    os.status_name AS order_status,

    c.client_id,
    c.first_name AS client_first_name,
    c.last_name AS client_last_name,
    c.phone AS client_phone,

    d.device_id,
    dt.type_name AS device_type,
    d.brand,
    d.model,
    d.serial_number,

    ro.total_cost::NUMERIC(10,2) AS total_cost,
    COALESCE(SUM(p.amount), 0)::NUMERIC(10,2) AS paid_amount,
    (ro.total_cost - COALESCE(SUM(p.amount), 0))::NUMERIC(10,2) AS remaining_amount
FROM repair_order ro
JOIN order_status os ON os.status_id = ro.status_id
JOIN device d ON d.device_id = ro.device_id
JOIN client c ON c.client_id = d.client_id
JOIN device_type dt ON dt.device_type_id = d.device_type_id
LEFT JOIN payment p ON p.order_id = ro.order_id
GROUP BY
    ro.order_id, ro.order_number, ro.created_at, os.status_name,
    c.client_id, c.first_name, c.last_name, c.phone,
    d.device_id, dt.type_name, d.brand, d.model, d.serial_number,
    ro.total_cost
HAVING COALESCE(SUM(p.amount), 0) < ro.total_cost;


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
    ro.created_at,
    ro.completed_at,
    ro.issued_at,
    os.status_name AS order_status,
    ro.problem_description,
    ro.total_cost::NUMERIC(10,2) AS total_cost,

    COALESCE(SUM(p.amount), 0)::NUMERIC(10,2) AS paid_amount,
    (ro.total_cost - COALESCE(SUM(p.amount), 0))::NUMERIC(10,2) AS remaining_amount
FROM client c
JOIN device d ON d.client_id = c.client_id
JOIN device_type dt ON dt.device_type_id = d.device_type_id
LEFT JOIN repair_order ro ON ro.device_id = d.device_id
LEFT JOIN order_status os ON os.status_id = ro.status_id
LEFT JOIN payment p ON p.order_id = ro.order_id
GROUP BY
    c.client_id, c.first_name, c.last_name, c.phone, c.email,
    d.device_id, dt.type_name, d.brand, d.model, d.serial_number,
    ro.order_id, ro.order_number, ro.created_at, ro.completed_at, ro.issued_at,
    os.status_name, ro.problem_description, ro.total_cost;

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
    FROM order_part
    WHERE order_id = p_order_id;

    v_total := COALESCE(v_service_sum, 0) + COALESCE(v_part_sum, 0);

    UPDATE repair_order
    SET total_cost = v_total
    WHERE order_id = p_order_id;

    RETURN v_total;
END;
$$;


CREATE OR REPLACE FUNCTION create_client_with_device(
    p_first_name VARCHAR,
    p_last_name VARCHAR,
    p_phone VARCHAR,
    p_email VARCHAR DEFAULT NULL,
    p_address TEXT DEFAULT NULL,
    p_client_notes TEXT DEFAULT NULL,
    p_device_type_id INT,
    p_brand VARCHAR DEFAULT NULL,
    p_model VARCHAR DEFAULT NULL,
    p_serial_number VARCHAR DEFAULT NULL,
    p_purchase_date DATE DEFAULT NULL,
    p_equipment_description TEXT DEFAULT NULL,
    p_external_condition TEXT DEFAULT NULL,
    p_device_notes TEXT DEFAULT NULL
)
RETURNS TABLE(client_id INT, device_id INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_client_id INT;
    v_device_id INT;
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM device_type
        WHERE device_type_id = p_device_type_id
    ) THEN
        RAISE EXCEPTION 'Device type with id % does not exist', p_device_type_id;
    END IF;

    INSERT INTO client (
        first_name, last_name, phone, email, address, notes
    )
    VALUES (
        p_first_name, p_last_name, p_phone, p_email, p_address, p_client_notes
    )
    RETURNING client.client_id INTO v_client_id;

    INSERT INTO device (
        client_id, device_type_id, brand, model, serial_number,
        purchase_date, equipment_description, external_condition, notes
    )
    VALUES (
        v_client_id, p_device_type_id, p_brand, p_model, p_serial_number,
        p_purchase_date, p_equipment_description, p_external_condition, p_device_notes
    )
    RETURNING device.device_id INTO v_device_id;

    RETURN QUERY
    SELECT v_client_id, v_device_id;
END;
$$;


CREATE OR REPLACE FUNCTION create_repair_order(
    p_device_id INT,
    p_problem_description TEXT,
    p_estimated_cost NUMERIC(10,2) DEFAULT 0,
    p_warranty_months SMALLINT DEFAULT NULL,
    p_is_warranty_repair BOOLEAN DEFAULT FALSE,
    p_diagnostic_result TEXT DEFAULT NULL,
    p_notes TEXT DEFAULT NULL
)
RETURNS TABLE(order_id INT, order_number VARCHAR)
LANGUAGE plpgsql
AS $$
DECLARE
    v_order_id INT;
    v_order_number VARCHAR(30);
    v_created_status_id SMALLINT;
BEGIN
    SELECT status_id
    INTO v_created_status_id
    FROM order_status
    WHERE status_name = 'Created';

    IF v_created_status_id IS NULL THEN
        RAISE EXCEPTION 'Status "Created" not found in order_status';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM device
        WHERE device_id = p_device_id
    ) THEN
        RAISE EXCEPTION 'Device with id % does not exist', p_device_id;
    END IF;

    v_order_id := nextval(pg_get_serial_sequence('repair_order', 'order_id'));
    v_order_number := 'RO-' || TO_CHAR(CURRENT_DATE, 'YYYYMMDD') || '-' || LPAD(v_order_id::TEXT, 6, '0');

    INSERT INTO repair_order (
        order_id,
        order_number,
        device_id,
        status_id,
        problem_description,
        estimated_cost,
        warranty_months,
        is_warranty_repair,
        diagnostic_result,
        notes
    )
    VALUES (
        v_order_id,
        v_order_number,
        p_device_id,
        v_created_status_id,
        p_problem_description,
        COALESCE(p_estimated_cost, 0),
        p_warranty_months,
        COALESCE(p_is_warranty_repair, FALSE),
        p_diagnostic_result,
        p_notes
    );

    RETURN QUERY
    SELECT v_order_id, v_order_number;
END;
$$;



CREATE OR REPLACE FUNCTION add_service_to_order(
    p_order_id INT,
    p_service_id INT,
    p_quantity SMALLINT DEFAULT 1,
    p_price_at_moment NUMERIC(10,2) DEFAULT NULL,
    p_technician_id INT DEFAULT NULL,
    p_completed_at TIMESTAMP DEFAULT NULL,
    p_notes TEXT DEFAULT NULL
)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_price NUMERIC(10,2);
BEGIN
    SELECT COALESCE(p_price_at_moment, s.base_price)
    INTO v_price
    FROM service s
    WHERE s.service_id = p_service_id
      AND s.is_active = TRUE;

    IF v_price IS NULL THEN
        RAISE EXCEPTION 'Active service with id % not found or no price available', p_service_id;
    END IF;

    INSERT INTO order_service (
        order_id,
        service_id,
        technician_id,
        quantity,
        price_at_moment,
        completed_at,
        notes
    )
    VALUES (
        p_order_id,
        p_service_id,
        p_technician_id,
        p_quantity,
        v_price,
        p_completed_at,
        p_notes
    );
END;
$$;


CREATE OR REPLACE FUNCTION add_part_to_order(
    p_order_id INT,
    p_part_id INT,
    p_quantity INT,
    p_price_at_moment NUMERIC(10,2) DEFAULT NULL
)
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_price NUMERIC(10,2);
BEGIN
    SELECT COALESCE(p_price_at_moment, p.default_price)
    INTO v_price
    FROM part p
    WHERE p.part_id = p_part_id
      AND p.is_active = TRUE;

    IF v_price IS NULL THEN
        RAISE EXCEPTION 'Active part with id % not found or no price available', p_part_id;
    END IF;

    INSERT INTO order_part (
        order_id,
        part_id,
        quantity,
        price_at_moment
    )
    VALUES (
        p_order_id,
        p_part_id,
        p_quantity,
        v_price
    );
END;
$$;


CREATE OR REPLACE FUNCTION register_payment(
    p_order_id INT,
    p_amount NUMERIC(10,2),
    p_payment_method VARCHAR,
    p_transaction_number VARCHAR DEFAULT NULL,
    p_notes TEXT DEFAULT NULL,
    p_payment_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_payment_id INT;
BEGIN
    INSERT INTO payment (
        order_id,
        payment_date,
        amount,
        payment_method,
        transaction_number,
        notes
    )
    VALUES (
        p_order_id,
        COALESCE(p_payment_date, CURRENT_TIMESTAMP),
        p_amount,
        p_payment_method,
        p_transaction_number,
        p_notes
    )
    RETURNING payment_id INTO v_payment_id;

    RETURN v_payment_id;
END;
$$;


-- =========================================================
-- ТРИГГЕР 1. Начальная запись в историю статусов
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_repair_order_initial_history()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO order_status_history (
        order_id,
        status_id,
        changed_at,
        comment
    )
    VALUES (
        NEW.order_id,
        NEW.status_id,
        NEW.created_at,
        'Initial status'
    );

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_repair_order_initial_history
AFTER INSERT ON repair_order
FOR EACH ROW
EXECUTE FUNCTION trgfn_repair_order_initial_history();


-- =========================================================
-- ТРИГГЕР 2. Запись смены статуса в историю
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_repair_order_status_history()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    IF NEW.status_id IS DISTINCT FROM OLD.status_id THEN
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
            NULL
        );
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_repair_order_status_history
AFTER UPDATE OF status_id ON repair_order
FOR EACH ROW
WHEN (OLD.status_id IS DISTINCT FROM NEW.status_id)
EXECUTE FUNCTION trgfn_repair_order_status_history();


-- =========================================================
-- ТРИГГЕР 3. Пересчёт total_cost после изменения услуг
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
-- ТРИГГЕР 4. Пересчёт total_cost после изменения запчастей
-- =========================================================

CREATE OR REPLACE FUNCTION trgfn_recalculate_total_from_order_part()
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

CREATE TRIGGER trg_recalculate_total_after_order_part_ins
AFTER INSERT ON order_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_part();

CREATE TRIGGER trg_recalculate_total_after_order_part_upd
AFTER UPDATE ON order_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_part();

CREATE TRIGGER trg_recalculate_total_after_order_part_del
AFTER DELETE ON order_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_recalculate_total_from_order_part();


-- =========================================================
-- ТРИГГЕР 5. Запрет переплаты
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

    IF TG_OP = 'UPDATE' THEN
        SELECT COALESCE(SUM(amount), 0)
        INTO v_already_paid
        FROM payment
        WHERE order_id = NEW.order_id
          AND payment_id <> OLD.payment_id;
    ELSE
        SELECT COALESCE(SUM(amount), 0)
        INTO v_already_paid
        FROM payment
        WHERE order_id = NEW.order_id;
    END IF;

    IF v_already_paid + NEW.amount > v_total_cost THEN
        RAISE EXCEPTION
            'Overpayment is not allowed. Order %, already paid: %, new payment: %, total cost: %',
            NEW.order_id, v_already_paid, NEW.amount, v_total_cost;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_prevent_overpayment
BEFORE INSERT OR UPDATE ON payment
FOR EACH ROW
EXECUTE FUNCTION trgfn_prevent_overpayment();


-- =========================================================
-- ТРИГГЕР 6. Запрет изменения услуг/деталей закрытого заказа
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

CREATE TRIGGER trg_prevent_modify_final_order_part
BEFORE INSERT OR UPDATE OR DELETE ON order_part
FOR EACH ROW
EXECUTE FUNCTION trgfn_prevent_modify_final_order_lines();


-- =========================================================
-- ТРИГГЕР 7. Запрет изменения самого закрытого/отменённого заказа
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
-- 1. ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
-- =========================================================

CREATE OR REPLACE FUNCTION has_order_services(p_order_id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_exists BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM order_service
        WHERE order_id = p_order_id
    )
    INTO v_exists;

    RETURN v_exists;
END;
$$;


CREATE OR REPLACE FUNCTION are_all_order_services_completed(p_order_id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_has_services BOOLEAN;
    v_all_completed BOOLEAN;
BEGIN
    v_has_services := has_order_services(p_order_id);

    IF NOT v_has_services THEN
        RETURN FALSE;
    END IF;

    SELECT NOT EXISTS (
        SELECT 1
        FROM order_service
        WHERE order_id = p_order_id
          AND completed_at IS NULL
    )
    INTO v_all_completed;

    RETURN v_all_completed;
END;
$$;


CREATE OR REPLACE FUNCTION is_order_fully_paid(p_order_id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_cost NUMERIC(10,2);
    v_paid_amount NUMERIC(10,2);
BEGIN
    SELECT total_cost
    INTO v_total_cost
    FROM repair_order
    WHERE order_id = p_order_id;

    IF v_total_cost IS NULL THEN
        RAISE EXCEPTION 'Repair order % not found', p_order_id;
    END IF;

    SELECT COALESCE(SUM(amount), 0)::NUMERIC(10,2)
    INTO v_paid_amount
    FROM payment
    WHERE order_id = p_order_id;

    RETURN v_paid_amount >= v_total_cost;
END;
$$;


-- =========================================================
-- 2. СМЕНА СТАТУСА ЗАКАЗА
-- =========================================================

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
    v_last_history_id INT;
BEGIN
    -- Текущий статус заказа
    SELECT os.status_name
    INTO v_current_status_name
    FROM repair_order ro
    JOIN order_status os ON os.status_id = ro.status_id
    WHERE ro.order_id = p_order_id
    FOR UPDATE;

    IF v_current_status_name IS NULL THEN
        RAISE EXCEPTION 'Repair order % not found', p_order_id;
    END IF;

    -- Новый статус
    SELECT status_id
    INTO v_new_status_id
    FROM order_status
    WHERE status_name = p_new_status_name;

    IF v_new_status_id IS NULL THEN
        RAISE EXCEPTION 'Status "%" not found in order_status', p_new_status_name;
    END IF;

    -- Ничего не делать, если статус тот же
    IF v_current_status_name = p_new_status_name THEN
        RETURN;
    END IF;

    -- Финальные статусы нельзя менять
    IF v_current_status_name IN ('Closed', 'Canceled') THEN
        RAISE EXCEPTION
            'Repair order % is already in final status: %',
            p_order_id, v_current_status_name;
    END IF;

    -- Допустимые переходы
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

    -- Проверки по переходам
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

    -- Обновление заказа
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

    -- Триггер сам создаст запись в history.
    -- Если комментарий передан, добавим его в последнюю запись истории.
    IF p_comment IS NOT NULL THEN
        SELECT history_id
        INTO v_last_history_id
        FROM order_status_history
        WHERE order_id = p_order_id
        ORDER BY changed_at DESC, history_id DESC
        LIMIT 1;

        IF v_last_history_id IS NOT NULL THEN
            UPDATE order_status_history
            SET comment = p_comment
            WHERE history_id = v_last_history_id;
        END IF;
    END IF;
END;
$$;