using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161

namespace LedgerLens.PortfolioCore.Migrations
{
    /// <inheritdoc />
    public partial class CompleteAuditableLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "correction_reason",
                table: "cash_ledger_entries",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "corrects_entry_id",
                table: "cash_ledger_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "entry_role",
                table: "cash_ledger_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Original");

            migrationBuilder.AddColumn<string>(
                name: "instrument_symbol",
                table: "cash_ledger_entries",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "cash_ledger_entries",
                type: "numeric(28,12)",
                precision: 28,
                scale: 12,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "unit_price",
                table: "cash_ledger_entries",
                type: "numeric(28,10)",
                precision: 28,
                scale: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cash_ledger_entries_corrects_entry_id_entry_role",
                table: "cash_ledger_entries",
                columns: new[] { "corrects_entry_id", "entry_role" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_cash_ledger_entries_cash_ledger_entries_corrects_entry_id",
                table: "cash_ledger_entries",
                column: "corrects_entry_id",
                principalTable: "cash_ledger_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cash_ledger_entries_cash_ledger_entries_corrects_entry_id",
                table: "cash_ledger_entries");

            migrationBuilder.DropIndex(
                name: "IX_cash_ledger_entries_corrects_entry_id_entry_role",
                table: "cash_ledger_entries");

            migrationBuilder.DropColumn(
                name: "correction_reason",
                table: "cash_ledger_entries");

            migrationBuilder.DropColumn(
                name: "corrects_entry_id",
                table: "cash_ledger_entries");

            migrationBuilder.DropColumn(
                name: "entry_role",
                table: "cash_ledger_entries");

            migrationBuilder.DropColumn(
                name: "instrument_symbol",
                table: "cash_ledger_entries");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "cash_ledger_entries");

            migrationBuilder.DropColumn(
                name: "unit_price",
                table: "cash_ledger_entries");
        }
    }
}
