using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Application.Services;
using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;
using Moq;
using Xunit;

namespace GestaoColetas.Tests;

/// <summary>
/// Testes do ColetaService usando Moq (mock do repositório) — testa a camada de aplicação
/// sem precisar de banco de verdade.
/// </summary>
public class ColetaServiceTests
{
    private static CriarColetaRequest RequestValido() => new(
        ClienteId: 1, RemetenteNome: "Rem", RemetenteEndereco: "EndA",
        DestinatarioNome: "Dest", DestinatarioEndereco: "EndB",
        DataColetaPrevista: DateTime.UtcNow.AddDays(1), Prioridade: Prioridade.Alta, Observacoes: null);

    private static SolicitacaoColeta UmaColeta() => new(
        "COL-2026-0001", 1, "Rem", "EndA", "Dest", "EndB",
        DateTime.UtcNow.AddDays(1), Prioridade.Alta);

    [Fact]
    public async Task CriarAsync_ClienteNaoExiste_LancaExcecao()
    {
        var repo = new Mock<IColetaRepository>();
        repo.Setup(r => r.ClienteExisteAsync(It.IsAny<int>())).ReturnsAsync(false);
        var service = new ColetaService(repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CriarAsync(RequestValido()));
    }

    [Fact]
    public async Task CriarAsync_ClienteExiste_AdicionaESalva()
    {
        var repo = new Mock<IColetaRepository>();
        repo.Setup(r => r.ClienteExisteAsync(1)).ReturnsAsync(true);
        repo.Setup(r => r.ContarAsync()).ReturnsAsync(5);
        repo.Setup(r => r.AdicionarAsync(It.IsAny<SolicitacaoColeta>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.SalvarAlteracoesAsync()).Returns(Task.CompletedTask);
        repo.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync(UmaColeta());

        var service = new ColetaService(repo.Object);
        var resp = await service.CriarAsync(RequestValido());

        Assert.NotNull(resp);
        repo.Verify(r => r.AdicionarAsync(It.IsAny<SolicitacaoColeta>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
    }

    [Fact]
    public async Task ListarAsync_DevolveResultadoPaginado()
    {
        var repo = new Mock<IColetaRepository>();
        var itens = new List<SolicitacaoColeta> { UmaColeta(), UmaColeta() };
        repo.Setup(r => r.ListarAsync(
                It.IsAny<StatusColeta?>(), It.IsAny<int?>(), It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((itens, 2));

        var service = new ColetaService(repo.Object);
        var resultado = await service.ListarAsync(null, null, null, null, 1, 10);

        Assert.Equal(2, resultado.Total);
        Assert.Equal(2, resultado.Itens.Count);
    }

    [Fact]
    public async Task MarcarComoColetadaAsync_ColetaNaoExiste_LancaKeyNotFound()
    {
        var repo = new Mock<IColetaRepository>();
        repo.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((SolicitacaoColeta?)null);
        var service = new ColetaService(repo.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.MarcarComoColetadaAsync(99));
    }

    [Fact]
    public async Task ObterPorIdAsync_ColetaAlta_MapeiaComPrioridadeAlta()
    {
        var repo = new Mock<IColetaRepository>();
        repo.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(UmaColeta()); // prioridade Alta
        var service = new ColetaService(repo.Object);

        var resp = await service.ObterPorIdAsync(1);

        Assert.NotNull(resp);
        Assert.True(resp!.PrioridadeAlta);
        Assert.Equal("Alta", resp.Prioridade);
    }
}
