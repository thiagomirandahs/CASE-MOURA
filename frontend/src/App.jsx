import { useEffect, useState } from "react";
import Login from "./Login";
import {
  getToken,
  logout as apiLogout,
  listarColetas,
  criarColeta,
  atribuirMotoristaVeiculo,
  marcarComoColetada,
  cancelarColeta,
  registrarOcorrencia,
  listarClientes,
  listarMotoristas,
  listarVeiculos,
  getDashboard,
} from "./api";

function Modal({ titulo, onFechar, children }) {
  return (
    <div className="modal-overlay" onClick={onFechar}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-head">
          <h3>{titulo}</h3>
          <button type="button" className="x" onClick={onFechar}>×</button>
        </div>
        {children}
      </div>
    </div>
  );
}

const FORM_COLETA = {
  clienteId: "", remetenteNome: "", remetenteEndereco: "",
  destinatarioNome: "", destinatarioEndereco: "",
  dataColetaPrevista: "", prioridade: "Normal", observacoes: "",
};

const CARDS = [
  { chave: "total", rotulo: "Total", cor: "#475569" },
  { chave: "abertas", rotulo: "Abertas", cor: "#1e40af" },
  { chave: "emColeta", rotulo: "Em Coleta", cor: "#92400e" },
  { chave: "coletadas", rotulo: "Coletadas", cor: "#166534" },
  { chave: "emAtraso", rotulo: "Em Atraso", cor: "#b91c1c" },
  { chave: "altaPrioridadeAtivas", rotulo: "Alta Prioridade", cor: "#be185d" },
];

export default function App() {
  const [logado, setLogado] = useState(!!getToken());
  if (!logado) return <Login onEntrar={() => setLogado(true)} />;
  return <Painel onSair={() => { apiLogout(); setLogado(false); }} />;
}

