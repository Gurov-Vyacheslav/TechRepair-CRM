BEGIN;

DROP TABLE IF EXISTS test_ctx;

CREATE TEMP TABLE test_ctx (
    suffix          TEXT NOT NULL,
    device_type_id  INT,
    client_id       INT,
    device_id       INT,
    order_id        INT,
    technician_id   INT,
    service_id      INT,
    part_id         INT
);

INSERT INTO test_ctx (suffix)
VALUES (SUBSTRING(MD5(random()::TEXT || clock_timestamp()::TEXT), 1, 8));

-- =========================================================
-- 1. БАЗОВЫЕ ДАННЫЕ
-- =========================================================

WITH inserted AS (
    INSERT INTO device_type (type_name)
    SELECT 'TEST_LAPTOP_' || suffix
    FROM test_ctx
    RETURNING device_type_id
)
UPDATE test_ctx
SET device_type_id = (SELECT device_type_id FROM inserted);

WITH inserted AS (
    INSERT INTO technician (
        first_name,
        last_name,
        email,
        phone,
        specialization,
        is_active,
        notes
    )
    SELECT
        'Тест',
        'Мастер',
        'test.tech.' || suffix || '@example.com',
        '7900' || suffix,
        'Laptop repair',
        TRUE,
        'Тестовый мастер'
    FROM test_ctx
    RETURNING technician_id
)
UPDATE test_ctx
SET technician_id = (SELECT technician_id FROM inserted);

WITH inserted AS (
    INSERT INTO service (
        service_name,
        description,
        base_price,
        estimated_duration,
        is_active
    )
    SELECT
        'TEST_DIAGNOSTICS_' || suffix,
        'Тестовая диагностика',
        1200.00,
        60,
        TRUE
    FROM test_ctx
    RETURNING service_id
)
UPDATE test_ctx
SET service_id = (SELECT service_id FROM inserted);

WITH inserted AS (
    INSERT INTO part (
        part_number,
        part_name,
        manufacturer,
        default_price,
        is_active,
        description
    )
    SELECT
        'TEST-PART-' || suffix,
        'Тестовая деталь',
        'TestManufacturer',
        350.00,
        TRUE,
        'Тестовая запчасть'
    FROM test_ctx
    RETURNING part_id
)
UPDATE test_ctx
SET part_id = (SELECT part_id FROM inserted);

-- =========================================================
-- 2. СОЗДАНИЕ КЛИЕНТА, УСТРОЙСТВА И ЗАКАЗА НАПРЯМУЮ
-- =========================================================

WITH inserted AS (
    INSERT INTO client (
        first_name,
        last_name,
        phone,
        email,
        address,
        notes
    )
    SELECT
        'Иван',
        'Тестов',
        '7901' || suffix,
        'ivan.test.' || suffix || '@example.com',
        'Москва, тестовый адрес',
        'Тестовый клиент'
    FROM test_ctx
    RETURNING client_id
)
UPDATE test_ctx
SET client_id = (SELECT client_id FROM inserted);

WITH inserted AS (
    INSERT INTO device (
        client_id,
        device_type_id,
        brand,
        model,
        serial_number,
        purchase_date,
        equipment_description,
        external_condition,
        notes
    )
    SELECT
        client_id,
        device_type_id,
        'Lenovo',
        'ThinkPad',
        'TEST-SN-' || suffix,
        DATE '2024-01-10',
        'Зарядка, чехол',
        'Есть мелкие царапины',
        'Тестовое устройство'
    FROM test_ctx
    RETURNING device_id
)
UPDATE test_ctx
SET device_id = (SELECT device_id FROM inserted);

WITH inserted AS (
    INSERT INTO repair_order (
        order_number,
        device_id,
        status_id,
        problem_description,
        estimated_cost,
        warranty_months,
        is_warranty_repair,
        notes
    )
    SELECT
        'TEST-ORD-' || suffix,
        device_id,
        (SELECT status_id FROM order_status WHERE status_name = 'Created'),
        'Ноутбук не включается',
        1500.00,
        0,
        FALSE,
        'Первичная заявка'
    FROM test_ctx
    RETURNING order_id
)
UPDATE test_ctx
SET order_id = (SELECT order_id FROM inserted);

DO $$
DECLARE
    v_history_count INT;
    v_status_name VARCHAR(50);
BEGIN
    SELECT os.status_name
    INTO v_status_name
    FROM repair_order ro
    JOIN order_status os ON os.status_id = ro.status_id
    WHERE ro.order_id = (SELECT order_id FROM test_ctx);

    IF v_status_name <> 'Created' THEN
        RAISE EXCEPTION 'FAIL: initial order status must be Created, got %', v_status_name;
    END IF;

    SELECT COUNT(*)
    INTO v_history_count
    FROM order_status_history
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_history_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: initial status history row was not created';
    END IF;

    RAISE NOTICE 'OK: repair order created and initial status history written';
