# Roteiro do vídeo (até 3 minutos)

Um guia do que mostrar e falar na demonstração, na ordem. A meta é caber em 3 min,
então é melhor ir direto ao ponto.

## Antes de gravar

- Deixar a API e o front já rodando (pra não gastar tempo subindo tudo no vídeo)
- Abrir as abas: o sistema (http://localhost:5173) e o Swagger
- Deixar o VS Code aberto no projeto pra mostrar o código rapidinho

## Roteiro

**(0:00 – 0:20) Abertura**
- "Esse é o sistema de Gestão de Coletas que fiz pro case. Serve pra uma transportadora
  controlar as coletas, tirando isso de planilha e WhatsApp."
- Stack em uma frase: .NET 8 com Clean Architecture, SQL Server, React e Docker.

**(0:20 – 0:40) Login e visão geral**
- Mostrar a tela de login e entrar com `admin` / `admin123`.
- Comentar que os endpoints são protegidos por JWT.

**(0:40 – 1:30) O fluxo principal (as regras de negócio)**
- Criar uma coleta nova (mostrar o formulário com os campos).
- Na lista, apontar a coleta de **prioridade Alta destacada e no topo**.
- Atribuir motorista e veículo → o status vira **Em Coleta**.
- Marcar como **Coletado**.
- Fazer uma ação inválida de propósito pra mostrar a regra (ex.: tentar coletar sem
  motorista, ou cancelar uma já coletada) — aparece a mensagem de erro.
- Abrir as **ocorrências** de uma coleta: mostrar que guarda data/hora e quem registrou.

**(1:30 – 2:10) Dashboard, filtros e exportação**
- Ir no **Dashboard**: o gráfico de distribuição por status, a taxa de conclusão e os
  alertas de "em atraso" e "alta prioridade".
- Voltar em Coletas e usar os **filtros** (status, cliente, período) — eles aplicam sozinhos.
- Clicar em **Exportar** e abrir o CSV no Excel.

**(2:10 – 2:45) Por dentro do código**
- Mostrar a estrutura em camadas (as pastas em `src/`).
- Abrir a `SolicitacaoColeta` e mostrar que as **regras estão na entidade** (o `Status`
  com set privado e os métodos que validam).
- Rodar `dotnet test` e mostrar os **12 testes passando**.

**(2:45 – 3:00) Fechamento**
- "Resumindo: as regras de negócio ficam isoladas e testadas, e o sistema cobre o fluxo
  inteiro da coleta, com dashboard e exportação. Obrigado!"

## Dicas

- Falar pausado, mas sem enrolar — 3 min passa rápido.
- Se travar, corta e regrava o trecho; não precisa ser de primeira.
- Manter o foco nas **regras de negócio** e na **arquitetura** — é o que mais conta no case.
