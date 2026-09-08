using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportForAlle.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowerAndGuardianLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AnonymisedAt",
                table: "Guardians",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Guardians",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AnonymisedAt",
                table: "Borrowers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "Borrowers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnonymisedAt",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "AnonymisedAt",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Borrowers");
        }
    }
}
