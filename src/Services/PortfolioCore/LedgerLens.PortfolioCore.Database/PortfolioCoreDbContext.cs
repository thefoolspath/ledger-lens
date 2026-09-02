using LedgerLens.PortfolioCore.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerLens.PortfolioCore.Database;

public partial class PortfolioCoreDbContext(DbContextOptions<PortfolioCoreDbContext> options)
    : DbContext(options)
{
    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<Portfolio> Portfolios { get; set; }

    public virtual DbSet<InvestmentAccount> InvestmentAccounts { get; set; }

    public virtual DbSet<CashLedgerEntry> CashLedgerEntries { get; set; }
    public virtual DbSet<SimulationAccount> SimulationAccounts { get; set; }
    public virtual DbSet<SimulationTradeDraft> SimulationTradeDrafts { get; set; }
    public virtual DbSet<SimulationTradeEntry> SimulationTradeEntries { get; set; }
    public virtual DbSet<SimulationValuationSnapshot> SimulationValuationSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(user => user.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
            entity.Property(user => user.CreatedAt).HasColumnName("created_at").IsRequired();
        });

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.ToTable("portfolios");
            entity.HasKey(portfolio => portfolio.Id);
            entity.Property(portfolio => portfolio.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(portfolio => portfolio.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
            entity.Property(portfolio => portfolio.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(portfolio => portfolio.BaseCurrency).HasColumnName("base_currency").HasMaxLength(3).IsFixedLength().IsRequired();
            entity.Property(portfolio => portfolio.ReportingCurrency).HasColumnName("reporting_currency").HasMaxLength(3).IsFixedLength().IsRequired();
            entity.Property(portfolio => portfolio.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.HasIndex(portfolio => new { portfolio.OwnerUserId, portfolio.Name }).IsUnique();
            entity.HasOne<UserProfile>()
                .WithMany()
                .HasForeignKey(portfolio => portfolio.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvestmentAccount>(entity =>
        {
            entity.ToTable("investment_accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(account => account.PortfolioId).HasColumnName("portfolio_id").IsRequired();
            entity.Property(account => account.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(account => account.Broker).HasColumnName("broker").HasMaxLength(100).IsRequired();
            entity.Property(account => account.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength().IsRequired();
            entity.Property(account => account.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.HasIndex(account => new { account.PortfolioId, account.Name }).IsUnique();
            entity.HasOne<Portfolio>()
                .WithMany()
                .HasForeignKey(account => account.PortfolioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CashLedgerEntry>(entity =>
        {
            entity.ToTable("cash_ledger_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(entry => entry.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(entry => entry.EntryType).HasColumnName("entry_type").HasMaxLength(20).IsRequired();
            entity.Property(entry => entry.EntryRole).HasColumnName("entry_role").HasMaxLength(20).IsRequired();
            entity.Property(entry => entry.Amount).HasColumnName("amount").HasPrecision(28, 10).IsRequired();
            entity.Property(entry => entry.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength().IsRequired();
            entity.Property(entry => entry.EffectiveAt).HasColumnName("effective_at").IsRequired();
            entity.Property(entry => entry.Note).HasColumnName("note").HasMaxLength(500);
            entity.Property(entry => entry.RecordedAt).HasColumnName("recorded_at").IsRequired();
            entity.Property(entry => entry.InstrumentSymbol).HasColumnName("instrument_symbol").HasMaxLength(32);
            entity.Property(entry => entry.Quantity).HasColumnName("quantity").HasPrecision(28, 12);
            entity.Property(entry => entry.UnitPrice).HasColumnName("unit_price").HasPrecision(28, 10);
            entity.Property(entry => entry.CorrectsEntryId).HasColumnName("corrects_entry_id");
            entity.Property(entry => entry.CorrectionReason).HasColumnName("correction_reason").HasMaxLength(500);
            entity.HasIndex(entry => new { entry.AccountId, entry.EffectiveAt, entry.Id });
            entity.HasIndex(entry => new { entry.CorrectsEntryId, entry.EntryRole }).IsUnique();
            entity.HasOne<InvestmentAccount>()
                .WithMany()
                .HasForeignKey(entry => entry.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CashLedgerEntry>()
                .WithMany()
                .HasForeignKey(entry => entry.CorrectsEntryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SimulationAccount>(entity =>
        {
            entity.ToTable("simulation_accounts");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(item => item.PortfolioId).HasColumnName("portfolio_id").IsRequired();
            entity.Property(item => item.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.HasIndex(item => new { item.PortfolioId, item.Name }).IsUnique();
            entity.HasOne<Portfolio>().WithMany().HasForeignKey(item => item.PortfolioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SimulationTradeDraft>(entity =>
        {
            entity.ToTable("simulation_trade_drafts");
            ConfigureSimulationDraft(entity);
        });

        modelBuilder.Entity<SimulationTradeEntry>(entity =>
        {
            entity.ToTable("simulation_trade_entries");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(item => item.SimulationAccountId).HasColumnName("simulation_account_id").IsRequired();
            entity.Property(item => item.Side).HasColumnName("side").HasMaxLength(8).IsRequired();
            entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(28, 12).IsRequired();
            entity.Property(item => item.UnitPrice).HasColumnName("unit_price").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.GrossAmount).HasColumnName("gross_amount").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.AssumedFee).HasColumnName("assumed_fee").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.AssumedTax).HasColumnName("assumed_tax").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.FxRate).HasColumnName("fx_rate").HasPrecision(28, 12);
            entity.Property(item => item.EffectiveAt).HasColumnName("effective_at").IsRequired();
            entity.Property(item => item.RecordedAt).HasColumnName("recorded_at").IsRequired();
            ConfigureEvidence(entity);
            entity.Property(item => item.EntryRole).HasColumnName("entry_role").HasMaxLength(16).IsRequired();
            entity.Property(item => item.CorrectsEntryId).HasColumnName("corrects_entry_id");
            entity.Property(item => item.CorrectionReason).HasColumnName("correction_reason").HasMaxLength(500);
            entity.HasIndex(item => new { item.SimulationAccountId, item.EffectiveAt, item.Id });
            entity.HasIndex(item => new { item.CorrectsEntryId, item.EntryRole }).IsUnique();
            entity.HasOne<SimulationAccount>().WithMany().HasForeignKey(item => item.SimulationAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<SimulationTradeEntry>().WithMany().HasForeignKey(item => item.CorrectsEntryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SimulationValuationSnapshot>(entity =>
        {
            entity.ToTable("simulation_valuation_snapshots");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(item => item.SimulationAccountId).HasColumnName("simulation_account_id").IsRequired();
            entity.Property(item => item.Symbol).HasColumnName("symbol").HasMaxLength(32).IsRequired();
            entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(28, 12).IsRequired();
            entity.Property(item => item.CurrentPrice).HasColumnName("current_price").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.CurrentValueUsd).HasColumnName("current_value_usd").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.RemainingCostUsd).HasColumnName("remaining_cost_usd").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.RealizedUsd).HasColumnName("realized_usd").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.UnrealizedUsd).HasColumnName("unrealized_usd").HasPrecision(28, 10).IsRequired();
            entity.Property(item => item.CurrentFxRate).HasColumnName("current_fx_rate").HasPrecision(28, 12);
            entity.Property(item => item.CurrentValueThb).HasColumnName("current_value_thb").HasPrecision(28, 10);
            entity.Property(item => item.TotalPlThb).HasColumnName("total_pl_thb").HasPrecision(28, 10);
            entity.Property(item => item.IsComplete).HasColumnName("is_complete").IsRequired();
            entity.Property(item => item.MissingReasons).HasColumnName("missing_reasons").HasMaxLength(1000).IsRequired();
            entity.Property(item => item.Provider).HasColumnName("provider").HasMaxLength(40).IsRequired();
            entity.Property(item => item.Feed).HasColumnName("feed").HasMaxLength(80).IsRequired();
            entity.Property(item => item.PriceAsOf).HasColumnName("price_as_of").IsRequired();
            entity.Property(item => item.RetrievedAt).HasColumnName("retrieved_at").IsRequired();
            entity.Property(item => item.Freshness).HasColumnName("freshness").HasMaxLength(32).IsRequired();
            entity.Property(item => item.RequestId).HasColumnName("request_id").HasMaxLength(100).IsRequired();
            entity.Property(item => item.RecordedAt).HasColumnName("recorded_at").IsRequired();
            entity.HasIndex(item => new { item.SimulationAccountId, item.Symbol, item.RecordedAt });
            entity.HasOne<SimulationAccount>().WithMany().HasForeignKey(item => item.SimulationAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private static void ConfigureSimulationDraft(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<SimulationTradeDraft> entity)
    {
        entity.HasKey(item => item.Id);
        entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(item => item.SimulationAccountId).HasColumnName("simulation_account_id").IsRequired();
        entity.Property(item => item.Side).HasColumnName("side").HasMaxLength(8).IsRequired();
        entity.Property(item => item.InputMode).HasColumnName("input_mode").HasMaxLength(16).IsRequired();
        entity.Property(item => item.RequestedQuantity).HasColumnName("requested_quantity").HasPrecision(28, 12);
        entity.Property(item => item.RequestedAmount).HasColumnName("requested_amount").HasPrecision(28, 10);
        entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(28, 12).IsRequired();
        entity.Property(item => item.GrossAmount).HasColumnName("gross_amount").HasPrecision(28, 10).IsRequired();
        entity.Property(item => item.UnusedAmount).HasColumnName("unused_amount").HasPrecision(28, 10).IsRequired();
        entity.Property(item => item.AssumedFee).HasColumnName("assumed_fee").HasPrecision(28, 10).IsRequired();
        entity.Property(item => item.AssumedTax).HasColumnName("assumed_tax").HasPrecision(28, 10).IsRequired();
        entity.Property(item => item.AcquisitionFxRate).HasColumnName("acquisition_fx_rate").HasPrecision(28, 12);
        entity.Property(item => item.EffectiveAt).HasColumnName("effective_at").IsRequired();
        entity.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(item => item.ExpiresAt).HasColumnName("expires_at").IsRequired();
        ConfigureEvidence(entity);
        entity.Property(item => item.ConfirmedTradeId).HasColumnName("confirmed_trade_id");
        entity.HasIndex(item => item.ConfirmedTradeId).IsUnique();
        entity.HasOne<SimulationAccount>().WithMany().HasForeignKey(item => item.SimulationAccountId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureEvidence<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
        where TEntity : class
    {
        entity.Property<Guid>("MarketInstrumentId").HasColumnName("market_instrument_id").IsRequired();
        entity.Property<string>("Symbol").HasColumnName("symbol").HasMaxLength(32).IsRequired();
        entity.Property<string>("Exchange").HasColumnName("exchange").HasMaxLength(80).IsRequired();
        entity.Property<string>("Mic").HasColumnName("mic").HasMaxLength(8).IsRequired();
        entity.Property<string>("Currency").HasColumnName("currency").HasMaxLength(3).IsFixedLength().IsRequired();
        entity.Property<decimal>("Price").HasColumnName("price").HasPrecision(28, 10).IsRequired();
        entity.Property<string>("PriceKind").HasColumnName("price_kind").HasMaxLength(20).IsRequired();
        entity.Property<string>("Provider").HasColumnName("provider").HasMaxLength(40).IsRequired();
        entity.Property<string>("Feed").HasColumnName("feed").HasMaxLength(80).IsRequired();
        entity.Property<DateTimeOffset>("PriceAsOf").HasColumnName("price_as_of").IsRequired();
        entity.Property<DateTimeOffset>("RetrievedAt").HasColumnName("retrieved_at").IsRequired();
        entity.Property<string>("Freshness").HasColumnName("freshness").HasMaxLength(32).IsRequired();
        entity.Property<int?>("DelaySeconds").HasColumnName("delay_seconds");
        entity.Property<string>("RequestId").HasColumnName("request_id").HasMaxLength(100).IsRequired();
        entity.Property<string>("RetentionPolicyKey").HasColumnName("retention_policy_key").HasMaxLength(80).IsRequired();
    }
}
