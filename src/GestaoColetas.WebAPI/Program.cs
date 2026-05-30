using GestaoColetas.Application.Services;
using GestaoColetas.Infrastructure;
using GestaoColetas.Infrastructure.Data;
using GestaoColetas.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Em nuvem (ex.: Render), a plataforma define a porta pela variavel de ambiente PORT.
var portaHost = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portaHost))
    builder.WebHost.UseUrls($"http://0.0.0.0:{portaHost}");

// Serilog — logs estruturados no console e em arquivo (um por dia, na pasta logs/).
builder.Host.UseSerilog((context, config) => config
    .WriteTo.Console()
    .WriteTo.File("logs/gestaocoletas-.log", rollingInterval: RollingInterval.Day));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gestão de Coletas",
        Version = "v1",
        Description = "API para gestão de solicitações de coleta de uma transportadora. "
                    + "Permite registrar coletas, acompanhar o status, atribuir motorista e veículo, "
                    + "registrar ocorrências e consultar com filtros."
    });

    // Inclui os comentários (/// <summary>) dos controllers na documentação.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);

    // Botão "Authorize" no Swagger pra enviar o token JWT.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole aqui o token JWT obtido em /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Banco de dados (EF Core) — SQL Server no local/Docker, PostgreSQL na nuvem.
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
if (string.Equals(dbProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // datas sem fuso: evita erro de Kind
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!, dbProvider);

// Casos de uso da aplicação
builder.Services.AddScoped<IColetaService, ColetaService>();
builder.Services.AddScoped<IMotoristaService, MotoristaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

// CORS — permite o front-end (em outro endereço) consumir a API.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Autenticação JWT — valida o token enviado no header Authorization.
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

var app = builder.Build();

// No startup: aplica as migrations (cria/atualiza as tabelas) e popula o seed inicial.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // O banco pode demorar a ficar pronto (ex.: subindo junto no Docker). Tenta algumas vezes.
    var ehPostgres = string.Equals(dbProvider, "Postgres", StringComparison.OrdinalIgnoreCase);
    for (var tentativa = 1; ; tentativa++)
    {
        try
        {
            // No Postgres (nuvem) cria o schema pelo modelo; no SQL Server aplica as migrations.
            if (ehPostgres) await db.Database.EnsureCreatedAsync();
            else await db.Database.MigrateAsync();
            break;
        }
        catch when (tentativa < 10) { await Task.Delay(TimeSpan.FromSeconds(5)); }
    }

    await SeedData.SeedAsync(db);
}

// Loga cada requisição HTTP (método, rota, status e tempo de resposta).
app.UseSerilogRequestLogging();

// Tratamento centralizado de erros (transforma exceções em respostas HTTP claras).
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
