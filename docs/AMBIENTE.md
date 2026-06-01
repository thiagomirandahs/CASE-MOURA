# Configuração do ambiente

Guia para preparar uma máquina Windows, do zero, para executar o projeto. Os passos foram validados no Windows 10.

## Ferramentas necessárias

| Ferramenta | Finalidade |
|---|---|
| Git | Controle de versão e integração com o GitHub |
| .NET 8 SDK | Compilação e execução do back-end (C#) |
| Node.js + npm | Execução do front-end (React) |
| WSL2 | Subsistema Linux exigido pelo Docker no Windows |
| Docker Desktop | Execução da API e do banco de dados em contêineres |

## Pré-requisitos

O projeto exige especificamente o **.NET 8 SDK**. Caso a máquina já possua o .NET 9, não é necessário removê-lo: as versões coexistem sem conflito. Git, Node.js e npm também são necessários e podem já estar presentes no ambiente.

## Instalação

Os comandos a seguir utilizam o **winget** (gerenciador de pacotes nativo do Windows) e devem ser executados no **PowerShell**. Confirme as solicitações de permissão exibidas pelo sistema.

### 1. .NET 8 SDK

```powershell
winget install --id Microsoft.DotNet.SDK.8 -e --accept-package-agreements --accept-source-agreements
```

Verifique a instalação (exemplo de saída: `8.0.421`):

```powershell
dotnet --list-sdks
```

### 2. WSL2

No Windows, o Docker Desktop depende do WSL2; portanto, instale-o **antes** do Docker:

```powershell
wsl --install --no-distribution
```

**Reinicie o computador** após a instalação — as alterações do WSL2 só passam a valer após a reinicialização. Em seguida, confirme que a versão 2 está ativa:

```powershell
wsl --status
```

### 3. Docker Desktop

Com o WSL2 ativo, instale o Docker Desktop:

```powershell
winget install --id Docker.DockerDesktop -e --accept-package-agreements --accept-source-agreements
```

Abra o Docker Desktop uma vez, aceite os termos e aguarde a inicialização do serviço (aproximadamente 2 minutos na primeira execução). Valide o funcionamento com:

```powershell
docker run hello-world
```

A mensagem "Hello from Docker!" confirma que o Docker está operacional de ponta a ponta.

## Verificação final

```powershell
git --version
dotnet --version
node --version
docker --version
```

Se todos os comandos retornarem um número de versão, o ambiente está pronto.

## Problemas comuns

- **Falha na instalação do Docker:** normalmente decorre da ausência do WSL2. Instale o WSL2, reinicie a máquina e repita a instalação.
- **Comando `docker` não reconhecido:** geralmente indica PATH desatualizado na sessão atual. Feche e reabra o terminal.
- **`docker` exige o Docker Desktop em execução:** o serviço precisa estar ativo. Configure o Docker Desktop para iniciar com o Windows.
