﻿// <auto-generated />
using System;
using Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WebApiSample.Migrations
{
    [DbContext(typeof(RepoContext))]
    [Migration("20210603110211_JeuDeDonneesSimple")]
    partial class JeuDeDonneesSimple
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Entites.Models.Employe", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("EmployeId");

                    b.Property<int>("Age")
                        .HasColumnType("int");

                    b.Property<Guid>("EntrepriseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Poste")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("EntrepriseId");

                    b.ToTable("Employes");

                    b.HasData(
                        new
                        {
                            Id = new Guid("f4263866-da7b-4d34-8804-d2338575c320"),
                            Age = 18,
                            EntrepriseId = new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"),
                            Nom = "Employé 01",
                            Poste = "Concepteur logiciel"
                        },
                        new
                        {
                            Id = new Guid("c2efc608-3741-4acf-89d4-f82f0211c31f"),
                            Age = 20,
                            EntrepriseId = new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"),
                            Nom = "Employé 02",
                            Poste = "Architecte logiciel"
                        },
                        new
                        {
                            Id = new Guid("80ec219c-5a54-4670-96ad-92969a997454"),
                            Age = 19,
                            EntrepriseId = new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09"),
                            Nom = "Employe 03",
                            Poste = "Admin"
                        });
                });

            modelBuilder.Entity("Entites.Models.Entreprise", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("EntrepriseId");

                    b.Property<string>("Adresse")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Pays")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Entreprises");

                    b.HasData(
                        new
                        {
                            Id = new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"),
                            Adresse = "Adresse01 chemin du 01",
                            Nom = "Entreprise01 SA",
                            Pays = "FR"
                        },
                        new
                        {
                            Id = new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09"),
                            Adresse = "Adresse02 rue du 02",
                            Nom = "Entreprise02 SARL",
                            Pays = "BE"
                        });
                });

            modelBuilder.Entity("Entites.Models.Employe", b =>
                {
                    b.HasOne("Entites.Models.Entreprise", "entreprise")
                        .WithMany("Employes")
                        .HasForeignKey("EntrepriseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("entreprise");
                });

            modelBuilder.Entity("Entites.Models.Entreprise", b =>
                {
                    b.Navigation("Employes");
                });
#pragma warning restore 612, 618
        }
    }
}
