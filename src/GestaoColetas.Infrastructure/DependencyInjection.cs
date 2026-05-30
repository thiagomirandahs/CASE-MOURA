using GestaoColetas.Application.Interfaces;
using GestaoColetas.Infrastructure.Data;
using GestaoColetas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoColetas.Infrastructure;

/// <summary>
/// Registra os serviços da camada de Infraestrutura (banco de dados) no app.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string? provider = null)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            // SQL Server no local/Docker; PostgreSQL na nuvem — escolhido por configuracao.
            var ehPostgres = (provider ?? "").Trim().ToLowerInvariant() == "postgres";
            if (ehPostgres)
                options.UseNpgsql(connectionString);
            else
                options.UseSqlServer(connectionString);
        });

        services.AddScoped<IColetaRepository, ColetaRepository>();
        services.AddScoped<IMotoristaRepository, MotoristaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();

        return services;
    }
}
