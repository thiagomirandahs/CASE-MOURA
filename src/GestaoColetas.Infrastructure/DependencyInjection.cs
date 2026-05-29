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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IColetaRepository, ColetaRepository>();

        return services;
    }
}
