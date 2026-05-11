-- NOTE: These queries are for Postgres

-- Projects
CREATE TABLE projects (
    id          SERIAL          PRIMARY KEY,
    name        VARCHAR(100)    NOT NULL,
    api_key     CHAR(64)        NOT NULL UNIQUE,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Function Registry (manual history)
CREATE TABLE function_registry (
    id              SERIAL          PRIMARY KEY,
    project_id      INT             NOT NULL REFERENCES projects(id),
    function_name   VARCHAR(256)    NOT NULL,
    file_name       VARCHAR(512)    NOT NULL,
    first_seen_at   TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    valid_from      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    valid_to        TIMESTAMPTZ,    -- NULL means currently active

    CONSTRAINT uq_function UNIQUE (project_id, function_name, file_name)
);

CREATE TABLE function_registry_history (
    id              INT             NOT NULL,
    project_id      INT             NOT NULL,
    function_name   VARCHAR(256)    NOT NULL,
    file_name       VARCHAR(512)    NOT NULL,
    first_seen_at   TIMESTAMPTZ     NOT NULL,
    valid_from      TIMESTAMPTZ     NOT NULL,
    valid_to        TIMESTAMPTZ     NOT NULL
);

CREATE INDEX ix_function_registry_project_id ON function_registry(project_id);

-- Function Registry history trigger
CREATE OR REPLACE FUNCTION fn_function_registry_history()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO function_registry_history
        (id, project_id, function_name, file_name, first_seen_at, valid_from, valid_to)
    VALUES
        (OLD.id, OLD.project_id, OLD.function_name, OLD.file_name,
         OLD.first_seen_at, OLD.valid_from, NOW());
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_function_registry_history
BEFORE UPDATE OR DELETE ON function_registry
FOR EACH ROW EXECUTE FUNCTION fn_function_registry_history();

-- Individual user installs
CREATE TABLE installs (
    id          SERIAL          PRIMARY KEY,
    project_id  INT             NOT NULL REFERENCES projects(id),
    install_id  CHAR(36)        NOT NULL,
    first_seen_at TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    last_seen_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_install UNIQUE (project_id, install_id)
);

CREATE INDEX ix_installs_project_id ON installs(project_id);

-- Events (for function fires)
CREATE TABLE events (
    id                      BIGSERIAL       PRIMARY KEY,
    function_registry_id    INT             NOT NULL REFERENCES function_registry(id),
    install_id              INT             NOT NULL REFERENCES installs(id),
    recorded_at             TIMESTAMPTZ     NOT NULL
);

CREATE INDEX ix_events_function_registry_id ON events(function_registry_id);
CREATE INDEX ix_events_install_id ON events(install_id);
