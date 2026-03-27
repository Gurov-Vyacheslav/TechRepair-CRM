BEGIN;

DROP TABLE IF EXISTS test_ctx;

CREATE TEMP TABLE test_ctx (
    client_id       INT,
    device_id       INT,
    order_id        INT,
    order_number    VARCHAR(30),
    technician_id   INT,
    service_id      INT,
    part_id         INT
);

INSERT INTO test_ctx DEFAULT VALUES;

-- =========================================================
-- 1. БАЗОВЫЕ ДАННЫЕ
-- =========================================================

INSERT INTO device_type (type_name)
VALUES ('TEST_LAPTOP')
ON CONFLICT (type_name) DO NOTHING;

INSERT INTO technician (
    first_name,
    last_name,
    email,
    phone,
    specialization,
    is_active,
    notes
)
VALUES (
    'Тест',
    'Мастер',
    'test.tech@example.com',
    '79000000011',
    'Laptop repair',
    TRUE,
    'Тестовый мастер'
)
ON CONFLICT (email) DO NOTHING;

UPDATE test_ctx
SET technician_id = (
    SELECT technician_id
    FROM technician
    WHERE email = 'test.tech@example.com'
);

INSERT INTO service (
    service_name,
    description,
    base_price,
    estimated_duration,
    is_active
)
VALUES (
    'TEST_DIAGNOSTICS',
    'Тестовая диагностика',
    1200.00,
    60,
    TRUE
)
ON CONFLICT (service_name) DO NOTHING;

UPDATE test_ctx
SET service_id = (
    SELECT service_id
    FROM service
    WHERE service_name = 'TEST_DIAGNOSTICS'
);

INSERT INTO part (
    part_number,
    part_name,
    manufacturer,
    default_price,
    is_active,
    description
)
VALUES (
    'TEST-PART-001',
    'Тестовая деталь',
    'TestManufacturer',
    350.00,
    TRUE,
    'Тестовая запчасть'
)
ON CONFLICT (part_number) DO NOTHING;

UPDATE test_ctx
SET part_id = (
    SELECT part_id
    FROM part
    WHERE part_number = 'TEST-PART-001'
);

-- =========================================================
-- 2. СОЗДАНИЕ КЛИЕНТА И УСТРОЙСТВА
-- =========================================================

WITH created AS (
    SELECT *
    FROM create_client_with_device(
        'Иван'::VARCHAR,
        'Тестов'::VARCHAR,
        '79000000001'::VARCHAR,
        (SELECT device_type_id FROM device_type WHERE type_name = 'TEST_LAPTOP')::INT,
        'ivan.test@example.com'::VARCHAR,
        'Москва, тестовый адрес'::TEXT,
        'Тестовый клиент'::TEXT,
        'Lenovo'::VARCHAR,
        'ThinkPad'::VARCHAR,
        'TEST-SN-001'::VARCHAR,
        DATE '2024-01-10',
        'Зарядка, чехол'::TEXT,
        'Есть мелкие царапины'::TEXT,
        'Тестовое устройство'::TEXT
    )
)
UPDATE test_ctx t
SET
    client_id = c.client_id,
    device_id = c.device_id
FROM created c;

DO $$
DECLARE
    v_client_count INT;
    v_device_count INT;
BEGIN
    SELECT COUNT(*) INTO v_client_count
    FROM client
    WHERE phone = '79000000001';

    SELECT COUNT(*) INTO v_device_count
    FROM device
    WHERE serial_number = 'TEST-SN-001';

    IF v_client_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: client was not created correctly';
    END IF;

    IF v_device_count <> 1 THEN
        RAISE EXCEPTION 'FAIL: device was not created correctly';
    END IF;

    RAISE NOTICE 'OK: client and device created';
END $$;

-- =========================================================
-- 3. СОЗДАНИЕ ЗАКАЗА
-- =========================================================

WITH created_order AS (
    SELECT *
    FROM create_repair_order(
        (SELECT device_id FROM test_ctx)::INT,
        'Ноутбук не включается'::TEXT,
        1500.00::NUMERIC(10,2),
        0::SMALLINT,
        FALSE,
        NULL::TEXT,
        'Первичная заявка'::TEXT
    )
)
UPDATE test_ctx t
SET
    order_id = o.order_id,
    order_number = o.order_number
FROM created_order o;

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
-- 3A. НОВЫЕ ЗАЩИТЫ: direct update repair_order запрещён
-- =========================================================

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        UPDATE repair_order
        SET order_number = 'HACK-ORDER-NUMBER'
        WHERE order_id = (SELECT order_id FROM test_ctx);
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: direct update of order_number blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: direct update of order_number must fail';
    END IF;
