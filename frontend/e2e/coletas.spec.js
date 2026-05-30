import { test, expect } from "@playwright/test";

// Faz login no sistema (usado por todos os testes).
async function logar(page) {
  await page.goto("/");
  await page.getByPlaceholder("Usuário").fill("admin");
  await page.getByPlaceholder("Senha").fill("admin123");
  await page.getByRole("button", { name: "Entrar" }).click();
  // depois de entrar, o menu do app aparece
  await expect(page.getByText("Gestão de Coletas")).toBeVisible();
}

test("faz login e vê o dashboard com os indicadores", async ({ page }) => {
  await logar(page);
  await page.getByRole("link", { name: "Dashboard" }).click();

  await expect(page.getByText("Distribuição por status")).toBeVisible();
  await expect(page.getByText("Taxa de conclusão")).toBeVisible();
});

test("lista as coletas da seed, com prioridade Alta presente", async ({ page }) => {
  await logar(page);
  await page.getByRole("link", { name: "Coletas" }).click();

  await expect(page.getByText(/COL-2026-/).first()).toBeVisible();
  await expect(page.getByText("Alta").first()).toBeVisible();
});

test("cria uma nova coleta e ela aparece na lista", async ({ page }) => {
  await logar(page);
  await page.getByRole("link", { name: "Coletas" }).click();
  await page.getByRole("button", { name: /nova coleta/i }).click();

  // o modal abre
  await expect(page.getByRole("heading", { name: "Nova coleta" })).toBeVisible();

  // cliente (primeiro select do modal), remetente, destinatário e data
  await page.locator(".modal select").first().selectOption({ index: 1 });
  await page.getByPlaceholder("Ex.: Mercado Central").fill("Remetente E2E");
  await page.getByPlaceholder("Ex.: Filial Sul").fill("Destinatario E2E");
  await page.locator('.modal input[type="date"]').fill("2026-12-31");

  await page.getByRole("button", { name: "Criar coleta" }).click();

  // a lista recarrega e mostra a coleta criada
  await expect(page.getByText("Remetente E2E")).toBeVisible();
});
