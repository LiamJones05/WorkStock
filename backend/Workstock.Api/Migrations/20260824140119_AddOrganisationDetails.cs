using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workstock.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostCode",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Organisations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Organisations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "County",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PostCode",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Organisations");
        }
    }
}
