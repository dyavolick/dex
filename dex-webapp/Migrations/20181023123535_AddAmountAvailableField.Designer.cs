﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using dex_webapp.Data;

namespace dex_webapp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20181023123535_AddAmountAvailableField")]
    partial class AddAmountAvailableField
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("dex_webapp.Models.ActivateTokenEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("BlockNum");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<string>("Name");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("Token");

                    b.Property<string>("TransactionHash");

                    b.HasKey("Id");

                    b.ToTable("ActivateTokenEvent");
                });

            modelBuilder.Entity("dex_webapp.Models.CancelEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AmountGet");

                    b.Property<string>("AmountGive");

                    b.Property<long>("BlockNum");

                    b.Property<string>("Expires");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<string>("Hash");

                    b.Property<string>("Nonce");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("TokenGet");

                    b.Property<string>("TokenGive");

                    b.Property<string>("TransactionHash");

                    b.Property<string>("User");

                    b.HasKey("Id");

                    b.HasIndex("User");

                    b.ToTable("CancelEvent");
                });

            modelBuilder.Entity("dex_webapp.Models.DeactivateTokenEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("BlockNum");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<string>("Name");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("Token");

                    b.Property<string>("TransactionHash");

                    b.HasKey("Id");

                    b.ToTable("DeactivateTokenEvent");
                });

            modelBuilder.Entity("dex_webapp.Models.DepositEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Amount");

                    b.Property<string>("Balance");

                    b.Property<long>("BlockNum");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("Token");

                    b.Property<string>("TransactionHash");

                    b.Property<string>("User");

                    b.HasKey("Id");

                    b.ToTable("DepositEvent");
                });

            modelBuilder.Entity("dex_webapp.Models.OhlcData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Close");

                    b.Property<string>("CurrencyId");

                    b.Property<DateTime>("Date");

                    b.Property<decimal>("Max");

                    b.Property<decimal>("Min");

                    b.Property<decimal>("Open");

                    b.Property<int>("Range");

                    b.Property<decimal>("Volume");

                    b.Property<decimal>("VolumeBase");

                    b.HasKey("Id");

                    b.HasIndex("Date");

                    b.ToTable("OHLCData");
                });

            modelBuilder.Entity("dex_webapp.Models.OrderEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AmountGet");

                    b.Property<string>("AmountGive");

                    b.Property<long>("BlockNum");

                    b.Property<string>("Expires");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<string>("Hash");

                    b.Property<string>("Nonce");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("TokenGet");

                    b.Property<string>("TokenGive");

                    b.Property<string>("TransactionHash");

                    b.Property<string>("User");

                    b.HasKey("Id");

                    b.HasIndex("User");

                    b.ToTable("OrderEvent");
                });

            modelBuilder.Entity("dex_webapp.Models.OrderFilledModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AmountAvailable");

                    b.Property<string>("AmountFilled");

                    b.Property<string>("Hash");

                    b.Property<bool>("IsDone");

                    b.HasKey("Id");

                    b.ToTable("OrderFilled");
                });

            modelBuilder.Entity("dex_webapp.Models.ServiceParameter", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Parameters");
                });

            modelBuilder.Entity("dex_webapp.Models.TokenModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Image");

                    b.Property<string>("Name");

                    b.Property<int>("Status");

                    b.Property<long>("StatusBlockUpdate");

                    b.Property<DateTime>("StatusDateUpdate");

                    b.Property<string>("Token");

                    b.HasKey("Id");

                    b.ToTable("Token");
                });

            modelBuilder.Entity("dex_webapp.Models.TradeEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AmountGet");

                    b.Property<string>("AmountGive");

                    b.Property<long>("BlockNum");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<string>("Get");

                    b.Property<string>("Give");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("TokenGet");

                    b.Property<string>("TokenGive");

                    b.Property<string>("TransactionHash");

                    b.HasKey("Id");

                    b.ToTable("TradeEvent");
                });

            modelBuilder.Entity("dex_webapp.Models.WithdrawEventModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Amount");

                    b.Property<string>("Balance");

                    b.Property<long>("BlockNum");

                    b.Property<long>("GasPriceWei");

                    b.Property<long>("GasUsed");

                    b.Property<DateTimeOffset?>("Timestamp");

                    b.Property<string>("Token");

                    b.Property<string>("TransactionHash");

                    b.Property<string>("User");

                    b.HasKey("Id");

                    b.ToTable("WithdrawEvent");
                });
#pragma warning restore 612, 618
        }
    }
}
