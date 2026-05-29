# CASE-MOURA — Gestão de Coletas

Sistema interno pra uma transportadora organizar as solicitações de coleta, desde
quando o pedido é aberto até a confirmação de que a carga foi coletada. A ideia é
tirar esse controle das planilhas e do WhatsApp e centralizar num lugar só, com
rastreabilidade do que acontece em cada coleta.

> Projeto do case técnico pra vaga de Desenvolvedor Web Jr.

## Tecnologias

- Back-end: C# / .NET 8 (Web API)
- Banco de dados: SQL Server, com Entity Framework Core
- Front-end: React
- Infra: Docker e Docker Compose

## Como o projeto está organizado

O back-end segue Clean Architecture, separado em camadas:

```
src/
  GestaoColetas.Domain          entidades e regras de negócio (o núcleo)
  GestaoColetas.Application     casos de uso, DTOs e contratos (interfaces)
  GestaoColetas.Infrastructure  acesso a dados (EF Core / SQL Server)
  GestaoColetas.WebAPI          a API: controllers e configuração
```

A regra principal: as camadas de fora dependem das de dentro, e o Domain não depende
de ninguém — assim as regras ficam isoladas e fáceis de testar.

## Configuração do ambiente

O que precisa instalar pra rodar está em [docs/AMBIENTE.md](docs/AMBIENTE.md).

As instruções completas de execução (com Docker) eu vou colocando aqui conforme o
projeto avança.
