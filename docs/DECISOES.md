# Decisões de arquitetura

Este documento registra as principais decisões de arquitetura do projeto e a justificativa de cada uma.

## Clean Architecture (separação em camadas)

O back-end é dividido em quatro projetos: **Domain**, **Application**, **Infrastructure** e **WebAPI**.

As dependências apontam sempre para dentro: a WebAPI conhece a Application, a Application conhece o Domain, e o Domain não conhece nenhuma outra camada. O acesso a dados (Infrastructure) permanece na borda.

**Justificativa:** mantém as regras de negócio isoladas de detalhes técnicos. A substituição do SQL Server por outro banco, ou da API por outra interface, não afeta o núcleo (Domain). Além disso, as regras podem ser testadas sem dependência de banco.

## Domínio rico (regras dentro da entidade)

As regras de negócio ficam na entidade `SolicitacaoColeta`, em métodos como `AtribuirMotoristaEVeiculo`, `MarcarComoColetada`, `Cancelar` e `RegistrarOcorrencia`. O `Status` possui `set` privado: não pode ser alterado externamente, apenas por esses métodos, que validam as regras antes de aplicar a mudança.

**Justificativa:** impede estados inválidos (por exemplo, "Coletado" sem motorista). A regra fica concentrada em um único ponto, junto ao dado que protege, em vez de dispersa por vários serviços. É o aspecto mais valorizado pelo case.

## EF Core com migrations e seed

O Entity Framework Core é utilizado para o acesso ao SQL Server. O banco é criado por **migration** (não por script avulso) e, na inicialização, a aplicação aplica as migrations e popula dados de exemplo (**seed**) caso o banco esteja vazio.

**Justificativa:** a migration mantém a estrutura do banco versionada junto ao código. O seed disponibiliza dados para teste já na primeira execução, sem cadastro manual.

## Repository + Service

Entre a entidade e a API há duas camadas: o **repositório** (acesso ao banco) e o **service** (orquestração do caso de uso). A API chama o service, que chama o repositório, que utiliza o EF Core.

**Justificativa:** o service não precisa conhecer *como* o dado é obtido, apenas *que* ele existe (por meio da interface do repositório). Cada parte assume uma única responsabilidade, o que facilita a manutenção e os testes.

## DTOs (sem expor a entidade diretamente)

A API recebe e devolve **DTOs** (objetos de entrada e saída), e não as entidades de domínio.

**Justificativa:** permite controlar o que entra e o que sai. O DTO possibilita montar a resposta conforme a necessidade do front-end — por exemplo, informando se a coleta está "em atraso", que é um cálculo e não um campo do banco.

## Tratamento de erro centralizado

Um middleware captura as exceções e as traduz para o status HTTP adequado: `KeyNotFoundException` resulta em 404, erros de regra de negócio em 400, e os demais em 500.

**Justificativa:** os controllers permanecem enxutos (sem `try/catch` repetido em cada método) e a resposta de erro fica padronizada para quem consome a API.

## Autenticação via JWT

Os endpoints são protegidos com `[Authorize]`, e o acesso ocorre por **token JWT**: o endpoint `/api/auth/login` valida o usuário e devolve o token, que o front-end armazena e envia nas chamadas seguintes.

**Justificativa:** é a abordagem padrão para proteger uma API REST sem manter sessão no servidor. Adicionalmente, o responsável por cada ocorrência é obtido do próprio token.

## Serilog, Swagger e paginação

- **Serilog:** logs estruturados em console e em arquivo, para acompanhar o comportamento da API.
- **Swagger:** documenta a API e permite testá-la pelo navegador, inclusive o login com token.
- **Paginação:** a lista de coletas é paginada no banco (`Skip`/`Take`), evitando carregar todos os registros de uma vez.

**Justificativa:** são itens citados como diferencial pelo case e necessários em qualquer API real — observabilidade, documentação e controle do volume de dados retornado.

## Front-end em React, separado em páginas

O front-end utiliza React (com Vite). A navegação é dividida em duas páginas com o React Router (**Dashboard** e **Coletas**), e toda a comunicação com a API fica concentrada em um único arquivo (`api.js`).

**Justificativa:** centralizar as chamadas HTTP evita a repetição de `fetch` e token em cada componente. A separação em páginas com URL própria torna a navegação mais natural.

## Exportação em CSV com BOM

A exportação gera um arquivo CSV com um **BOM** (marca de UTF-8) no início.

**Justificativa:** sem o BOM, o Excel interpreta os acentos incorretamente ("Farmácia" é exibido como "FarmÃ¡cia"). O BOM sinaliza a codificação UTF-8 e corrige a exibição. Optou-se por CSV em vez de um `.xlsx` para atender ao requisito (abertura no Excel) sem adicionar uma biblioteca ao projeto.

## Testes no domínio

Os testes (xUnit) atuam diretamente sobre a entidade `SolicitacaoColeta`, sem subir banco ou API.

**Justificativa:** as regras de transição de status são a parte mais crítica, onde os testes têm maior retorno. Por estarem isoladas no Domain, o conjunto executa rapidamente e os testes permanecem simples de ler.

## Desafios técnicos

Principais desafios enfrentados e respectivas soluções:

- **Instalação do Docker no Windows:** o Docker Desktop depende do WSL2, ausente na máquina. Após instalar o WSL2 e reiniciar, a instalação foi concluída. (Detalhes em [AMBIENTE.md](AMBIENTE.md).)
- **Ordem de inicialização no Docker:** ao subir os serviços pelo compose, a API ficava pronta antes de o SQL Server aceitar conexões, falhando ao aplicar as migrations. Solução: rotina de *retry* na inicialização, que repete a tentativa até o banco responder.
- **Acentuação no Excel:** o CSV exportado exibia "Farmácia" como "FarmÃ¡cia", por questão de codificação. A inclusão de um BOM UTF-8 no início do arquivo corrigiu a leitura.
- **Localização das regras de negócio:** a abordagem inicial concentraria a lógica no service, mas as regras foram mantidas na própria entidade `SolicitacaoColeta` (com o status protegido). Exigiu mais organização, porém tornou inviável burlar uma regra externamente.
- **Fim de linha (CRLF/LF):** no Windows, o Git emite avisos frequentes sobre o fim de linha. São inofensivos, mas demandaram análise até a identificação da causa.