END $$;

-- =========================================================
-- 3. ПЕРЕСЧЁТ total_cost ПОСЛЕ ДОБАВЛЕНИЯ УСЛУГИ И ДЕТАЛИ
-- =========================================================

INSERT INTO order_service (
    order_id,
    service_id,
    technician_id,
    quantity,
    price_at_moment,
    notes
)
SELECT
    order_id,
    service_id,
    technician_id,
    1,
    1200.00,
    'Диагностика назначена'
FROM test_ctx;

INSERT INTO order_part (
    order_id,
    part_id,
    quantity,
    price_at_moment
)
SELECT
    order_id,
    part_id,
    1,
    350.00
FROM test_ctx;

DO $$
DECLARE
    v_total NUMERIC(10,2);
BEGIN
    SELECT total_cost
    INTO v_total
    FROM repair_order
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_total <> 1550.00 THEN
        RAISE EXCEPTION 'FAIL: total_cost must be 1550.00, got %', v_total;
    END IF;

    RAISE NOTICE 'OK: total_cost recalculated after adding service and part';
END $$;

-- =========================================================
-- 4. СМЕНА СТАТУСОВ И ЗАПИСЬ ИСТОРИИ ВРУЧНУЮ
-- =========================================================

UPDATE repair_order
SET status_id = (SELECT status_id FROM order_status WHERE status_name = 'Accepted'),
    accepted_at = CURRENT_TIMESTAMP,
    notes = 'Заказ принят'
WHERE order_id = (SELECT order_id FROM test_ctx);

INSERT INTO order_status_history (order_id, status_id, comment)
SELECT
    order_id,
    (SELECT status_id FROM order_status WHERE status_name = 'Accepted'),
    'Заказ принят'
FROM test_ctx;

UPDATE repair_order
SET status_id = (SELECT status_id FROM order_status WHERE status_name = 'InRepair'),
    notes = 'Ремонт начат'
WHERE order_id = (SELECT order_id FROM test_ctx);

INSERT INTO order_status_history (order_id, status_id, comment)
SELECT
    order_id,
    (SELECT status_id FROM order_status WHERE status_name = 'InRepair'),
    'Ремонт начат'
FROM test_ctx;

UPDATE order_service
SET completed_at = CURRENT_TIMESTAMP,
    notes = 'Диагностика завершена'
WHERE order_id = (SELECT order_id FROM test_ctx)
  AND service_id = (SELECT service_id FROM test_ctx);

UPDATE repair_order
SET status_id = (SELECT status_id FROM order_status WHERE status_name = 'Ready'),
    completed_at = CURRENT_TIMESTAMP,
    diagnostic_result = 'Неисправность устранена'
WHERE order_id = (SELECT order_id FROM test_ctx);

INSERT INTO order_status_history (order_id, status_id, comment)
SELECT
    order_id,
    (SELECT status_id FROM order_status WHERE status_name = 'Ready'),
    'Работы завершены'
FROM test_ctx;

DO $$
DECLARE
    v_history_count INT;
BEGIN
    SELECT COUNT(*)
    INTO v_history_count
    FROM order_status_history
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_history_count <> 4 THEN
        RAISE EXCEPTION 'FAIL: expected 4 history rows after manual status changes, got %', v_history_count;
    END IF;

    RAISE NOTICE 'OK: manual status history records added';
END $$;

-- =========================================================
-- 5. ОПЛАТЫ: ЗАПРЕТ ПЕРЕПЛАТЫ И НЕИЗМЕНЯЕМОСТЬ ПЛАТЕЖЕЙ
-- =========================================================

INSERT INTO payment (
    order_id,
    amount,
    payment_method,
    transaction_number,
    notes
)
SELECT
    order_id,
    1000.00,
    'Card',
    'TEST-TRX-1-' || suffix,
    'Частичная оплата'
FROM test_ctx;

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        INSERT INTO payment (
            order_id,
            amount,
            payment_method,
            transaction_number,
            notes
        )
        SELECT
            order_id,
            600.00,
            'Cash',
            NULL,
            'Попытка переплаты'
        FROM test_ctx;
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: overpayment blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: overpayment must be blocked';
    END IF;
END $$;

DO $$
DECLARE
    v_payment_id INT;
    v_failed BOOLEAN := FALSE;
BEGIN
    SELECT payment_id
    INTO v_payment_id
    FROM payment
    WHERE order_id = (SELECT order_id FROM test_ctx)
    ORDER BY payment_date
    LIMIT 1;

    BEGIN
        UPDATE payment
        SET amount = 999.99
        WHERE payment_id = v_payment_id;
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: payment update blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: payment update must be blocked';
    END IF;
