using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update_Business_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BusinessUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Businesses",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Businesses",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "NG");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Businesses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Businesses",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Businesses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Businesses",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Name",
                table: "Businesses",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Businesses_Name",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BusinessUsers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Businesses");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
