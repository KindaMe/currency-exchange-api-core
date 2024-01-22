using Microsoft.EntityFrameworkCore;

namespace currency_exchange_api_core.Models;

public partial class CurrencyExchangeApiDbContext : DbContext
{
    public CurrencyExchangeApiDbContext()
    {
    }

    public CurrencyExchangeApiDbContext(DbContextOptions<CurrencyExchangeApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversion> Conversions { get; set; }

    public virtual DbSet<Cut> Cuts { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversion>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AmountAfter)
                .HasColumnType("decimal(18, 8)")
                .HasColumnName("amount_after");
            entity.Property(e => e.AmountBefore)
                .HasColumnType("decimal(18, 8)")
                .HasColumnName("amount_before");
            entity.Property(e => e.CurrencyAfter)
                .HasMaxLength(3)
                .HasColumnName("currency_after");
            entity.Property(e => e.CurrencyBefore)
                .HasMaxLength(3)
                .HasColumnName("currency_before");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.Rate)
                .HasColumnType("decimal(18, 8)")
                .HasColumnName("rate");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Conversions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversions_Transactions");
        });

        modelBuilder.Entity<Cut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_GlobalSettings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyPercentage)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("buy_percentage");
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("effective_date");
            entity.Property(e => e.SellPercentage)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sell_percentage");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.WalletFromId).HasColumnName("wallet_from_id");
            entity.Property(e => e.WalletToId).HasColumnName("wallet_to_id");

            entity.HasOne(d => d.WalletFrom).WithMany(p => p.TransactionWalletFroms)
                .HasForeignKey(d => d.WalletFromId)
                .HasConstraintName("FK_Transactions_Wallets1");

            entity.HasOne(d => d.WalletTo).WithMany(p => p.TransactionWalletTos)
                .HasForeignKey(d => d.WalletToId)
                .HasConstraintName("FK_Transactions_Wallets");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
               .HasDefaultValue((byte)1)
               .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasColumnName("currency");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wallets_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}