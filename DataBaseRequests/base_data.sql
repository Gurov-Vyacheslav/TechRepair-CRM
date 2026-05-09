INSERT INTO device_type (type_name)
VALUES
    ('Ноутбук'),
    ('Смартфон'),
    ('Планшет'),
    ('Монитор'),
    ('Принтер'),
    ('Системный блок'),
    ('Игровая приставка'),
    ('Другое')
ON CONFLICT (type_name) DO NOTHING;

INSERT INTO service (service_name, description, base_price, estimated_duration, is_active)
VALUES
    ('Диагностика', 'Первичная диагностика устройства', 1000.00, 60, TRUE),
    ('Чистка от пыли', 'Разборка и чистка устройства от пыли', 1500.00, 90, TRUE),
    ('Замена экрана', 'Замена дисплея или матрицы', 3500.00, 120, TRUE),
    ('Замена аккумулятора', 'Замена аккумуляторной батареи', 2000.00, 60, TRUE),
    ('Установка ОС', 'Установка операционной системы и базовая настройка', 2500.00, 120, TRUE)
ON CONFLICT (service_name) DO NOTHING;

INSERT INTO technician (
    first_name,
    last_name,
    email,
    phone,
    specialization,
    is_active,
    notes
)
VALUES
    ('Алексей', 'Иванов', 'ivanov.tech@example.com', '+79000000001', 'Ноутбуки', TRUE, NULL),
    ('Мария', 'Петрова', 'petrova.tech@example.com', '+79000000002', 'Смартфоны', TRUE, NULL),
    ('Дмитрий', 'Соколов', 'sokolov.tech@example.com', '+79000000003', 'ПК и периферия', TRUE, NULL)
ON CONFLICT (email) DO NOTHING;

INSERT INTO part (
    part_number,
    part_name,
    manufacturer,
    default_price,
    is_active,
    description
)
VALUES
    ('BAT-LEN-001', 'Аккумулятор для ноутбука Lenovo', 'Lenovo', 4500.00, TRUE, NULL),
    ('SCR-IP-001', 'Экран для смартфона', 'Generic', 6000.00, TRUE, NULL),
    ('SSD-500-001', 'SSD 500 GB', 'Kingston', 4200.00, TRUE, NULL),
    ('FAN-LAP-001', 'Вентилятор охлаждения ноутбука', 'Generic', 1800.00, TRUE, NULL)
ON CONFLICT (part_number) DO NOTHING;