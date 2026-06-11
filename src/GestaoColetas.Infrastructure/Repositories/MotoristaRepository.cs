using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;
using GestaoColetas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoColetas.Infrastructure.Repositories;

/// <summary>
/// Implementação do contrato de motoristas usando EF Core (SQL Server).
/// </summary>
public class MotoristaRepository : IMotoristaRepository
{
    private readonly AppDbContext _db;

    public MotoristaRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Motorista>> ListarAsync() =>
        await _db.Motoristas.ToListAsync();

    public async Task<Motorista?> ObterPorIdAsync(int id) =>
        await _db.Motoristas.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<bool> CnhExisteAsync(string cnh) =>
        await _db.Motoristas.AnyAsync(m => m.Cnh == cnh);

    public async Task AdicionarAsync(Motorista motorista) =>
        await _db.Motoristas.AddAsync(motorista);

    public Task SalvarAlteracoesAsync() => _db.SaveChangesAsync();
}