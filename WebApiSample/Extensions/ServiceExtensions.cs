using Contrats;
using Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using ServiceLogging;

namespace WebApiSample.Extensions
{
  public static class ServiceExtensions
  {
    public static void ConfigureServiceLogging(this IServiceCollection services) => services.AddScoped<ILoggable, Logger>();

    public static void ConfigureContextSql(this IServiceCollection services, IConfiguration config) => services.AddDbContext<RepoContext>(opts => opts.UseSqlServer(config.GetConnectionString("sqlConn"), opts => opts.MigrationsAssembly("WebApiSample")));

    public static void ConfigureGestionRepos(this IServiceCollection services) => services.AddScoped<IGestionRepos, GestionRepos>();
  }
}
