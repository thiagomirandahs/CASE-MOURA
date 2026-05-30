import { useEffect, useState } from "react";
import { getDashboard } from "./api";

// status que entram na "distribuição" (a rosca)
const STATUS = [
  { chave: "abertas", rotulo: "Aberta", cor: "#2563eb" },
  { chave: "emColeta", rotulo: "Em Coleta", cor: "#f59e0b" },
  { chave: "coletadas", rotulo: "Coletado", cor: "#16a34a" },
  { chave: "canceladas", rotulo: "Cancelada", cor: "#dc2626" },
];

export default function Dashboard() {
  const [d, setD] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    getDashboard().then(setD).catch((e) => setErro(e.message));
  }, []);

  const Cabecalho = <div className="page-head"><h2>Dashboard</h2></div>;
  if (erro) return <div>{Cabecalho}<p className="erro">⚠ {erro}</p></div>;
  if (!d) return <div>{Cabecalho}<p>Carregando...</p></div>;

  const total = d.total || 0;
  const pct = (n) => (total ? Math.round((n / total) * 100) : 0);
  const taxaConclusao = pct(d.coletadas);

  // monta os pedaços da rosca (conic-gradient) proporcionais a cada status
  let acumulado = 0;
  const fatias = STATUS.filter((s) => d[s.chave] > 0).map((s) => {
    const inicio = (acumulado / total) * 100;
    acumulado += d[s.chave];
    const fim = (acumulado / total) * 100;
    return `${s.cor} ${inicio}% ${fim}%`;
  });
  const rosca = fatias.length ? `conic-gradient(${fatias.join(", ")})` : "#e2e8f0";

  return (
    <div>
      {Cabecalho}

      <div className="kpis">
        <div className="kpi"><strong>{d.total}</strong><span>Total</span></div>
        <div className="kpi"><strong style={{ color: "#2563eb" }}>{d.abertas}</strong><span>Abertas</span></div>
        <div className="kpi"><strong style={{ color: "#b45309" }}>{d.emColeta}</strong><span>Em Coleta</span></div>
        <div className="kpi"><strong style={{ color: "#16a34a" }}>{d.coletadas}</strong><span>Coletadas</span></div>
        <div className="kpi"><strong style={{ color: "#dc2626" }}>{d.canceladas}</strong><span>Canceladas</span></div>
      </div>

      <div className="dash-grid">
        <div className="painel">
          <h3>Distribuição por status</h3>
          <div className="donut-wrap">
            <div className="donut" style={{ background: rosca }}>
              <div className="donut-hole">
                <strong>{total}</strong>
                <span>coletas</span>
              </div>
            </div>
            <ul className="legenda">
              {STATUS.map((s) => (
                <li key={s.chave}>
                  <span className="dot" style={{ background: s.cor }} />
                  <span className="leg-rot">{s.rotulo}</span>
                  <strong>{d[s.chave]}</strong>
                  <em>{pct(d[s.chave])}%</em>
                </li>
              ))}
            </ul>
          </div>
        </div>

        <div className="painel">
          <h3>Indicadores</h3>
          <div className="indicador">
            <div className="indicador-top">
              <span>Taxa de conclusão</span>
              <strong>{taxaConclusao}%</strong>
            </div>
            <div className="barra"><div className="barra-fill" style={{ width: `${taxaConclusao}%` }} /></div>
            <small>{d.coletadas} de {total} coletas concluídas</small>
          </div>

          <div className="alertas">
            <div className={`alerta ${d.emAtraso > 0 ? "perigo" : "ok"}`}>
              <strong>{d.emAtraso}</strong>
              <span>Em atraso</span>
            </div>
            <div className={`alerta ${d.altaPrioridadeAtivas > 0 ? "aviso" : "ok"}`}>
              <strong>{d.altaPrioridadeAtivas}</strong>
              <span>Alta prioridade ativas</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
