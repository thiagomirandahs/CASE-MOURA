# CASE-MOURA — Gestão de Coletas

Sistema interno pra uma transportadora organizar as solicitações de coleta, desde
quando o pedido é aberto até a confirmação de que a carga foi coletada. A ideia é
tirar esse controle das planilhas e do WhatsApp e centralizar num lugar só, com
rastreabilidade do que acontece em cada coleta.

> Projeto do case técnico pra vaga de Desenvolvedor Web Jr.

## Demo ao vivo

- **Aplicação:** https://case-moura-web.onrender.com — entre com **admin** / **admin123**
- **API (Swagger):** https://case-moura.onrender.com/swagger/index.html

> Está numa hospedagem gratuita que "dorme" quando fica parada: a primeira visita pode levar uns ~50s pra acordar (se aparecer "Not Found", é só recarregar uma vez).

## O que dá pra fazer

- Abrir uma solicitação de coleta (cliente, remetente, destinatário, data prevista e prioridade)
- Acompanhar o status de cada coleta: **Aberta → Em Coleta → Coletado**, ou **Cancelada**
- Atribuir motorista e veículo (é isso que move a coleta pra "Em Coleta")
- Marcar como coletada e cancelar, sempre respeitando as regras de negócio
- Registrar **ocorrências** (ex.: endereço errado, cliente ausente), guardando data/hora e quem registrou
- Filtrar a lista por situação, cliente e período, com busca e paginação
- Ver um **dashboard** com indicadores (total, por status, em atraso, alta prioridade e taxa de conclusão)
- **Exportar** as coletas em CSV (abre no Excel)
- Entrar com login (autenticação JWT)

## Tecnologias

- Back-end: C# / .NET 8 (Web API), seguindo Clean Architecture
- Banco de dados: SQL Server com Entity Framework Core (migrations + seed)
- Front-end: React (Vite)
- Autenticação: JWT
- Logs: Serilog
- Documentação da API: Swagger
- Testes: xUnit
- Infra: Docker e Docker Compose

## Como o projeto está organizado

O back-end segue Clean Architecture, separado em camadas:

```
src/
  GestaoColetas.Domain          entidades e regras de negócio (o núcleo)
  GestaoColetas.Application     casos de uso, DTOs e contratos (interfaces)
  GestaoColetas.Infrastructure  acesso a dados (EF Core / SQL Server)
  GestaoColetas.WebAPI          a API: controllers e configuração
tests/
  GestaoColetas.Tests           testes das regras de negócio (xUnit)
frontend/                       o front em React
```

A regra principal: as camadas de fora dependem das de dentro, e o Domain não depende
de ninguém — assim as regras ficam isoladas e fáceis de testar. O porquê de cada
escolha eu escrevi em [docs/DECISOES.md](docs/DECISOES.md).

## As regras de negócio (o coração do sistema)

Essas regras moram dentro da própria entidade `SolicitacaoColeta`, não espalhadas pela aplicação:

- O status só anda pelo caminho certo: **Aberta → Em Coleta → Coletado**, ou **Cancelada**
- **Cancelada é ponto final**: uma coleta cancelada não volta pro fluxo
- Só dá pra marcar como **Coletado** se tiver motorista **E** veículo vinculados
- Toda **ocorrência** guarda data/hora e o usuário responsável
- Coleta de prioridade **Alta** aparece destacada e no topo da lista

## Como rodar

Precisa ter o Docker e o .NET 8 instalados. Se for montar a máquina do zero, anotei o
passo a passo em [docs/AMBIENTE.md](docs/AMBIENTE.md).

### Opção 1 — Docker (sobe o banco + a API juntos)

Na raiz do projeto:

```bash
docker compose up --build
```

Sobe o SQL Server e a API. Quando terminar de subir, a API fica em:

- Swagger: http://localhost:8080/swagger

Pra parar: `docker compose down`.

### Opção 2 — Rodar local (pra desenvolver)

1. Subir só o banco (SQL Server num contêiner):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=GestaoColetas@2026" -p 1433:1433 -d --name sql-gestaocoletas mcr.microsoft.com/mssql/server:2022-latest
```

2. Rodar a API (na porta 5080, que é a que o front procura):

```bash
dotnet run --project src/GestaoColetas.WebAPI --urls http://localhost:5080
```

Swagger em http://localhost:5080/swagger.

> Na primeira vez, a API cria o banco (migrations) e popula com dados de exemplo (seed) sozinha.

### Front-end (React)

Em outro terminal:

```bash
cd frontend
npm install
npm run dev
```

O front abre em http://localhost:5173. Por padrão ele conversa com a API em
`http://localhost:5080` (a opção 2). Se você subiu pela opção 1 (Docker, porta 8080),
crie um arquivo `frontend/.env` com a linha:

```
VITE_API_URL=http://localhost:8080
```

### Login

O sistema é protegido por JWT. O usuário de demonstração é:

- **usuário:** `admin`
- **senha:** `admin123`

## Rodar os testes

```bash
dotnet test
```

São 12 testes (xUnit) que cobrem as regras de transição de status direto no domínio.

## A API

Com a API no ar, o **Swagger** documenta e deixa testar todos os endpoints (dá pra
fazer o login por lá e usar o token nos demais). Os principais:

| Método | Rota | O que faz |
|---|---|---|
| POST | `/api/auth/login` | Autentica e devolve o token JWT |
| GET | `/api/coletas` | Lista as coletas (filtros, busca e paginação) |
| POST | `/api/coletas` | Abre uma nova coleta |
| POST | `/api/coletas/{id}/atribuir` | Atribui motorista e veículo (vai pra "Em Coleta") |
| POST | `/api/coletas/{id}/coletar` | Marca como coletada |
| POST | `/api/coletas/{id}/cancelar` | Cancela a coleta |
| POST | `/api/coletas/{id}/ocorrencias` | Registra uma ocorrência |
| GET | `/api/coletas/exportar` | Exporta as coletas em CSV |
| GET | `/api/dashboard` | Os indicadores do dashboard |
| GET | `/api/clientes` · `/api/motoristas` · `/api/veiculos` | Cadastros (usados nos formulários) |

## Documentação

- [docs/DECISOES.md](docs/DECISOES.md) — por que cada escolha de arquitetura
- [docs/AMBIENTE.md](docs/AMBIENTE.md) — como preparei a máquina pra rodar o projeto
