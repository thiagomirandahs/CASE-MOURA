using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;
using GestaoColetas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoColetas.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly AppDbContext _db;

    public VeiculoRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Veiculo>> ListarAsync() =>
        await _db.Veiculos.OrderBy(v => v.Placa).ToListAsync();

    public async Task<Veiculo?> ObterPorIdAsync(int id) =>
        await _db.Veiculos.FirstOrDefaultAsync(v => v.Id == id);

    public async Task AdicionarAsync(Veiculo veiculo) => await _db.Veiculos.AddAsync(veiculo);

    public Task SalvarAlteracoesAsync() => _db.SaveChangesAsync();
}
