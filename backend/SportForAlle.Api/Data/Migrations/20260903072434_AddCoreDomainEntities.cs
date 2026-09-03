using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportForAlle.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // An int identity column cannot be retyped to uuid in place -
            // Postgres has no cast between the two, and identity columns
            // cannot be a uuid at all. EquipmentCategories is empty at this
            // point (it exists only to prove the very first migration
            // worked), so there is no data to preserve: drop and recreate
            // the column instead of trying to convert it.
            migrationBuilder.Sql("ALTER TABLE \"EquipmentCategories\" DROP CONSTRAINT \"PK_EquipmentCategories\";");
            migrationBuilder.Sql("ALTER TABLE \"EquipmentCategories\" DROP COLUMN \"Id\";");
            migrationBuilder.Sql("ALTER TABLE \"EquipmentCategories\" ADD COLUMN \"Id\" uuid NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE \"EquipmentCategories\" ADD CONSTRAINT \"PK_EquipmentCategories\" PRIMARY KEY (\"Id\");");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "EquipmentCategories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByStaffId",
                table: "EquipmentCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "EquipmentCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByStaffId",
                table: "EquipmentCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Auth0UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Staff_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_EquipmentCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "EquipmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Equipment_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Guardians",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guardians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guardians_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Guardians_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Borrowers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    GuardianId = table.Column<Guid>(type: "uuid", nullable: false),
                    LateReturnCount = table.Column<int>(type: "integer", nullable: false),
                    IsUnreliable = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Borrowers_Guardians_GuardianId",
                        column: x => x.GuardianId,
                        principalTable: "Guardians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Borrowers_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Borrowers_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Bans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LiftedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LiftedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    FeePaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bans_Borrowers_BorrowerId",
                        column: x => x.BorrowerId,
                        principalTable: "Borrowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bans_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bans_Staff_LiftedByStaffId",
                        column: x => x.LiftedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bans_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DaysLate = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Borrowers_BorrowerId",
                        column: x => x.BorrowerId,
                        principalTable: "Borrowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loans_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loans_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Loans_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Borrowers_BorrowerId",
                        column: x => x.BorrowerId,
                        principalTable: "Borrowers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notes_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ContactAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByStaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactAttempts_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactAttempts_Staff_CreatedByStaffId",
                        column: x => x.CreatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContactAttempts_Staff_UpdatedByStaffId",
                        column: x => x.UpdatedByStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_CreatedByStaffId",
                table: "EquipmentCategories",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_UpdatedByStaffId",
                table: "EquipmentCategories",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_BorrowerId",
                table: "Bans",
                column: "BorrowerId",
                unique: true,
                filter: "\"LiftedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_CreatedByStaffId",
                table: "Bans",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_LiftedByStaffId",
                table: "Bans",
                column: "LiftedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_UpdatedByStaffId",
                table: "Bans",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowers_CreatedByStaffId",
                table: "Borrowers",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowers_GuardianId",
                table: "Borrowers",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowers_UpdatedByStaffId",
                table: "Borrowers",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAttempts_CreatedByStaffId",
                table: "ContactAttempts",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAttempts_LoanId",
                table: "ContactAttempts",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactAttempts_UpdatedByStaffId",
                table: "ContactAttempts",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_CategoryId",
                table: "Equipment",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_CreatedByStaffId",
                table: "Equipment",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_SerialNumber",
                table: "Equipment",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_UpdatedByStaffId",
                table: "Equipment",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_CreatedByStaffId",
                table: "Guardians",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_UpdatedByStaffId",
                table: "Guardians",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BorrowerId_Status",
                table: "Loans",
                columns: new[] { "BorrowerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_CreatedByStaffId",
                table: "Loans",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_EquipmentId",
                table: "Loans",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_Status_DueDate",
                table: "Loans",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_UpdatedByStaffId",
                table: "Loans",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_BorrowerId",
                table: "Notes",
                column: "BorrowerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CreatedByStaffId",
                table: "Notes",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UpdatedByStaffId",
                table: "Notes",
                column: "UpdatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_Auth0UserId",
                table: "Staff",
                column: "Auth0UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_CreatedByStaffId",
                table: "Staff",
                column: "CreatedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_UpdatedByStaffId",
                table: "Staff",
                column: "UpdatedByStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentCategories_Staff_CreatedByStaffId",
                table: "EquipmentCategories",
                column: "CreatedByStaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentCategories_Staff_UpdatedByStaffId",
                table: "EquipmentCategories",
                column: "UpdatedByStaffId",
                principalTable: "Staff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentCategories_Staff_CreatedByStaffId",
                table: "EquipmentCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentCategories_Staff_UpdatedByStaffId",
                table: "EquipmentCategories");

            migrationBuilder.DropTable(
                name: "Bans");

            migrationBuilder.DropTable(
                name: "ContactAttempts");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "Borrowers");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Guardians");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentCategories_CreatedByStaffId",
                table: "EquipmentCategories");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentCategories_UpdatedByStaffId",
                table: "EquipmentCategories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EquipmentCategories");

            migrationBuilder.DropColumn(
                name: "CreatedByStaffId",
                table: "EquipmentCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EquipmentCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedByStaffId",
                table: "EquipmentCategories");

            migrationBuilder.Sql("ALTER TABLE \"EquipmentCategories\" DROP CONSTRAINT \"PK_EquipmentCategories\";");
            migrationBuilder.Sql("ALTER TABLE \"EquipmentCategories\" DROP COLUMN \"Id\";");
            migrationBuilder.Sql(
                "ALTER TABLE \"EquipmentCategories\" ADD COLUMN \"Id\" integer GENERATED BY DEFAULT AS IDENTITY;");
            migrationBuilder.Sql(
                "ALTER TABLE \"EquipmentCategories\" ADD CONSTRAINT \"PK_EquipmentCategories\" PRIMARY KEY (\"Id\");");
        }
    }
}
