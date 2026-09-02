using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LedgerLens.PortfolioCore.Migrations.Migrations;

    /// <inheritdoc />
    public partial class AddManualSimulationAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "simulation_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulation_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_simulation_accounts_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalTable: "portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "simulation_trade_drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    simulation_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    side = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    input_mode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    requested_quantity = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: true),
                    requested_amount = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    unused_amount = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    assumed_fee = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    assumed_tax = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    acquisition_fx_rate = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: true),
                    effective_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    market_instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    exchange = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    mic = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    price = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    price_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    feed = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    price_as_of = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retrieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    freshness = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    delay_seconds = table.Column<int>(type: "integer", nullable: true),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    retention_policy_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    confirmed_trade_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulation_trade_drafts", x => x.id);
                    table.ForeignKey(
                        name: "FK_simulation_trade_drafts_simulation_accounts_simulation_acco~",
                        column: x => x.simulation_account_id,
                        principalTable: "simulation_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "simulation_trade_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    simulation_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    side = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    assumed_fee = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    assumed_tax = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    fx_rate = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: true),
                    effective_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    market_instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    exchange = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    mic = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    price = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    price_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    feed = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    price_as_of = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retrieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    freshness = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    delay_seconds = table.Column<int>(type: "integer", nullable: true),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    retention_policy_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    entry_role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    corrects_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correction_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulation_trade_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_simulation_trade_entries_simulation_accounts_simulation_acc~",
                        column: x => x.simulation_account_id,
                        principalTable: "simulation_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_simulation_trade_entries_simulation_trade_entries_corrects_~",
                        column: x => x.corrects_entry_id,
                        principalTable: "simulation_trade_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "simulation_valuation_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    simulation_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: false),
                    current_price = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    current_value_usd = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    remaining_cost_usd = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    realized_usd = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    unrealized_usd = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: false),
                    current_fx_rate = table.Column<decimal>(type: "numeric(28,12)", precision: 28, scale: 12, nullable: true),
                    current_value_thb = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: true),
                    total_pl_thb = table.Column<decimal>(type: "numeric(28,10)", precision: 28, scale: 10, nullable: true),
                    is_complete = table.Column<bool>(type: "boolean", nullable: false),
                    missing_reasons = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    feed = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    price_as_of = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    retrieved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    freshness = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulation_valuation_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_simulation_valuation_snapshots_simulation_accounts_simulati~",
                        column: x => x.simulation_account_id,
                        principalTable: "simulation_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_simulation_accounts_portfolio_id_name",
                table: "simulation_accounts",
                columns: new[] { "portfolio_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulation_trade_drafts_confirmed_trade_id",
                table: "simulation_trade_drafts",
                column: "confirmed_trade_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulation_trade_drafts_simulation_account_id",
                table: "simulation_trade_drafts",
                column: "simulation_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_simulation_trade_entries_corrects_entry_id_entry_role",
                table: "simulation_trade_entries",
                columns: new[] { "corrects_entry_id", "entry_role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_simulation_trade_entries_simulation_account_id_effective_at~",
                table: "simulation_trade_entries",
                columns: new[] { "simulation_account_id", "effective_at", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_simulation_valuation_snapshots_simulation_account_id_symbol~",
                table: "simulation_valuation_snapshots",
                columns: new[] { "simulation_account_id", "symbol", "recorded_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "simulation_trade_drafts");

            migrationBuilder.DropTable(
                name: "simulation_trade_entries");

            migrationBuilder.DropTable(
                name: "simulation_valuation_snapshots");

            migrationBuilder.DropTable(
                name: "simulation_accounts");
        }
}
