using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;
using GestaoColetas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoColetas.Infrastructure.Repositories;

/// <summary>
/// Implementação do contrato de coletas usando EF Core (SQL Server).
/// </summary>
public class ColetaRepository : IColetaRepository
{
    private readonly AppDbContext _db;

    public ColetaRepository(AppDbContext db) => _db = db;

    public async Task<SolicitacaoColeta?> ObterPorIdAsync(int id) =>
        await ComIncludes(_db.Coletas).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<(IReadOnlyList<SolicitacaoColeta> Itens, int Total)> ListarAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim, int pagina, int tamanhoPagina)
    {
        var query = ComIncludes(_db.Coletas);

        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (clienteId.HasValue) query = query.Where(c => c.ClienteId == clienteId.Value);
        if (inicio.HasValue) query = query.Where(c => c.DataColetaPrevista >= inicio.Value);
        if (fim.HasValue) query = query.Where(c => c.DataColetaPrevista <= fim.Value);

        var total = await query.CountAsync();

        // Prioridade Alta primeiro, depois pela data prevista, e então pagina no banco.
        var itens = await query
            .OrderBy(c => c.Prioridade == Prioridade.Alta ? 0 : c.Prioridade == Prioridade.Normal ? 1 : 2)
            .ThenBy(c => c.DataColetaPrevista)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }

    public Task<int> ContarAsync() => _db.Coletas.CountAsync();

    public Task<bool> ClienteExisteAsync(int clienteId) => _db.Clientes.AnyAsync(c => c.Id == clienteId);
    public Task<bool> MotoristaExisteAsync(int motoristaId) => _db.Motoristas.AnyAsync(m => m.Id == motoristaId);
    public Task<bool> VeiculoExisteAsync(int veiculoId) => _db.Veiculos.AnyAsync(v => v.Id == veiculoId);

    public async Task AdicionarAsync(SolicitacaoColeta coleta) => await _db.Coletas.AddAsync(coleta);

    public Task SalvarAlteracoesAsync() => _db.SaveChangesAsync();

    public async Task<DashboardResponse> ObterDashboardAsync()
    {
        var agora = DateTime.UtcNow;
        return new DashboardResponse(
            await _db.Coletas.CountAsync(),
            await _db.Coletas.CountAsync(c => c.Status == StatusColeta.Aberta),
            await _db.Coletas.CountAsync(c => c.Status == StatusColeta.EmColeta),
            await _db.Coletas.CountAsync(c => c.Status == StatusColeta.Coletado),
            await _db.Coletas.CountAsync(c => c.Status == StatusColeta.Cancelada),
            await _db.Coletas.CountAsync(c => c.Status != StatusColeta.Coletado && c.Status != StatusColeta.Cancelada && c.DataColetaPrevista < agora),
            await _db.Coletas.CountAsync(c => c.Prioridade == Prioridade.Alta && (c.Status == StatusColeta.Aberta || c.Status == StatusColeta.EmColeta)));
    }

    private static IQueryable<SolicitacaoColeta> ComIncludes(IQueryable<SolicitacaoColeta> q) =>
        q.Include(c => c.Cliente)
         .Include(c => c.Motorista)
         .Include(c => c.Veiculo)
         .Include(c => c.Ocorrencias);
}
