using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestaoColetas.Infrastructure.Data;

/// <summary>
/// Popula o banco com uma massa de dados inicial (só se ainda estiver vazio).
/// Usa as próprias regras do domínio pra montar coletas em situações variadas.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Clientes.AnyAsync())
            return; // já tem dados — não duplica

        var mercado = new Cliente("Mercado Central LTDA", "12.345.678/0001-90");
        var farmacia = new Cliente("Farmácia Saúde", "98.765.432/0001-10");
        var loja = new Cliente("Loja TechPlus", "11.222.333/0001-44");
        context.Clientes.AddRange(mercado, farmacia, loja);

        var carlos = new Motorista("Carlos Souza", "12345678900");
        var marina = new Motorista("Marina Lima", "98765432100");
        context.Motoristas.AddRange(carlos, marina);

        var fiorino = new Veiculo("ABC1D23", "Fiat Fiorino");
        var hr = new Veiculo("XYZ4E56", "Hyundai HR");
        context.Veiculos.AddRange(fiorino, hr);

        await context.SaveChangesAsync(); // gera os Ids dos cadastros acima

        var hoje = DateTime.UtcNow;

        // Aberta + prioridade alta
        var c1 = new SolicitacaoColeta("COL-2026-0001", mercado.Id, "Mercado Central", "Av. Brasil, 1000",
            "Distribuidora Norte", "Rua das Flores, 50", hoje.AddDays(1), Prioridade.Alta, "Carga frágil");

        // Aberta + normal
        var c2 = new SolicitacaoColeta("COL-2026-0002", farmacia.Id, "Farmácia Saúde", "Rua A, 200",
            "Hospital Vida", "Rua B, 300", hoje.AddDays(2), Prioridade.Normal);

        // Em coleta (motorista e veículo atribuídos)
        var c3 = new SolicitacaoColeta("COL-2026-0003", loja.Id, "Loja TechPlus", "Av. Paulista, 900",
            "Cliente Final", "Rua C, 10", hoje.AddDays(1), Prioridade.Normal);
        c3.AtribuirMotoristaEVeiculo(carlos.Id, fiorino.Id);

        // Coletado (fluxo completo)
        var c4 = new SolicitacaoColeta("COL-2026-0004", mercado.Id, "Mercado Central", "Av. Brasil, 1000",
            "Filial Sul", "Rua D, 80", hoje.AddDays(-1), Prioridade.Alta);
        c4.AtribuirMotoristaEVeiculo(marina.Id, hr.Id);
        c4.MarcarComoColetada();

        // Cancelada (com uma ocorrência registrada antes)
        var c5 = new SolicitacaoColeta("COL-2026-0005", farmacia.Id, "Farmácia Saúde", "Rua A, 200",
            "Destinatário X", "Endereço a confirmar", hoje, Prioridade.Baixa);
        c5.RegistrarOcorrencia("Endereço do destinatário incorreto", "atendente.ana");
        c5.Cancelar();

        context.Coletas.AddRange(c1, c2, c3, c4, c5);
        await context.SaveChangesAsync();
    }
}
