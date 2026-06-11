import { useState } from "react";
import { BrowserRouter, Routes, Route, NavLink, Navigate } from "react-router-dom";
import { getToken, logout as apiLogout } from "./api";
import Login from "./Login";
import Dashboard from "./Dashboard";
import Coletas from "./Coletas";
import Cadastros from "./Cadastros";

export default function App() {
  const [logado, setLogado] = useState(!!getToken());

  if (!logado) return <Login onEntrar={() => setLogado(true)} />;

  function sair() {
    apiLogout();
    setLogado(false);
  }

  return (
    <BrowserRouter>
      <nav className="nav">
        <span className="nav-brand">Gestão de Coletas</span>
        <div className="nav-links">
          <NavLink to="/dashboard">Dashboard</NavLink>
          <NavLink to="/coletas">Coletas</NavLink>
          <NavLink to="/cadastros">Cadastros</NavLink>
        </div>
        <button className="secundario" onClick={sair}>Sair</button>
      </nav>
      <main>
        <Routes>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/coletas" element={<Coletas />} />
          <Route path="/cadastros" element={<Cadastros />} />
          <Route path="*" element={<Navigate to="/coletas" replace />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
