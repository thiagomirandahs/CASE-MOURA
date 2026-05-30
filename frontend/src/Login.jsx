import { useState } from "react";
import { login } from "./api";

export default function Login({ onEntrar }) {
  const [usuario, setUsuario] = useState("admin");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState(null);
  const [carregando, setCarregando] = useState(false);

  async function entrar(e) {
    e.preventDefault();
    setErro(null);
    setCarregando(true);
    try {
      await login(usuario, senha);
      onEntrar();
    } catch (e) {
      setErro(e.message);
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="login-wrap">
      <form className="login-card" onSubmit={entrar}>
        <h1>Gestão de Coletas</h1>
        <p className="sub">Entre para acessar o painel</p>

        <input placeholder="Usuário" value={usuario} onChange={(e) => setUsuario(e.target.value)} />
        <input type="password" placeholder="Senha" value={senha} onChange={(e) => setSenha(e.target.value)} />

        {erro && <p className="erro">{erro}</p>}

        <button disabled={carregando}>{carregando ? "Entrando..." : "Entrar"}</button>
        <p className="dica">Demo: <strong>admin</strong> / <strong>admin123</strong></p>
      </form>
    </div>
  );
}
