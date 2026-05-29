# Configuração do ambiente

Aqui anotei como deixei minha máquina pronta pra rodar o projeto, na ordem que fiz.
Se você quiser rodar do zero (estou no Windows 10), é só seguir os mesmos passos.

## O que o projeto usa

| Ferramenta | Pra que serve |
|---|---|
| Git | Versionar o código e mandar pro GitHub |
| .NET 8 SDK | Rodar o back-end (C#) |
| Node.js + npm | Rodar o front-end (React) |
| WSL2 | Um Linux interno do Windows que o Docker precisa pra funcionar |
| Docker Desktop | Subir a API e o banco de dados em contêineres |

## O que eu já tinha

Quando comecei, o Git, o Node.js e o npm já estavam instalados. Eu também tinha o
.NET 9, mas o case pede o **.NET 8**, então instalei o 8 também. Os dois ficam
instalados juntos sem conflito, então não precisei desinstalar nada.

## Passo a passo

Fiz tudo pelo **PowerShell**, usando o `winget` (o instalador de programas que já
vem no Windows). Quando aparece a janela de permissão do Windows, é só clicar em Sim.

### 1. .NET 8

```powershell
winget install --id Microsoft.DotNet.SDK.8 -e --accept-package-agreements --accept-source-agreements
```

Conferi se instalou (no meu caso veio a versão `8.0.421`):

```powershell
dotnet --list-sdks
```

### 2. WSL2

Aqui eu tomei um perrengue. Tentei instalar o Docker direto e ele baixou tudo, mas
falhou bem na hora de instalar. Fui atrás e descobri que no Windows o Docker roda em
cima do **WSL2** (esse Linux interno), e eu não tinha ele.

Então instalei o WSL2 primeiro:

```powershell
wsl --install --no-distribution
```

E **reiniciei o PC** — sem reiniciar não adianta, as mudanças só valem depois. Pra
conferir que ativou (deu versão 2):

```powershell
wsl --status
```

### 3. Docker Desktop

Depois de reiniciar, instalei o Docker de novo e aí foi de boa:

```powershell
winget install --id Docker.DockerDesktop -e --accept-package-agreements --accept-source-agreements
```

Abri o Docker Desktop uma vez, aceitei os termos e esperei a baleia ficar estável
(uns 2 minutos na primeira vez). Pra ter certeza que tava tudo certo, rodei:

```powershell
docker run hello-world
```

Apareceu o "Hello from Docker!", então o Docker tava funcionando de ponta a ponta.

## Conferir se está tudo ok

```powershell
git --version
dotnet --version
node --version
docker --version
```

Se todos responderem com um número de versão, o ambiente tá pronto.

## Uns perrengues que tive (caso aconteça com você)

- **O Docker não instalava:** era falta do WSL2. Instalei o WSL2 e reiniciei, aí foi.
- **O terminal não achava o `docker`:** era só o PATH antigo. Fechei e abri um
  terminal novo e funcionou.
- **`docker` só funciona com o Docker Desktop aberto:** o motor precisa estar ligado.
  Deixei o Docker abrir junto com o Windows pra não esquecer.
