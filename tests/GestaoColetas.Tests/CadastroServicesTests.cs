using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Application.Services;
using GestaoColetas.Domain.Entities;
using Moq;
using Xunit;

namespace GestaoColetas.Tests;

/// <summary>
/// Testes dos services de cadastro (Motorista e Veiculo) usando Moq —
/// cobrem a regra de CNH e placa únicas.
/// </summary>
public class CadastroServicesTests
{
    // ---------- Motorista: CNH única ----------

    [Fact]
    public async Task CriarMotorista_CnhJaCadastrada_LancaExcecao()
    {
        var repo = new Mock<IMotoristaRepository>();
        repo.Setup(r => r.CnhExisteAsync("12345678900")).ReturnsAsync(true);
        var service = new MotoristaService(repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(new CriarMotoristaRequest("Carlos Souza", "12345678900")));

        repo.Verify(r => r.AdicionarAsync(It.IsAny<Motorista>()), Times.Never);
    }

    [Fact]
    public async Task CriarMotorista_CnhNova_AdicionaESalva()
    {
        var repo = new Mock<IMotoristaRepository>();
        repo.Setup(r => r.CnhExisteAsync(It.IsAny<string>())).ReturnsAsync(false);
        repo.Setup(r => r.AdicionarAsync(It.IsAny<Motorista>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.SalvarAlteracoesAsync()).Returns(Task.CompletedTask);
        var service = new MotoristaService(repo.Object);

        var resp = await service.CriarAsync(new CriarMotoristaRequest("Carlos Souza", " 12345678900 "));

        Assert.Equal("12345678900", resp.Cnh); // entra com espaços, sai normalizada
        repo.Verify(r => r.AdicionarAsync(It.IsAny<Motorista>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
    }

    // ---------- Veiculo: placa única ----------

    [Fact]
    public async Task CriarVeiculo_PlacaJaCadastrada_LancaExcecao()
    {
        var repo = new Mock<IVeiculoRepository>();
        repo.Setup(r => r.PlacaExisteAsync("ABC1D23")).ReturnsAsync(true);
        var service = new VeiculoService(repo.Object);

        // mesmo digitada em minúsculas, conta como a mesma placa
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(new CriarVeiculoRequest("abc1d23", "Fiat Fiorino")));

        repo.Verify(r => r.AdicionarAsync(It.IsAny<Veiculo>()), Times.Never);
    }

    [Fact]
    public async Task CriarVeiculo_PlacaNova_AdicionaESalvaEmMaiusculas()
    {
        var repo = new Mock<IVeiculoRepository>();
        repo.Setup(r => r.PlacaExisteAsync(It.IsAny<string>())).ReturnsAsync(false);
        repo.Setup(r => r.AdicionarAsync(It.IsAny<Veiculo>())).Returns(Task.CompletedTask);
        repo.Setup(r => r.SalvarAlteracoesAsync()).Returns(Task.CompletedTask);
        var service = new VeiculoService(repo.Object);

        var resp = await service.CriarAsync(new CriarVeiculoRequest("abc1d23", "Fiat Fiorino"));

        Assert.Equal("ABC1D23", resp.Placa); // a placa é normalizada para maiúsculas
        repo.Verify(r => r.AdicionarAsync(It.IsAny<Veiculo>()), Times.Once);
        repo.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
    }
}