END $$;

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        UPDATE repair_order
        SET created_at = CURRENT_TIMESTAMP + INTERVAL '1 day'
        WHERE order_id = (SELECT order_id FROM test_ctx);
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: direct update of created_at blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: direct update of created_at must fail';
    END IF;
END $$;

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
    v_accepted_status SMALLINT;
BEGIN
    SELECT status_id INTO v_accepted_status
    FROM order_status
    WHERE status_name = 'Accepted';

    BEGIN
        UPDATE repair_order
        SET status_id = v_accepted_status
        WHERE order_id = (SELECT order_id FROM test_ctx);
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: direct update of status_id blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: direct update of status_id must fail';
    END IF;
END $$;

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        UPDATE repair_order
        SET accepted_at = CURRENT_TIMESTAMP
        WHERE order_id = (SELECT order_id FROM test_ctx);
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: direct update of accepted_at blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: direct update of accepted_at must fail';
    END IF;
END $$;

-- =========================================================
-- 4. НЕГАТИВ: нельзя InRepair без услуг
-- =========================================================

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        PERFORM change_repair_order_status(
            (SELECT order_id FROM test_ctx)::INT,
            'InRepair'::VARCHAR,
            'Попытка перевода без услуг'::TEXT
        );
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: transition Created -> InRepair without services blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: transition Created -> InRepair without services must fail';
    END IF;
END $$;

-- =========================================================
-- 5. ДОБАВЛЯЕМ УСЛУГУ И ДЕТАЛЬ
-- =========================================================

SELECT add_service_to_order(
    (SELECT order_id FROM test_ctx)::INT,
    (SELECT service_id FROM test_ctx)::INT,
    1::SMALLINT,
    NULL::NUMERIC(10,2),
    (SELECT technician_id FROM test_ctx)::INT,
    NULL::TIMESTAMP,
    'Диагностика назначена'::TEXT
);

SELECT add_part_to_order(
    (SELECT order_id FROM test_ctx)::INT,
    (SELECT part_id FROM test_ctx)::INT,
    1::INT,
    NULL::NUMERIC(10,2)
);

DO $$
DECLARE
    v_total NUMERIC(10,2);
BEGIN
    SELECT total_cost
    INTO v_total
    FROM repair_order
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_total <> 1550.00 THEN
        RAISE EXCEPTION 'FAIL: total_cost must be 1550.00 after adding service and part, got %', v_total;
    END IF;

    RAISE NOTICE 'OK: total_cost recalculated after adding service and part';
END $$;

-- =========================================================
-- 6. Created -> Accepted -> InRepair
-- =========================================================

SELECT change_repair_order_status(
    (SELECT order_id FROM test_ctx)::INT,
    'Accepted'::VARCHAR,
    'Заказ принят'::TEXT
);

SELECT change_repair_order_status(
    (SELECT order_id FROM test_ctx)::INT,
    'InRepair'::VARCHAR,
    'Ремонт начат'::TEXT
);

DO $$
DECLARE
    v_status_name VARCHAR(50);
    v_history_count INT;
BEGIN
    SELECT os.status_name
    INTO v_status_name
    FROM repair_order ro
    JOIN order_status os ON os.status_id = ro.status_id
    WHERE ro.order_id = (SELECT order_id FROM test_ctx);

    IF v_status_name <> 'InRepair' THEN
        RAISE EXCEPTION 'FAIL: order status must be InRepair, got %', v_status_name;
    END IF;

    SELECT COUNT(*)
    INTO v_history_count
    FROM order_status_history
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_history_count <> 3 THEN
        RAISE EXCEPTION 'FAIL: expected 3 history rows after Created->Accepted->InRepair, got %', v_history_count;
    END IF;

    RAISE NOTICE 'OK: status transitions Created -> Accepted -> InRepair work';
END $$;

-- =========================================================
-- 7. НЕГАТИВ: нельзя Ready, пока услуга не завершена
-- =========================================================

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        PERFORM change_repair_order_status(
            (SELECT order_id FROM test_ctx)::INT,
            'Ready'::VARCHAR,
            'Попытка без завершения работ'::TEXT
        );
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: transition InRepair -> Ready before service completion blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: transition InRepair -> Ready before service completion must fail';
    END IF;
END $$;

-- =========================================================
-- 8. ЗАВЕРШАЕМ УСЛУГУ И ПЕРЕВОДИМ В Ready
-- =========================================================

UPDATE order_service
SET completed_at = CURRENT_TIMESTAMP,
    notes = 'Диагностика завершена'
WHERE order_id = (SELECT order_id FROM test_ctx)
  AND service_id = (SELECT service_id FROM test_ctx);

SELECT change_repair_order_status(
    (SELECT order_id FROM test_ctx)::INT,
    'Ready'::VARCHAR,
    'Работы завершены'::TEXT
);

