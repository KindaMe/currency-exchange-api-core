using System;
using System.Collections.Generic;
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

    public virtual DbSet<GlobalSetting> GlobalSettings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlobalSetting>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyPercentageCut)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("buy_percentage_cut");
            entity.Property(e => e.SellPercentageCut)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sell_percentage_cut");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AmountIn)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount_in");
            entity.Property(e => e.CurrencyIn)
                .HasMaxLength(3)
                .HasColumnName("currency_in");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.RateIn)
                .HasColumnType("decimal(18, 10)")
                .HasColumnName("rate_in");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
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
            entity.Property(e => e.Balance)
                .HasComputedColumnSql("([dbo].[GetTotalTransactionValue]([id]))", false)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
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
