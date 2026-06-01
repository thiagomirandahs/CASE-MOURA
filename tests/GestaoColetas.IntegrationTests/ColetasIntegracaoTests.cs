using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace GestaoColetas.IntegrationTests;

// Modelos mínimos só para ler as respostas (campos extras são ignorados).
public record LoginResp(string Token);
public record PaginaColetas(int Total);

/// <summary>
/// Testes de integração: sobem a API de verdade e batem nos endpoints HTTP, testando
/// o pipeline inteiro (rota -> JWT -> controller -> service -> EF -> banco SQLite).
/// </summary>
public class ColetasIntegracaoTests : IClassFixture<FabricaApiTeste>
{
    private readonly FabricaApiTeste _fabrica;

    public ColetasIntegracaoTests(FabricaApiTeste fabrica)
    {
        _fabrica = fabrica;
        _fabrica.GarantirBancoSemeado();
    }

    private static object Credencial(string senha) => new { usuario = "admin", senha };

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var client = _fabrica.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", Credencial("admin123"));
        resp.EnsureSuccessStatusCode();
        var dados = await resp.Content.ReadFromJsonAsync<LoginResp>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dados!.Token);
        return client;
    }

    [Fact]
    public async Task Login_ComCredenciaisCorretas_RetornaToken()
    {
        var client = _fabrica.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", Credencial("admin123"));

        resp.EnsureSuccessStatusCode();
        var dados = await resp.Content.ReadFromJsonAsync<LoginResp>();
        Assert.False(string.IsNullOrWhiteSpace(dados!.Token));
    }

    [Fact]
    public async Task Login_ComSenhaErrada_Retorna401()
    {
        var client = _fabrica.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", Credencial("senha-errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ListarColetas_SemToken_Retorna401()
    {
        var client = _fabrica.CreateClient();
        var resp = await client.GetAsync("/api/coletas");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ListarColetas_ComToken_RetornaDadosDaSeed()
    {
        var client = await ClienteAutenticadoAsync();
        var resp = await client.GetAsync("/api/coletas?pagina=1&tamanhoPagina=20");

        resp.EnsureSuccessStatusCode();
        var pagina = await resp.Content.ReadFromJsonAsync<PaginaColetas>();
        Assert.True(pagina!.Total >= 5); // a seed tem 5 coletas
    }

    [Fact]
    public async Task CriarColeta_ComToken_Retorna201()
    {
        var client = await ClienteAutenticadoAsync();
        var nova = new
        {
            clienteId = 1,
            remetenteNome = "Remetente Teste",
            remetenteEndereco = "Rua X, 10",
            destinatarioNome = "Destinatario Teste",
            destinatarioEndereco = "Rua Y, 20",
            dataColetaPrevista = DateTime.UtcNow.AddDays(2),
            prioridade = "Alta",
            observacoes = "criada num teste de integracao"
        };

        var resp = await client.PostAsJsonAsync("/api/coletas", nova);

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Coletar_SemMotoristaEVeiculo_Retorna400()
    {
        var client = await ClienteAutenticadoAsync();

        // A coleta de id 2 (COL-2026-0002) nasce "Aberta" na seed, sem motorista/veículo.
        var resp = await client.PostAsync("/api/coletas/2/coletar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
