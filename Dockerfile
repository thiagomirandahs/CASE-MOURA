# ---- Build: compila e publica a API usando o SDK do .NET 8 ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia primeiro só os .csproj pra aproveitar o cache no restore
COPY global.json ./
COPY GestaoColetas.sln ./
COPY src/GestaoColetas.Domain/GestaoColetas.Domain.csproj src/GestaoColetas.Domain/
COPY src/GestaoColetas.Application/GestaoColetas.Application.csproj src/GestaoColetas.Application/
COPY src/GestaoColetas.Infrastructure/GestaoColetas.Infrastructure.csproj src/GestaoColetas.Infrastructure/
COPY src/GestaoColetas.WebAPI/GestaoColetas.WebAPI.csproj src/GestaoColetas.WebAPI/
RUN dotnet restore GestaoColetas.sln

# Copia o resto do código e publica
COPY . .
RUN dotnet publish src/GestaoColetas.WebAPI/GestaoColetas.WebAPI.csproj -c Release -o /app --no-restore

# ---- Runtime: imagem menor, só com o necessário pra rodar ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestaoColetas.WebAPI.dll"]
