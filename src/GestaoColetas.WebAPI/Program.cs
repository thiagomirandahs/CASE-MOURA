using GestaoColetas.Application.Services;
using GestaoColetas.Infrastructure;
using GestaoColetas.Infrastructure.Data;
using GestaoColetas.WebAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco de dados (EF Core + SQL Server)
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

// Casos de uso da aplicação
builder.Services.AddScoped<IColetaService, ColetaService>();

var app = builder.Build();

// No startup: aplica as migrations (cria/atualiza as tabelas) e popula o seed inicial.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // O banco pode demorar a ficar pronto (ex.: subindo junto no Docker). Tenta algumas vezes.
    for (var tentativa = 1; ; tentativa++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch when (tentativa < 10) { await Task.Delay(TimeSpan.FromSeconds(5)); }
    }

    await SeedData.SeedAsync(db);
}

// Tratamento centralizado de erros (transforma exceções em respostas HTTP claras).
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
