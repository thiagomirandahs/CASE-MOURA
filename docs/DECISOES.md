# Decisões de arquitetura

Aqui registro as principais escolhas que fiz no projeto e o porquê de cada uma. Tentei
explicar com minhas palavras, do jeito que entendi cada coisa.

## Clean Architecture (separar em camadas)

Separei o back-end em quatro projetos: **Domain**, **Application**, **Infrastructure** e **WebAPI**.

A ideia é que as dependências apontem sempre pra "dentro": a WebAPI conhece a Application,
a Application conhece o Domain, e o Domain não conhece ninguém. Quem mexe com banco
(Infrastructure) fica na borda.

**Por quê:** isso mantém as regras de negócio isoladas de detalhe técnico. Se um dia
trocar o SQL Server por outro banco, ou a API por outra interface, o núcleo (Domain)
não muda. E, principalmente, dá pra testar as regras sem precisar de banco nenhum.

## Domínio rico (regras dentro da entidade)

As regras de negócio ficam dentro da entidade `SolicitacaoColeta`, em métodos como
`AtribuirMotoristaEVeiculo`, `MarcarComoColetada`, `Cancelar` e `RegistrarOcorrencia`.
O `Status` tem `set` privado: ninguém muda o status "na mão" por fora — só passando
por esses métodos, que checam as regras antes.

**Por quê:** assim fica impossível deixar a coleta num estado inválido (tipo "Coletado"
sem motorista). A regra mora num lugar só, perto do dado que ela protege, em vez de
espalhada em vários serviços. Foi a parte que o case mais valoriza, então caprichei aqui.

## EF Core com migrations e seed

Usei o Entity Framework Core pra falar com o SQL Server. O banco é criado por
**migration** (não por script solto), e no start a aplicação aplica as migrations e
popula uns dados de exemplo (**seed**) se o banco estiver vazio.

**Por quê:** migration deixa a estrutura do banco versionada junto com o código. E o
seed faz o projeto já subir com dados pra testar, sem ninguém precisar cadastrar tudo na mão.

## Repository + Service

Entre a entidade e a API tem duas camadas: o **repositório** (que conversa com o banco)
e o **service** (que orquestra o caso de uso). A API chama o service, o service chama o
repositório, o repositório usa o EF Core.

**Por quê:** o service não precisa saber *como* o dado é buscado, só *que* ele existe
(pela interface do repositório). Cada parte fica com uma responsabilidade só, e isso
facilita tanto a manutenção quanto os testes.

## DTOs (não expor a entidade direto)

A API recebe e devolve **DTOs** (objetos simples de entrada e saída), não as entidades
do domínio.

**Por quê:** assim eu controlo o que entra e o que sai. O DTO me deixa montar a resposta
do jeito que o front precisa — por exemplo, já mandando se a coleta está "em atraso",
que é um cálculo, não um campo do banco.

## Tratamento de erro centralizado

Fiz um middleware que captura as exceções e traduz pra um status HTTP certo:
`KeyNotFoundException` vira 404, erros de regra de negócio viram 400, e o resto vira 500.

**Por quê:** os controllers ficam limpos (sem `try/catch` repetido em todo método) e a
resposta de erro fica padronizada pra quem consome a API.

## JWT pra autenticação

Os endpoints são protegidos com `[Authorize]` e o acesso é por **token JWT**: o
`/api/auth/login` valida o usuário e devolve o token, que o front guarda e manda nas
próximas chamadas.

**Por quê:** é o jeito padrão de proteger uma API REST sem guardar sessão no servidor.
E me deu de brinde o "quem registrou" das ocorrências — pego o usuário do próprio token.

## Serilog, Swagger e paginação

- **Serilog**: logs estruturados no console e em arquivo, pra acompanhar o que a API faz.
- **Swagger**: documenta a API e deixa testar tudo pelo navegador, inclusive o login com token.
- **Paginação**: a lista de coletas é paginada no banco (`Skip`/`Take`), não trazendo tudo de uma vez.

**Por quê:** são coisas que o case cita como diferencial e que, na prática, toda API de
verdade acaba precisando — acompanhar o que acontece, documentar, e não estourar a
memória com listas grandes.

## Front em React, separado em páginas

O front é em React (com Vite). Separei em duas páginas com o React Router
(**Dashboard** e **Coletas**) e deixei toda a conversa com a API num arquivo só (`api.js`).

**Por quê:** centralizar as chamadas HTTP num lugar evita repetir `fetch` e token em
todo componente. E separar em páginas com URL própria deixa a navegação mais natural.

## Exportação em CSV com BOM

A exportação gera um arquivo CSV, e coloco um **BOM** (marca de UTF-8) no começo dele.

**Por quê:** sem isso, o Excel abre os acentos errados ("Farmácia" vira "FarmÃ¡cia").
O BOM avisa o Excel que o arquivo é UTF-8 e os acentos saem certos. Preferi CSV a um
`.xlsx` de verdade porque resolve a necessidade (abrir no Excel) sem trazer uma
biblioteca a mais pro projeto.

## Testes no domínio

Os testes (xUnit) batem direto na entidade `SolicitacaoColeta`, sem subir banco nem API.

**Por quê:** as regras de transição de status são a parte mais importante, então é onde
o teste rende mais. Como elas estão isoladas no Domain, o conjunto roda rápido (uns
30ms) e os testes ficam simples de ler.

## Desafios que apareceram no caminho

Alguns perrengues que tive e como resolvi:

- **Docker no Windows não instalava:** ele depende do WSL2, que eu não tinha. Instalei o
  WSL2, reiniciei o PC, e aí o Docker subiu numa boa. (Anotei os detalhes em
  [AMBIENTE.md](AMBIENTE.md).)
- **A API subia antes do banco (no Docker):** quando sobe tudo junto pelo compose, a API
  ficava pronta antes do SQL Server aceitar conexão e quebrava na hora de aplicar as
  migrations. Resolvi com um retry no startup — a API tenta de novo algumas vezes até o
  banco responder.
- **Acentos errados no Excel:** o CSV exportado abria "Farmácia" como "FarmÃ¡cia". Era
  encoding — coloquei um BOM de UTF-8 no começo do arquivo e o Excel passou a ler certo.
- **Onde colocar as regras de negócio:** no começo eu ia jogar tudo no service, mas
  preferi deixar as regras dentro da própria entidade `SolicitacaoColeta` (com o status
  protegido). Deu mais trabalho de organizar, mas ficou impossível burlar uma regra por fora.
- **Quebra de linha (CRLF/LF):** no Windows o git fica avisando sobre fim de linha o tempo
  todo. É inofensivo, mas perdi um tempo até entender de onde vinha.
