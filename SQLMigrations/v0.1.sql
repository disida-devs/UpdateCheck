CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Params" (
    "Parameter" text NOT NULL,
    "Value" text NULL,
    CONSTRAINT "PK_Params" PRIMARY KEY ("Parameter")
);

CREATE TABLE "Sites" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "Name" varchar(25) NULL,
    "Url" text NULL,
    CONSTRAINT "PK_Sites" PRIMARY KEY ("Id")
);

CREATE TABLE "Links" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "SiteId" integer NULL,
    "Url" text NULL,
    "Posted" boolean NOT NULL,
    CONSTRAINT "PK_Links" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Links_Sites_SiteId" FOREIGN KEY ("SiteId") REFERENCES "Sites" ("Id")
);

CREATE INDEX "IX_Links_SiteId" ON "Links" ("SiteId");

CREATE UNIQUE INDEX "IX_Sites_Name" ON "Sites" ("Name");

CREATE UNIQUE INDEX "IX_Sites_Url" ON "Sites" ("Url");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20230813101740_v0.1', '7.0.10');

COMMIT;