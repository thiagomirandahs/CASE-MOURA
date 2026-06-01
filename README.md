# CASE-MOURA — Gestão de Coletas

Sistema interno para uma transportadora organizar as solicitações de coleta, desde a abertura do pedido até a confirmação de que a carga foi coletada. O objetivo é remover esse controle de planilhas e do WhatsApp e centralizá-lo em um único lugar, com rastreabilidade do que ocorre em cada coleta.

> Projeto de case técnico para a vaga de Desenvolvedor Web Jr.

## Demo ao vivo

- **Aplicação:** https://case-moura-web.onrender.com — acesse com **admin** / **admin123**
- **API (Swagger):** https://case-moura.onrender.com/swagger/index.html

> A aplicação está em hospedagem gratuita, que entra em suspensão quando ociosa: a primeira visita pode levar cerca de 50 segundos para iniciar (caso apareça "Not Found", basta recarregar a página).

## Funcionalidades

- Abertura de solicitação de coleta (cliente, remetente, destinatário, data prevista e prioridade)
- Acompanhamento do status de cada coleta: **Aberta → Em Coleta → Coletado**, ou **Cancelada**
- Atribuição de motorista e veículo (ação que move a coleta para "Em Coleta")
- Marcação como coletada e cancelamento, sempre respeitando as regras de negócio
- Registro de **ocorrências** (ex.: endereço incorreto, cliente ausente), com data/hora e responsável
- Filtro da lista por situação, cliente e período, com busca e paginação
- **Dashboard** com indicadores (total, por status, em atraso, alta prioridade e taxa de conclusão)
- **Exportação** das coletas em CSV (compatível com o Excel)
- Acesso autenticado (JWT)

## Tecnologias

- Back-end: C# / .NET 8 (Web API), seguindo Clean Architecture
- Banco de dados: SQL Server com Entity Framework Core (migrations + seed)
- Front-end: React (Vite)
- Autenticação: JWT
- Logs: Serilog
- Documentação da API: Swagger
- Testes: xUnit
- Infraestrutura: Docker e Docker Compose

## Organização do projeto

O back-end segue Clean Architecture, separado em camadas:

```
src/
  GestaoColetas.Domain          entidades e regras de negócio (o núcleo)
  GestaoColetas.Application     casos de uso, DTOs e contratos (interfaces)
  GestaoColetas.Infrastructure  acesso a dados (EF Core / SQL Server)
  GestaoColetas.WebAPI          a API: controllers e configuração
tests/
  GestaoColetas.Tests           testes das regras de negócio (xUnit)
frontend/                       o front-end em React
```

Princípio central: as camadas externas dependem das internas, e o Domain não depende de nenhuma outra — assim as regras permanecem isoladas e testáveis. A justificativa de cada escolha está em [docs/DECISOES.md](docs/DECISOES.md).

## Regras de negócio (núcleo do sistema)

As regras residem na própria entidade `SolicitacaoColeta`, e não dispersas pela aplicação:

- O status percorre apenas o fluxo válido: **Aberta → Em Coleta → Coletado**, ou **Cancelada**
- **Cancelada é estado terminal**: uma coleta cancelada não retorna ao fluxo
- Só é possível marcar como **Coletado** com motorista **E** veículo vinculados
- Toda **ocorrência** registra data/hora e o usuário responsável
- Coletas de prioridade **Alta** aparecem destacadas e no topo da lista

## Como executar

É necessário ter o Docker e o .NET 8 instalados. O passo a passo para preparar a máquina do zero está em [docs/AMBIENTE.md](docs/AMBIENTE.md).

### Opção 1 — Docker (banco + API juntos)

Na raiz do projeto:

```bash
docker compose up --build
```

Sobe o SQL Server e a API. Após a inicialização, a API fica disponível em:

- Swagger: http://localhost:8080/swagger

Para encerrar: `docker compose down`.

### Opção 2 — Execução local (desenvolvimento)

1. Suba apenas o banco (SQL Server em contêiner):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=GestaoColetas@2026" -p 1433:1433 -d --name sql-gestaocoletas mcr.microsoft.com/mssql/server:2022-latest
```

2. Execute a API (na porta 5080, utilizada pelo front-end):

```bash
dotnet run --project src/GestaoColetas.WebAPI --urls http://localhost:5080
```

Swagger em http://localhost:5080/swagger.

> Na primeira execução, a API cria o banco (migrations) e o popula com dados de exemplo (seed) automaticamente.

### Front-end (React)

Em outro terminal:

```bash
cd frontend
npm install
npm run dev
```

O front-end abre em http://localhost:5173. Por padrão, comunica-se com a API em `http://localhost:5080` (Opção 2). Caso utilize a Opção 1 (Docker, porta 8080), crie o arquivo `frontend/.env` com a linha:

```
VITE_API_URL=http://localhost:8080
```

### Login

O sistema é protegido por JWT. O usuário de demonstração é:

- **usuário:** `admin`
- **senha:** `admin123`

## Testes

```bash
dotnet test
```

São **47 testes** no total: **41 unitários** (regras de transição de status no domínio, validações das entidades e o `ColetaService` testado com Moq) e **6 de integração**, que executam a API real (via `WebApplicationFactory` + SQLite em memória) e testam os endpoints HTTP de ponta a ponta.

## A API

Com a API em execução, o **Swagger** documenta e permite testar todos os endpoints (incluindo o login e o uso do token nas demais chamadas). Os principais:

| Método | Rota | O que faz |
|---|---|---|
| POST | `/api/auth/login` | Autentica e devolve o token JWT |
| GET | `/api/coletas` | Lista as coletas (filtros, busca e paginação) |
| POST | `/api/coletas` | Abre uma nova coleta |
| POST | `/api/coletas/{id}/atribuir` | Atribui motorista e veículo (vai para "Em Coleta") |
| POST | `/api/coletas/{id}/coletar` | Marca como coletada |
| POST | `/api/coletas/{id}/cancelar` | Cancela a coleta |
| POST | `/api/coletas/{id}/ocorrencias` | Registra uma ocorrência |
| GET | `/api/coletas/exportar` | Exporta as coletas em CSV |
| GET | `/api/dashboard` | Indicadores do dashboard |
| GET | `/api/clientes` · `/api/motoristas` · `/api/veiculos` | Cadastros (usados nos formulários) |

## Documentação

- [docs/DECISOES.md](docs/DECISOES.md) — justificativa de cada escolha de arquitetura
- [docs/AMBIENTE.md](docs/AMBIENTE.md) — preparação da máquina para executar o projeto
