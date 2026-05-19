-- NOTE: These queries are for MS SQL Server

-- Projects
CREATE TABLE projects (
    id          INT             IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    name        NVARCHAR(100)                   NOT NULL,
    api_key     CHAR(64)                        NOT NULL UNIQUE,
    created_at  DATETIMEOFFSET                  NOT NULL CONSTRAINT df_projects_created_at DEFAULT SYSDATETIMEOFFSET()
);

-- Builds (one row per instrumented version of a project)
CREATE TABLE builds (
    id          INT             IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    project_id  INT                             NOT NULL REFERENCES projects(id),
    version     NVARCHAR(50)                    NOT NULL,
    created_at  DATETIMEOFFSET                  NOT NULL CONSTRAINT df_builds_created_at DEFAULT SYSDATETIMEOFFSET(),

    CONSTRAINT uq_build UNIQUE (project_id, version)
);

CREATE INDEX ix_builds_project_id ON builds(project_id);

-- Function Registry (scoped per build — each build is its own snapshot of functions)
CREATE TABLE function_registry (
    id              INT             IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    build_id        INT                             NOT NULL REFERENCES builds(id),
    function_name   NVARCHAR(256)                   NOT NULL,
    file_name       NVARCHAR(512)                   NOT NULL,
    first_seen_at   DATETIMEOFFSET                  NOT NULL CONSTRAINT df_function_registry_first_seen_at DEFAULT SYSDATETIMEOFFSET(),

    CONSTRAINT uq_function UNIQUE (build_id, function_name, file_name)
);

CREATE INDEX ix_function_registry_build_id ON function_registry(build_id);

-- Individual user installs (project-scoped — a browser keeps one install id across version upgrades)
CREATE TABLE installs (
    id              INT             IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    project_id      INT                             NOT NULL REFERENCES projects(id),
    install_id      CHAR(36)                        NOT NULL,
    first_seen_at   DATETIMEOFFSET                  NOT NULL CONSTRAINT df_installs_first_seen_at DEFAULT SYSDATETIMEOFFSET(),
    last_seen_at    DATETIMEOFFSET                  NOT NULL CONSTRAINT df_installs_last_seen_at DEFAULT SYSDATETIMEOFFSET(),

    CONSTRAINT uq_install UNIQUE (project_id, install_id)
);

CREATE INDEX ix_installs_project_id ON installs(project_id);

-- Events (function fires)
CREATE TABLE events (
    id                      BIGINT          IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    function_registry_id    INT                             NOT NULL REFERENCES function_registry(id),
    install_id              INT                             NOT NULL REFERENCES installs(id),
    recorded_at             DATETIMEOFFSET                  NOT NULL
);

CREATE INDEX ix_events_function_registry_id ON events(function_registry_id);
CREATE INDEX ix_events_install_id ON events(install_id);