-- =========================================================
-- 9. НЕГАТИВ: нельзя Closed без полной оплаты
-- =========================================================

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        PERFORM change_repair_order_status(
            (SELECT order_id FROM test_ctx)::INT,
            'Closed'::VARCHAR,
            'Попытка закрытия без оплаты'::TEXT
        );
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: transition Ready -> Closed without full payment blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: transition Ready -> Closed without full payment must fail';
    END IF;
END $$;

-- =========================================================
-- 10. ЧАСТИЧНАЯ ОПЛАТА
-- =========================================================

SELECT register_payment(
    (SELECT order_id FROM test_ctx)::INT,
    1000.00::NUMERIC(10,2),
    'Card'::VARCHAR,
    'TEST-TRX-001'::VARCHAR,
    'Частичная оплата'::TEXT
);

-- =========================================================
-- 11. НЕГАТИВ: переплата запрещена
-- =========================================================

DO $$
DECLARE
    v_failed BOOLEAN := FALSE;
BEGIN
    BEGIN
        PERFORM register_payment(
            (SELECT order_id FROM test_ctx)::INT,
            600.00::NUMERIC(10,2),
            'Cash'::VARCHAR,
            NULL::VARCHAR,
            'Попытка переплаты'::TEXT
        );
    EXCEPTION WHEN OTHERS THEN
        v_failed := TRUE;
        RAISE NOTICE 'OK: overpayment blocked: %', SQLERRM;
    END;

    IF NOT v_failed THEN
        RAISE EXCEPTION 'FAIL: overpayment must be blocked';
    END IF;
END $$;

-- =========================================================
-- 11A. НОВЫЕ ЗАЩИТЫ: payment нельзя менять и удалять
-- =========================================================

DO $$
DECLARE
    v_payment_id INT;
    v_failed BOOLEAN := FALSE;
BEGIN
    SELECT payment_id INTO v_payment_id
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
    SELECT payment_id INTO v_payment_id
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

-- =========================================================
-- 12. ДОПЛАТА И ЗАКРЫТИЕ
-- =========================================================

SELECT register_payment(
    (SELECT order_id FROM test_ctx)::INT,
    550.00::NUMERIC(10,2),
    'Transfer'::VARCHAR,
    'TEST-TRX-002'::VARCHAR,
    'Окончательная оплата'::TEXT
);

SELECT change_repair_order_status(
    (SELECT order_id FROM test_ctx)::INT,
    'Closed'::VARCHAR,
    'Заказ закрыт после полной оплаты'::TEXT
);

-- =========================================================
-- 12A. НОВЫЕ ЗАЩИТЫ: order_status_history append-only
-- =========================================================

DO $$
DECLARE
    v_history_id INT;
    v_failed BOOLEAN := FALSE;
BEGIN
    SELECT history_id INTO v_history_id
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
    SELECT history_id INTO v_history_id
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
-- 13. НЕГАТИВ: нельзя менять закрытый заказ
-- =========================================================

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

-- =========================================================
-- 14. ПРОВЕРКА ВСПОМОГАТЕЛЬНЫХ ФУНКЦИЙ
-- =========================================================

DO $$
DECLARE
    v_has_services BOOLEAN;
    v_all_completed BOOLEAN;
    v_fully_paid BOOLEAN;
BEGIN
    SELECT has_order_services((SELECT order_id FROM test_ctx))
    INTO v_has_services;

    SELECT are_all_order_services_completed((SELECT order_id FROM test_ctx))
    INTO v_all_completed;

    SELECT is_order_fully_paid((SELECT order_id FROM test_ctx))
    INTO v_fully_paid;

    IF NOT v_has_services THEN
        RAISE EXCEPTION 'FAIL: has_order_services must return TRUE';
    END IF;

    IF NOT v_all_completed THEN
        RAISE EXCEPTION 'FAIL: are_all_order_services_completed must return TRUE';
    END IF;

    IF NOT v_fully_paid THEN
        RAISE EXCEPTION 'FAIL: is_order_fully_paid must return TRUE';
    END IF;

    RAISE NOTICE 'OK: helper functions return expected values';
END $$;

-- =========================================================
-- 15. ПРОВЕРКА ПРЕДСТАВЛЕНИЙ БЕЗ ВЫВОДА ТАБЛИЦ
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

    SELECT COUNT(*) INTO v_count
    FROM vw_unpaid_orders
    WHERE order_id = (SELECT order_id FROM test_ctx);

    IF v_count <> 0 THEN
        RAISE EXCEPTION 'FAIL: fully paid closed test order must not appear in vw_unpaid_orders';
    END IF;

    RAISE NOTICE 'OK: views passed validation checks';
END $$;

ROLLBACK;