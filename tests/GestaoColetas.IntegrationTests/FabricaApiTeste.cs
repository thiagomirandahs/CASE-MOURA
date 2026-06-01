using GestaoColetas.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestaoColetas.IntegrationTests;

/// <summary>
/// Sobe a API de verdade (em memória), trocando o banco real (SQL Server/Postgres) por um
/// SQLite em memória — assim os testes de integração não precisam de banco externo.
/// </summary>
public class FabricaApiTeste : WebApplicationFactory<Program>
{
    // O SQLite "em memória" só existe enquanto esta conexão estiver aberta.
    private readonly SqliteConnection _conexao = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // faz o Program pular o auto-migrate

        builder.ConfigureTestServices(services =>
        {
            // remove o registro do banco real e coloca o SQLite no lugar
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            _conexao.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_conexao));
        });
    }

    /// <summary>Cria o schema e popula a seed no banco de teste (idempotente).</summary>
    public void GarantirBancoSemeado()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        if (!db.Clientes.Any())
            SeedData.SeedAsync(db).GetAwaiter().GetResult();
    }

    protected override void Dispose(bool disposing)
    {
        _conexao.Dispose();
        base.Dispose(disposing);
    }
}
