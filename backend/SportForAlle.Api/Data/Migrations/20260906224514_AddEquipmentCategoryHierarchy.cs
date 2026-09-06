using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportForAlle.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquipmentCategories_Name",
                table: "EquipmentCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentCategoryId",
                table: "EquipmentCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_Name",
                table: "EquipmentCategories",
                column: "Name",
                unique: true,
                filter: "\"ParentCategoryId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_ParentCategoryId_Name",
                table: "EquipmentCategories",
                columns: new[] { "ParentCategoryId", "Name" },
                unique: true,
                filter: "\"ParentCategoryId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentCategories_EquipmentCategories_ParentCategoryId",
                table: "EquipmentCategories",
                column: "ParentCategoryId",
                principalTable: "EquipmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentCategories_EquipmentCategories_ParentCategoryId",
                table: "EquipmentCategories");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentCategories_Name",
                table: "EquipmentCategories");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentCategories_ParentCategoryId_Name",
                table: "EquipmentCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "EquipmentCategories");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_Name",
                table: "EquipmentCategories",
                column: "Name",
                unique: true);
        }
    }
}