END $$;

DO $$
DECLARE
    v_payment_id INT;
    v_failed BOOLEAN := FALSE;
BEGIN
    SELECT payment_id
    INTO v_payment_id
    FROM payment
    WHERE order_id = (SELECT order_id FROM test_ctx)
    ORDER BY payment_date
    LIMIT 1;

    BEGIN
        DELETE FROM payment
        WHERE payment_id = v_payment_id;
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: payment delete blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: payment delete must be blocked';
    END IF;
END $$;

INSERT INTO payment (
    order_id,
    amount,
    payment_method,
    transaction_number,
    notes
)
SELECT
    order_id,
    550.00,
    'Transfer',
    'TEST-TRX-2-' || suffix,
    'Окончательная оплата'
FROM test_ctx;

-- =========================================================
-- 6. ЗАКРЫТИЕ ЗАКАЗА И ЗАПРЕТ ИЗМЕНЕНИЙ ПОСЛЕ ФИНАЛЬНОГО СТАТУСА
-- =========================================================

UPDATE repair_order
SET status_id = (SELECT status_id FROM order_status WHERE status_name = 'Closed'),
    issued_at = CURRENT_TIMESTAMP,
    notes = 'Заказ закрыт после полной оплаты'
WHERE order_id = (SELECT order_id FROM test_ctx);

INSERT INTO order_status_history (order_id, status_id, comment)
SELECT
    order_id,
    (SELECT status_id FROM order_status WHERE status_name = 'Closed'),
    'Заказ закрыт после полной оплаты'
FROM test_ctx;

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        UPDATE order_service
        SET notes = 'Попытка изменить закрытый заказ'
        WHERE order_id = (SELECT order_id FROM test_ctx)
          AND service_id = (SELECT service_id FROM test_ctx);
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: modification of order_service for closed order blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: modifying order_service of closed order must fail';
    END IF;
END $$;

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        UPDATE repair_order
        SET notes = 'Попытка изменить закрытый заказ'
        WHERE order_id = (SELECT order_id FROM test_ctx);
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: modification of closed repair_order blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: modifying closed repair_order must fail';
    END IF;
END $$;

-- =========================================================
-- 7. ORDER_STATUS_HISTORY = append-only
-- =========================================================

DO $$
DECLARE
    v_history_id INT;
    v_failed BOOLEAN := FALSE;
BEGIN
    SELECT history_id
    INTO v_history_id
    FROM order_status_history
    WHERE order_id = (SELECT order_id FROM test_ctx)
    ORDER BY changed_at
    LIMIT 1;

    BEGIN
        UPDATE order_status_history
        SET comment = 'Попытка изменить историю'
        WHERE history_id = v_history_id;
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: order_status_history update blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: order_status_history update must be blocked';
    END IF;
END $$;

DO $$
DECLARE
    v_history_id INT;
    v_failed BOOLEAN := FALSE;
BEGIN
    SELECT history_id
    INTO v_history_id
    FROM order_status_history
    WHERE order_id = (SELECT order_id FROM test_ctx)
    ORDER BY changed_at
    LIMIT 1;

    BEGIN
        DELETE FROM order_status_history
        WHERE history_id = v_history_id;
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: order_status_history delete blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: order_status_history delete must be blocked';
    END IF;
END $$;

-- =========================================================
-- 8. ПРОВЕРКА ПРЕДСТАВЛЕНИЙ БЕЗ ВЫВОДА ТАБЛИЦ
-- =========================================================

DO $$
DECLARE
    v_count INT;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM vw_order_full_info
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_order_full_info must contain exactly 1 row for test order';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_order_cost_breakdown
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_order_cost_breakdown must contain exactly 1 row for test order';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_order_payments
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_order_payments must contain exactly 1 row for test order';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_client_order_history
    WHERE client_id = (SELECT client_id FROM test_ctx);

    IF v_count < 1 THEN
        RAISE EXCEPTION 'FAIL: vw_client_order_history must contain at least 1 row for test client';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_repair_duration
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_repair_duration must contain exactly 1 row for test order';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_technician_workload
    WHERE technician_id = (SELECT technician_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_technician_workload must contain exactly 1 row for test technician';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_service_statistics
    WHERE service_id = (SELECT service_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_service_statistics must contain exactly 1 row for test service';
    END IF;

    SELECT COUNT(*) INTO v_count
    FROM vw_part_usage_statistics
    WHERE part_id = (SELECT part_id FROM test_ctx);

    IF v_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: vw_part_usage_statistics must contain exactly 1 row for test part';
    END IF;

    RAISE NOTICE 'OK: views passed validation checks';
END $$;

ROLLBACK;