function Painel({ onSair }) {
  const [resultado, setResultado] = useState({ itens: [], total: 0, pagina: 1, totalPaginas: 0 });
  const [dashboard, setDashboard] = useState(null);
  const [filtros, setFiltros] = useState({ status: "", clienteId: "", inicio: "", fim: "" });
  const [pagina, setPagina] = useState(1);
  const [busca, setBusca] = useState("");
  const [erro, setErro] = useState(null);
  const [carregando, setCarregando] = useState(false);

  const [clientes, setClientes] = useState([]);
  const [motoristas, setMotoristas] = useState([]);
  const [veiculos, setVeiculos] = useState([]);

  const [modalNova, setModalNova] = useState(false);
  const [formColeta, setFormColeta] = useState(FORM_COLETA);
  const [atribuirId, setAtribuirId] = useState(null);
  const [atribuirForm, setAtribuirForm] = useState({ motoristaId: "", veiculoId: "" });
  const [ocorrenciaColetaId, setOcorrenciaColetaId] = useState(null);
  const [novaOcorrencia, setNovaOcorrencia] = useState("");

  async function carregar() {
    setCarregando(true);
    setErro(null);
    try {
      setResultado(await listarColetas({ ...filtros, pagina, tamanhoPagina: 10 }));
      getDashboard().then(setDashboard).catch(() => {});
    } catch (e) {
      setErro(e.message);
    } finally {
      setCarregando(false);
    }
  }

  useEffect(() => {
    carregar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pagina]);

  useEffect(() => {
    listarClientes().then(setClientes).catch(() => {});
    listarMotoristas().then(setMotoristas).catch(() => {});
    listarVeiculos().then(setVeiculos).catch(() => {});
  }, []);

  function aplicarFiltros(e) {
    e.preventDefault();
    if (pagina !== 1) setPagina(1);
    else carregar();
  }

  async function acao(fn) {
    setErro(null);
    try {
      await fn();
      await carregar();
    } catch (e) {
      setErro(e.message);
    }
  }

  function criar(e) {
    e.preventDefault();
    acao(async () => {
      await criarColeta({ ...formColeta, clienteId: Number(formColeta.clienteId) });
      setFormColeta(FORM_COLETA);
      setModalNova(false);
    });
  }

  function confirmarAtribuir(e) {
    e.preventDefault();
    const id = atribuirId;
    acao(async () => {
      await atribuirMotoristaVeiculo(id, {
        motoristaId: Number(atribuirForm.motoristaId),
        veiculoId: Number(atribuirForm.veiculoId),
      });
      setAtribuirId(null);
      setAtribuirForm({ motoristaId: "", veiculoId: "" });
    });
  }

  function adicionarOcorrencia(e) {
    e.preventDefault();
    const id = ocorrenciaColetaId;
    const desc = novaOcorrencia.trim();
    if (!desc) return;
    acao(async () => {
      await registrarOcorrencia(id, desc);
      setNovaOcorrencia("");
    });
  }

  function cancelar(id) {
    if (window.confirm("Cancelar esta coleta? Essa ação não pode ser desfeita.")) {
      acao(() => cancelarColeta(id));
    }
  }

  const termo = busca.trim().toLowerCase();
  const itens = termo
    ? resultado.itens.filter((c) =>
        [c.numero, c.clienteNome, c.remetenteNome, c.destinatarioNome]
          .some((v) => (v || "").toLowerCase().includes(termo)))
    : resultado.itens;

  const coletaOco = resultado.itens.find((c) => c.id === ocorrenciaColetaId);

  return (
    <div className="container">
      <header>
        <div>
          <h1>Gestão de Coletas</h1>
          <p className="sub">Acompanhamento das solicitações da transportadora</p>
        </div>
        <div className="header-acoes">
          <button onClick={() => setModalNova(true)}>+ Nova coleta</button>
          <button className="secundario" onClick={onSair}>Sair</button>
        </div>
      </header>

      {dashboard && (
        <div className="cards">
          {CARDS.map((card) => (
            <div className="card" key={card.chave} style={{ borderTopColor: card.cor }}>
              <span className="card-num" style={{ color: card.cor }}>{dashboard[card.chave]}</span>
              <span className="card-rot">{card.rotulo}</span>
            </div>
          ))}
        </div>
      )}

      <form className="filtros" onSubmit={aplicarFiltros}>
        <select value={filtros.status} onChange={(e) => setFiltros({ ...filtros, status: e.target.value })}>
          <option value="">Todos os status</option>
          <option value="Aberta">Aberta</option>
          <option value="EmColeta">Em Coleta</option>
          <option value="Coletado">Coletado</option>
          <option value="Cancelada">Cancelada</option>
        </select>
        <select value={filtros.clienteId} onChange={(e) => setFiltros({ ...filtros, clienteId: e.target.value })}>
          <option value="">Todos os clientes</option>
          {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
        </select>
        <input type="date" value={filtros.inicio} onChange={(e) => setFiltros({ ...filtros, inicio: e.target.value })} />
        <input type="date" value={filtros.fim} onChange={(e) => setFiltros({ ...filtros, fim: e.target.value })} />
        <button type="submit">Filtrar</button>
        <input className="busca" placeholder="Buscar nesta página..." value={busca} onChange={(e) => setBusca(e.target.value)} />
      </form>

      {erro && <p className="erro">⚠ {erro}</p>}

      {carregando ? (
        <p>Carregando...</p>
      ) : (
        <>
          <table>
            <thead>
              <tr>
                <th>Número</th>
                <th>Cliente</th>
                <th>Remetente → Destinatário</th>
                <th>Prioridade</th>
                <th>Status</th>
                <th>Prevista</th>
                <th>Motorista / Veículo</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {itens.map((c) => (
                <tr key={c.id} className={c.prioridadeAlta ? "alta" : ""}>
                  <td>{c.numero}</td>
                  <td>{c.clienteNome}</td>
                  <td>{c.remetenteNome} → {c.destinatarioNome}</td>
                  <td>{c.prioridade}</td>
                  <td>
                    <span className={`status status-${c.status}`}>{c.status}</span>
                    {c.emAtraso && <span className="badge-atraso">atrasada</span>}
                  </td>
                  <td className={c.emAtraso ? "data-atraso" : ""}>{new Date(c.dataColetaPrevista).toLocaleDateString("pt-BR")}</td>
                  <td>{(c.motoristaNome || "—") + " / " + (c.veiculoPlaca || "—")}</td>
                  <td className="acoes">
                    <button className="b-azul" onClick={() => setAtribuirId(c.id)}>Atribuir</button>
                    <button className="b-verde" onClick={() => acao(() => marcarComoColetada(c.id))}>Coletar</button>
                    <button className="b-vermelho" onClick={() => cancelar(c.id)}>Cancelar</button>
                    <button className="b-cinza" onClick={() => setOcorrenciaColetaId(c.id)}>Ocorrências ({c.ocorrencias.length})</button>
                  </td>
                </tr>
              ))}
              {itens.length === 0 && (
                <tr><td colSpan="8" className="vazio">Nenhuma coleta encontrada.</td></tr>
              )}
            </tbody>
          </table>

          <div className="paginacao">
            <button disabled={pagina <= 1} onClick={() => setPagina((p) => p - 1)}>‹ Anterior</button>
            <span>Página {resultado.pagina} de {Math.max(resultado.totalPaginas, 1)} · {resultado.total} coletas</span>
            <button disabled={pagina >= resultado.totalPaginas} onClick={() => setPagina((p) => p + 1)}>Próxima ›</button>
          </div>
        </>
      )}

      {modalNova && (
        <Modal titulo="Nova coleta" onFechar={() => setModalNova(false)}>
          <form className="form-grid" onSubmit={criar}>
            <select required value={formColeta.clienteId} onChange={(e) => setFormColeta({ ...formColeta, clienteId: e.target.value })}>
              <option value="">Cliente...</option>
              {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
            </select>
            <input required placeholder="Remetente" value={formColeta.remetenteNome} onChange={(e) => setFormColeta({ ...formColeta, remetenteNome: e.target.value })} />
            <input placeholder="Endereço remetente" value={formColeta.remetenteEndereco} onChange={(e) => setFormColeta({ ...formColeta, remetenteEndereco: e.target.value })} />
            <input required placeholder="Destinatário" value={formColeta.destinatarioNome} onChange={(e) => setFormColeta({ ...formColeta, destinatarioNome: e.target.value })} />
            <input placeholder="Endereço destinatário" value={formColeta.destinatarioEndereco} onChange={(e) => setFormColeta({ ...formColeta, destinatarioEndereco: e.target.value })} />
            <input required type="date" value={formColeta.dataColetaPrevista} onChange={(e) => setFormColeta({ ...formColeta, dataColetaPrevista: e.target.value })} />
            <select value={formColeta.prioridade} onChange={(e) => setFormColeta({ ...formColeta, prioridade: e.target.value })}>
              <option>Baixa</option>
              <option>Normal</option>
              <option>Alta</option>
            </select>
            <input placeholder="Observações" value={formColeta.observacoes} onChange={(e) => setFormColeta({ ...formColeta, observacoes: e.target.value })} />
            <button type="submit" className="full">Criar coleta</button>
          </form>
        </Modal>
      )}

      {atribuirId && (
        <Modal titulo="Atribuir motorista e veículo" onFechar={() => setAtribuirId(null)}>
          <form className="form-grid" onSubmit={confirmarAtribuir}>
            <select required value={atribuirForm.motoristaId} onChange={(e) => setAtribuirForm({ ...atribuirForm, motoristaId: e.target.value })}>
              <option value="">Motorista...</option>
              {motoristas.map((m) => <option key={m.id} value={m.id}>{m.nome}</option>)}
            </select>
            <select required value={atribuirForm.veiculoId} onChange={(e) => setAtribuirForm({ ...atribuirForm, veiculoId: e.target.value })}>
              <option value="">Veículo...</option>
              {veiculos.map((v) => <option key={v.id} value={v.id}>{v.placa} — {v.modelo}</option>)}
            </select>
            <button type="submit" className="full">Atribuir</button>
          </form>
        </Modal>
      )}

      {ocorrenciaColetaId && coletaOco && (
        <Modal titulo={`Ocorrências · ${coletaOco.numero}`} onFechar={() => { setOcorrenciaColetaId(null); setNovaOcorrencia(""); }}>
          <div className="oco-lista">
            {coletaOco.ocorrencias.length === 0 && <p className="vazio">Nenhuma ocorrência registrada.</p>}
            {coletaOco.ocorrencias.map((o) => (
              <div className="oco-item" key={o.id}>
                <p>{o.descricao}</p>
                <small>{new Date(o.dataHora).toLocaleString("pt-BR")} · por {o.usuarioResponsavel}</small>
              </div>
            ))}
          </div>
          <form className="form-grid" onSubmit={adicionarOcorrencia}>
            <input required placeholder="Nova ocorrência (ex.: endereço errado)" value={novaOcorrencia} onChange={(e) => setNovaOcorrencia(e.target.value)} />
            <button type="submit" className="full">Registrar ocorrência</button>
          </form>
        </Modal>
      )}
    </div>
  );
}
