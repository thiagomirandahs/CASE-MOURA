// Camada que conversa com o back-end. Em um lugar só ficam todas as chamadas HTTP.
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5080";

// ----- Token JWT (guardado no navegador) -----
export const getToken = () => localStorage.getItem("token");
export const setToken = (t) => localStorage.setItem("token", t);
export const logout = () => localStorage.removeItem("token");

// Função base: injeta o token, faz o fetch e trata erros (lendo a mensagem {erro} do back-end).
async function req(path, options = {}) {
  const token = getToken();
  const resp = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (resp.status === 401) {
    logout();
    throw new Error("Sessão expirada ou inválida. Entre novamente.");
  }

  if (!resp.ok) {
    let mensagem = "Erro na requisição.";
    try {
      const corpo = await resp.json();
      mensagem = corpo.erro || mensagem;
    } catch {
      /* sem corpo JSON */
    }
    throw new Error(mensagem);
  }

  if (resp.status === 204) return null;
  return resp.json();
}

// ----- Autenticação -----
export async function login(usuario, senha) {
  const dados = await req("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ usuario, senha }),
  });
  setToken(dados.token);
  return dados;
}

// ----- Coletas (lista paginada) -----
export function listarColetas({ status, clienteId, inicio, fim, pagina = 1, tamanhoPagina = 10 } = {}) {
  const p = new URLSearchParams();
  if (status) p.append("status", status);
  if (clienteId) p.append("clienteId", clienteId);
  if (inicio) p.append("inicio", inicio);
  if (fim) p.append("fim", fim);
  p.append("pagina", pagina);
  p.append("tamanhoPagina", tamanhoPagina);
  return req(`/api/coletas?${p}`);
}

export const criarColeta = (dados) =>
  req("/api/coletas", { method: "POST", body: JSON.stringify(dados) });

export const atribuirMotoristaVeiculo = (id, dados) =>
  req(`/api/coletas/${id}/atribuir`, { method: "POST", body: JSON.stringify(dados) });

export const marcarComoColetada = (id) =>
  req(`/api/coletas/${id}/coletar`, { method: "POST" });

export const cancelarColeta = (id) =>
  req(`/api/coletas/${id}/cancelar`, { method: "POST" });

export const registrarOcorrencia = (id, descricao) =>
  req(`/api/coletas/${id}/ocorrencias`, { method: "POST", body: JSON.stringify({ descricao }) });

// ----- Cadastros (para os dropdowns) -----
export const listarClientes = () => req("/api/clientes");
export const listarMotoristas = () => req("/api/motoristas");
export const listarVeiculos = () => req("/api/veiculos");

// ----- Dashboard -----
export const getDashboard = () => req("/api/dashboard");
