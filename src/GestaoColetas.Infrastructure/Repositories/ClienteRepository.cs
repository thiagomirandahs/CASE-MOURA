using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;
using GestaoColetas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoColetas.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _db;

    public ClienteRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Cliente>> ListarAsync() =>
        await _db.Clientes.OrderBy(c => c.Nome).ToListAsync();

    public async Task<Cliente?> ObterPorIdAsync(int id) =>
        await _db.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public async Task AdicionarAsync(Cliente cliente) => await _db.Clientes.AddAsync(cliente);

    public Task SalvarAlteracoesAsync() => _db.SaveChangesAsync();
}
