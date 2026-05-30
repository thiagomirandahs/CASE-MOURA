import { defineConfig } from "@playwright/test";

// Configuração do Playwright (testes end-to-end).
// O Playwright sobe a API (.NET) e o front (Vite) sozinho antes de rodar os testes.
// Pré-requisito: o banco SQL Server precisa estar no ar (docker start sql-gestaocoletas).
export default defineConfig({
  testDir: "./e2e",
  timeout: 30_000,
  fullyParallel: false,
  workers: 1,
  reporter: "list",
  use: {
    baseURL: "http://localhost:5173",
    headless: true,
    trace: "on-first-retry",
  },
  webServer: [
    {
      // a API, na porta que o front procura
      command: "dotnet run --project ../src/GestaoColetas.WebAPI --urls http://localhost:5080",
      url: "http://localhost:5080/swagger/index.html",
      reuseExistingServer: true,
      timeout: 120_000,
    },
    {
      // o front (React/Vite)
      command: "npm run dev",
      url: "http://localhost:5173",
      reuseExistingServer: true,
      timeout: 60_000,
    },
  ],
});
