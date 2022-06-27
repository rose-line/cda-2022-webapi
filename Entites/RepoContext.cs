using Entites.Configs;
using Entites.Models;
using Microsoft.EntityFrameworkCore;

namespace Entites
{
  public class RepoContext : DbContext
  {

    public RepoContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      builder.ApplyConfiguration(new ConfigEntreprise());
      builder.ApplyConfiguration(new ConfigEmploye());
    }

    public DbSet<Entreprise> Entreprises { get; set; }

    public DbSet<Employe> Employes { get; set; }
  }
}
