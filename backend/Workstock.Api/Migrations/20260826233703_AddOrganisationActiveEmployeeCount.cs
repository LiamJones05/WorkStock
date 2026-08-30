using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workstock.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationActiveEmployeeCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveEmployeeCount",
                table: "Organisations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE "Organisations" o
                SET "ActiveEmployeeCount" = COALESCE((
                    SELECT COUNT(*)::integer
                    FROM "Users" u
                    WHERE u."OrganisationId" = o."Id" AND u."IsActive" = TRUE
                ), 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveEmployeeCount",
                table: "Organisations");
        }
    }
}
