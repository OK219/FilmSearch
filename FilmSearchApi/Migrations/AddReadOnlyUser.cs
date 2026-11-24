#nullable disable

using System;
using FilmSearchApi.DB;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FilmSearchApi.Migrations;

[DbContext(typeof(FilmDbContext))]
[Migration("20261124120000_AddReadOnlyUser")]
public partial class AddReadOnlyUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var password = Environment.GetEnvironmentVariable(
            "POSTGRES_PASSWORD_READONLY");

        var user = Environment.GetEnvironmentVariable(
            "POSTGRES_USER_READONLY");

        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(user))
        {
            throw new InvalidOperationException(
                "Read-only user or password is not set.");
        }

        var role = QuoteIdentifier(user);
        var passwordSql = QuotePassword(password);
        var userLiteral = QuoteLiteral(user);

        // Создаём роль, если её ещё нет
        migrationBuilder.Sql($"""
                              DO $$
                              BEGIN
                                  IF NOT EXISTS (
                                      SELECT 1 FROM pg_roles
                                      WHERE rolname = {userLiteral}
                                  ) THEN
                                      EXECUTE 'CREATE ROLE ' || quote_ident({userLiteral}) || ' LOGIN';
                                  END IF;
                              END
                              $$;
                              """);

        migrationBuilder.Sql($"""
                              ALTER ROLE {role}
                                  LOGIN
                                  NOSUPERUSER
                                  NOCREATEDB
                                  NOCREATEROLE
                                  NOREPLICATION
                                  NOBYPASSRLS;
                              """);

        migrationBuilder.Sql($"""
                              ALTER ROLE {role} PASSWORD {passwordSql};
                              """);

        migrationBuilder.Sql($"""
                              GRANT CONNECT ON DATABASE filmsearch TO {role};
                              GRANT USAGE ON SCHEMA public TO {role};
                              GRANT SELECT ON ALL TABLES IN SCHEMA public TO {role};
                              GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO {role};
                              GRANT pg_monitor TO {role};
                              """);

        migrationBuilder.Sql($"""
                              ALTER DEFAULT PRIVILEGES
                              IN SCHEMA public
                              GRANT SELECT ON TABLES TO {role};

                              ALTER DEFAULT PRIVILEGES
                              IN SCHEMA public
                              GRANT SELECT ON SEQUENCES TO {role};
                              """);
    }

    private static string QuoteIdentifier(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string QuoteLiteral(string value)
    {
        return "'" + value.Replace("'", "''") + "'";
    }

    private static string QuotePassword(string password)
    {
        return QuoteLiteral(password);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var user = Environment.GetEnvironmentVariable(
            "POSTGRES_USER_READONLY"
        );

        if (string.IsNullOrWhiteSpace(user))
        {
            throw new InvalidOperationException(
                "POSTGRES_PASSWORD_READONLY environment variable is not set."
            );
        }

        migrationBuilder.Sql("""
                             ALTER DEFAULT PRIVILEGES
                             IN SCHEMA public
                             REVOKE SELECT ON TABLES FROM app_readonly;

                             ALTER DEFAULT PRIVILEGES
                             IN SCHEMA public
                             REVOKE SELECT ON SEQUENCES FROM app_readonly;

                             REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public
                             FROM app_readonly;

                             REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public
                             FROM app_readonly;

                             REVOKE USAGE ON SCHEMA public
                             FROM app_readonly;

                             REVOKE CONNECT ON DATABASE "FilmSearch"
                             FROM app_readonly;

                             DROP OWNED BY app_readonly;

                             DROP ROLE IF EXISTS app_readonly;
                             """);
        migrationBuilder.Sql($"""
                              REVOKE pg_monitor FROM "{user.Replace("\"", "\"\"")}";
                              """);
    }
}