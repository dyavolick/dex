using System;
using System.Collections.Generic;
using System.Text;
using dex_webapp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dex_webapp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<OhlcData> OHLCData { get; set; }
        public DbSet<ServiceParameter> Parameters { get; set; }

        public DbSet<TokenModel> Token { get; set; }

        public DbSet<OrderFilledModel> OrderFilled { get; set; }
        public DbSet<ActivateTokenEventModel> ActivateTokenEvent { get; set; }
        public DbSet<CancelEventModel> CancelEvent { get; set; }
        public DbSet<DeactivateTokenEventModel> DeactivateTokenEvent { get; set; }
        public DbSet<DepositEventModel> DepositEvent { get; set; }
        public DbSet<OrderEventModel> OrderEvent { get; set; }
        public DbSet<TradeEventModel> TradeEvent { get; set; }
        public DbSet<WithdrawEventModel> WithdrawEvent { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            //builder.Entity<ServiceParameter>().HasData(
            //new ServiceParameter[]
            //{
            //    new ServiceParameter { Id=1, Key="Tom", Value=""},
            //});
            base.OnModelCreating(builder);
            builder.Entity<OhlcData>()
                .HasIndex(b => b.Date);
            builder.Entity<OrderEventModel>()
                .HasIndex(x => x.User);

            builder.Entity<CancelEventModel>()
                .HasIndex(x => x.User);
        }
    }
}
