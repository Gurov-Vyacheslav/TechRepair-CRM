-- =========================================
-- 1. CLIENT
-- =========================================
CREATE TABLE client (
    client_id           SERIAL PRIMARY KEY,
    first_name          VARCHAR(50) NOT NULL,
    last_name           VARCHAR(50) NOT NULL,
    phone               VARCHAR(20) NOT NULL UNIQUE,
    email               VARCHAR(254),
    address             TEXT,
    registration_date   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    notes               TEXT
);



-- =========================================
-- 2. DEVICE_TYPE
-- =========================================
CREATE TABLE device_type (
    device_type_id      SERIAL PRIMARY KEY,
    type_name           VARCHAR(100) NOT NULL UNIQUE
);


-- =========================================
-- 3. DEVICE
-- =========================================
CREATE TABLE device (
    device_id               SERIAL PRIMARY KEY,
    client_id               INT NOT NULL,
    device_type_id          INT NOT NULL,
    brand                   VARCHAR(50),
    model                   VARCHAR(50),
    serial_number           VARCHAR(100) UNIQUE,
    purchase_date           DATE,
    equipment_description   TEXT,
    external_condition      TEXT,
    notes                   TEXT,

    CONSTRAINT fk_device_client
        FOREIGN KEY (client_id)
        REFERENCES client(client_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_device_device_type
        FOREIGN KEY (device_type_id)
        REFERENCES device_type(device_type_id)
        ON DELETE RESTRICT
);

CREATE INDEX idx_device_client_id
    ON device(client_id);

CREATE INDEX idx_device_device_type_id
    ON device(device_type_id);



-- =========================================
-- 4. TECHNICIAN
-- =========================================
CREATE TABLE technician (
    technician_id        SERIAL PRIMARY KEY,
    first_name           VARCHAR(50) NOT NULL,
    last_name            VARCHAR(50) NOT NULL,
    email                VARCHAR(254) NOT NULL UNIQUE,
    phone                VARCHAR(20) NOT NULL UNIQUE,
    specialization       VARCHAR(100),
    hire_date            DATE NOT NULL,
    hourly_rate          NUMERIC(8,2) CHECK (hourly_rate >= 0),
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    notes                TEXT
);



-- =========================================
-- 5. ORDER_STATUS
-- =========================================
CREATE TABLE order_status (
    status_id            SMALLSERIAL PRIMARY KEY,
    status_name          VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO order_status (status_name)
VALUES
    ('Created'),
    ('Accepted'),
    ('InRepair'),
    ('Ready'),
    ('Closed'),
    ('Canceled')
ON CONFLICT (status_name) DO NOTHING;


-- =========================================
-- 6. REPAIR_ORDER
-- =========================================
CREATE TABLE repair_order (
    order_id                 SERIAL PRIMARY KEY,
    order_number             VARCHAR(30) NOT NULL UNIQUE,
    device_id                INT NOT NULL,
    status_id                SMALLINT NOT NULL,
    created_at               TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    accepted_at              TIMESTAMP,
    completed_at             TIMESTAMP,
    issued_at                TIMESTAMP,
    warranty_months          SMALLINT CHECK (warranty_months >= 0),
    problem_description      TEXT NOT NULL,
    diagnostic_result        TEXT,
    estimated_cost           NUMERIC(10,2) NOT NULL DEFAULT 0 CHECK (estimated_cost >= 0),
    total_cost               NUMERIC(10,2) NOT NULL DEFAULT 0 CHECK (total_cost >= 0),
    is_warranty_repair       BOOLEAN NOT NULL DEFAULT FALSE,
    notes                    TEXT,

    CONSTRAINT fk_repair_order_device
        FOREIGN KEY (device_id)
        REFERENCES device(device_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_repair_order_status
        FOREIGN KEY (status_id)
        REFERENCES order_status(status_id)
        ON DELETE RESTRICT,

    CONSTRAINT chk_repair_order_dates
        CHECK (
            (accepted_at  IS NULL OR accepted_at  >= created_at) AND
            (completed_at IS NULL OR completed_at >= created_at) AND
            (issued_at    IS NULL OR issued_at    >= created_at) AND
            (completed_at IS NULL OR accepted_at IS NULL OR completed_at >= accepted_at) AND
            (issued_at    IS NULL OR completed_at IS NULL OR issued_at >= completed_at)
        )
);

CREATE INDEX idx_repair_order_device_id
    ON repair_order(device_id);

CREATE INDEX idx_repair_order_status_id
    ON repair_order(status_id);

CREATE INDEX idx_repair_order_created_at
    ON repair_order(created_at);

CREATE INDEX idx_repair_order_status_created_at
    ON repair_order(status_id, created_at);



-- =========================================
-- 7. ORDER_STATUS_HISTORY
-- =========================================
CREATE TABLE order_status_history (
    history_id           SERIAL PRIMARY KEY,
    order_id             INT NOT NULL,
    status_id            SMALLINT NOT NULL,
    changed_at           TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    comment              TEXT,

    CONSTRAINT fk_order_status_history_order
        FOREIGN KEY (order_id)
        REFERENCES repair_order(order_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_order_status_history_status
        FOREIGN KEY (status_id)
        REFERENCES order_status(status_id)
        ON DELETE RESTRICT
);

CREATE INDEX idx_order_status_history_order_id
    ON order_status_history(order_id);

CREATE INDEX idx_order_status_history_status_id
    ON order_status_history(status_id);

CREATE INDEX idx_order_status_history_order_changed_at
    ON order_status_history(order_id, changed_at);


-- =========================================
-- 8. SERVICE
-- =========================================
CREATE TABLE service (
    service_id            SERIAL PRIMARY KEY,
    service_name          VARCHAR(100) NOT NULL UNIQUE,
    description           TEXT,
    base_price            NUMERIC(10,2) NOT NULL CHECK (base_price >= 0),
    estimated_duration    INT CHECK (estimated_duration >= 0),
    is_active             BOOLEAN NOT NULL DEFAULT TRUE
);



-- =========================================
-- 9. ORDER_SERVICE
-- =========================================
CREATE TABLE order_service (
    order_id             INT NOT NULL,
    service_id           INT NOT NULL,
    technician_id        INT,
    quantity             SMALLINT NOT NULL CHECK (quantity > 0),
    price_at_moment      NUMERIC(10,2) NOT NULL CHECK (price_at_moment >= 0),
    completed_at         TIMESTAMP,
    notes                TEXT,

    PRIMARY KEY (order_id, service_id),

    CONSTRAINT fk_order_service_order
        FOREIGN KEY (order_id)
        REFERENCES repair_order(order_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_order_service_service
        FOREIGN KEY (service_id)
        REFERENCES service(service_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_order_service_technician
        FOREIGN KEY (technician_id)
        REFERENCES technician(technician_id)
        ON DELETE RESTRICT
);

CREATE INDEX idx_order_service_service_id
    ON order_service(service_id);

CREATE INDEX idx_order_service_technician_id
    ON order_service(technician_id);

CREATE INDEX idx_order_service_completed_at
    ON order_service(completed_at);

CREATE INDEX idx_order_service_technician_completed_at
    ON order_service(technician_id, completed_at);


-- =========================================
-- 10. PART
-- =========================================
CREATE TABLE part (
    part_id                SERIAL PRIMARY KEY,
    part_number            VARCHAR(50) UNIQUE,
    part_name              VARCHAR(100) NOT NULL,
    manufacturer           VARCHAR(100),
    default_price          NUMERIC(10,2) CHECK (default_price >= 0),
    is_active              BOOLEAN NOT NULL DEFAULT TRUE,
    description            TEXT
);



-- =========================================
-- 11. ORDER_PART
-- =========================================
CREATE TABLE order_part (
    order_id             INT NOT NULL,
    part_id              INT NOT NULL,
    quantity             INT NOT NULL CHECK (quantity > 0),
    price_at_moment      NUMERIC(10,2) NOT NULL CHECK (price_at_moment >= 0),

    PRIMARY KEY (order_id, part_id),

    CONSTRAINT fk_order_part_order
        FOREIGN KEY (order_id)
        REFERENCES repair_order(order_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_order_part_part
        FOREIGN KEY (part_id)
        REFERENCES part(part_id)
        ON DELETE RESTRICT
);

CREATE INDEX idx_order_part_part_id
    ON order_part(part_id);


-- =========================================
-- 12. PAYMENT
-- =========================================
CREATE TABLE payment (
    payment_id            SERIAL PRIMARY KEY,
    order_id              INT NOT NULL,
    payment_date          TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    amount                NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    payment_method        VARCHAR(20) NOT NULL
                            CHECK (payment_method IN ('Cash', 'Card', 'Transfer')),
    transaction_number    VARCHAR(100),
    notes                 TEXT,

    CONSTRAINT fk_payment_order
        FOREIGN KEY (order_id)
        REFERENCES repair_order(order_id)
        ON DELETE RESTRICT
);

CREATE INDEX idx_payment_order_id
    ON payment(order_id);

CREATE INDEX idx_payment_payment_date
    ON payment(payment_date);

CREATE INDEX idx_payment_method
    ON payment(payment_method);

CREATE INDEX idx_payment_order_date
    ON payment(order_id, payment_date);