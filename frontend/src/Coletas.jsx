import { useEffect, useState } from "react";
import {
  listarColetas,
  criarColeta,
  atribuirMotoristaVeiculo,
  marcarComoColetada,
  cancelarColeta,
  registrarOcorrencia,
  exportarColetas,
  listarClientes,
  listarMotoristas,
  listarVeiculos,
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

// Popup de aviso/erro: aparece por cima e exige um "OK" pra fechar (o usuário não perde a mensagem).
function Aviso({ mensagem, onFechar }) {
  return (
    <div className="modal-overlay" onClick={onFechar}>
      <div className="modal modal-aviso" onClick={(e) => e.stopPropagation()}>
        <div className="aviso-icone">!</div>
        <h3>Atenção</h3>
        <p>{mensagem}</p>
        <button onClick={onFechar} autoFocus>OK</button>
      </div>
    </div>
  );
}

// Confirmação para ações destrutivas (ex.: cancelar) — no estilo do app, não o popup nativo do navegador.
function Confirmacao({ mensagem, textoConfirmar, onConfirmar, onCancelar }) {
  return (
    <div className="modal-overlay" onClick={onCancelar}>
      <div className="modal modal-aviso" onClick={(e) => e.stopPropagation()}>
        <div className="aviso-icone">!</div>
        <h3>Tem certeza?</h3>
        <p>{mensagem}</p>
        <div className="aviso-acoes">
          <button className="secundario" onClick={onCancelar} autoFocus>Voltar</button>
          <button className="perigo" onClick={onConfirmar}>{textoConfirmar}</button>
        </div>
      </div>
    </div>
  );
}

const FORM_COLETA = {
  clienteId: "", remetenteNome: "", remetenteEndereco: "",
  destinatarioNome: "", destinatarioEndereco: "",
  dataColetaPrevista: "", prioridade: "Normal", observacoes: "",
};

const FILTROS_VAZIOS = { status: "", clienteId: "", inicio: "", fim: "" };

// Mostra o status com espaço: o valor cru "EmColeta" vira "Em Coleta".
function rotuloStatus(s) {
  return s === "EmColeta" ? "Em Coleta" : s;
}

export default function Coletas() {
  const [resultado, setResultado] = useState({ itens: [], total: 0, pagina: 1, totalPaginas: 0 });
  const [filtros, setFiltros] = useState(FILTROS_VAZIOS);
  const [pagina, setPagina] = useState(1);
  const [busca, setBusca] = useState("");
  const [erro, setErro] = useState(null);
  const [carregando, setCarregando] = useState(false);
  const [exportando, setExportando] = useState(false);

  const [clientes, setClientes] = useState([]);
  const [motoristas, setMotoristas] = useState([]);
  const [veiculos, setVeiculos] = useState([]);

  const [modalNova, setModalNova] = useState(false);
  const [formColeta, setFormColeta] = useState(FORM_COLETA);
  const [atribuirId, setAtribuirId] = useState(null);
  const [atribuirForm, setAtribuirForm] = useState({ motoristaId: "", veiculoId: "" });
  const [ocorrenciaColetaId, setOcorrenciaColetaId] = useState(null);
  const [novaOcorrencia, setNovaOcorrencia] = useState("");
  const [confirmar, setConfirmar] = useState(null);
  const [registrando, setRegistrando] = useState(false);
  const [salvando, setSalvando] = useState(false);

  async function carregar() {
    setCarregando(true);
    setErro(null);
    try {
      setResultado(await listarColetas({ ...filtros, pagina, tamanhoPagina: 10 }));
    } catch (e) {
      setErro(e.message);
    } finally {
      setCarregando(false);
    }
  }

  // Recarrega sempre que mudar a página OU os filtros (filtro automático).
  useEffect(() => {
    carregar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pagina, filtros]);

  useEffect(() => {
    listarClientes().then(setClientes).catch(() => {});
    listarMotoristas().then(setMotoristas).catch(() => {});
    listarVeiculos().then(setVeiculos).catch(() => {});
  }, []);

  // Ao mexer num filtro, volta pra página 1 e atualiza — o useEffect acima recarrega.
  function mudarFiltro(campo, valor) {
    setPagina(1);
    setFiltros((f) => ({ ...f, [campo]: valor }));
  }

  function limparFiltros() {
    setPagina(1);
    setFiltros(FILTROS_VAZIOS);
  }

  const temFiltro = filtros.status || filtros.clienteId || filtros.inicio || filtros.fim;

  async function exportar() {
    setErro(null);
    setExportando(true);
    try {
      await exportarColetas(filtros);
    } catch (e) {
      setErro(e.message);
    } finally {
      setExportando(false);
    }
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

  async function criar(e) {
    e.preventDefault();
    if (salvando) return; // trava criacao duplicada (double-click)
    setSalvando(true);
    setErro(null);
    try {
      await criarColeta({ ...formColeta, clienteId: Number(formColeta.clienteId) });
      setFormColeta(FORM_COLETA);
      setModalNova(false);
      await carregar();
    } catch (err) {
      setErro(err.message);
    } finally {
      setSalvando(false);
    }
  }

  async function confirmarAtribuir(e) {
    e.preventDefault();
    if (salvando) return;
    const id = atribuirId;
    setSalvando(true);
    setErro(null);
    try {
      await atribuirMotoristaVeiculo(id, {
        motoristaId: Number(atribuirForm.motoristaId),
        veiculoId: Number(atribuirForm.veiculoId),
      });
      setAtribuirId(null);
      setAtribuirForm({ motoristaId: "", veiculoId: "" });
      await carregar();
    } catch (err) {
      setErro(err.message);
    } finally {
      setSalvando(false);
    }
  }

  async function adicionarOcorrencia(e) {
    e.preventDefault();
    if (registrando) return; // trava envio duplicado (double-click)
    const id = ocorrenciaColetaId;
    const desc = novaOcorrencia.trim();
    if (!desc) return;
    setRegistrando(true);
    setErro(null);
    try {
      await registrarOcorrencia(id, desc);
      setNovaOcorrencia("");
      await carregar();
    } catch (err) {
      setErro(err.message);
    } finally {
      setRegistrando(false);
    }
  }

  function cancelar(id) {
    setConfirmar({
      mensagem: "Cancelar esta coleta? Essa ação não pode ser desfeita.",
      textoConfirmar: "Sim, cancelar",
      aoConfirmar: () => acao(() => cancelarColeta(id)),
    });
  }

  const termo = busca.trim().toLowerCase();
  const itens = termo
    ? resultado.itens.filter((c) =>
        [c.numero, c.clienteNome, c.remetenteNome, c.destinatarioNome]
          .some((v) => (v || "").toLowerCase().includes(termo)))
    : resultado.itens;

  const coletaOco = resultado.itens.find((c) => c.id === ocorrenciaColetaId);

  return (
    <div>
      <div className="page-head">
        <h2>Coletas</h2>
        <div className="acoes-topo">
          <button className="secundario" onClick={exportar} disabled={exportando}>
            {exportando ? "Exportando..." : "⬇ Exportar"}
          </button>
          <button onClick={() => setModalNova(true)}>+ Nova coleta</button>
        </div>
      </div>

      <div className="filtros">
        <select value={filtros.status} onChange={(e) => mudarFiltro("status", e.target.value)}>
          <option value="">Todos os status</option>
          <option value="Aberta">Aberta</option>
          <option value="EmColeta">Em Coleta</option>
          <option value="Coletado">Coletado</option>
          <option value="Cancelada">Cancelada</option>
        </select>
        <select value={filtros.clienteId} onChange={(e) => mudarFiltro("clienteId", e.target.value)}>
          <option value="">Todos os clientes</option>
          {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
        </select>
        <label className="filtro-data">
          <span>De</span>
          <input type="date" value={filtros.inicio} onChange={(e) => mudarFiltro("inicio", e.target.value)} />
        </label>
        <label className="filtro-data">
          <span>Até</span>
          <input type="date" value={filtros.fim} onChange={(e) => mudarFiltro("fim", e.target.value)} />
        </label>
        {temFiltro && <button className="secundario" onClick={limparFiltros}>Limpar</button>}
        <input className="busca" placeholder="Buscar nesta página..." value={busca} onChange={(e) => setBusca(e.target.value)} />
      </div>

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
                    <span className={`status status-${c.status}`}>{rotuloStatus(c.status)}</span>
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
          <form className="form" onSubmit={criar}>
            <label className="campo">
              <span>Cliente *</span>
              <select required value={formColeta.clienteId} onChange={(e) => setFormColeta({ ...formColeta, clienteId: e.target.value })}>
                <option value="">Selecione o cliente...</option>
                {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
              </select>
            </label>

            <p className="secao">Remetente (quem envia a carga)</p>
            <label className="campo">
              <span>Nome *</span>
              <input required placeholder="Ex.: Mercado Central" value={formColeta.remetenteNome} onChange={(e) => setFormColeta({ ...formColeta, remetenteNome: e.target.value })} />
            </label>
            <label className="campo">
              <span>Endereço</span>
              <input placeholder="Ex.: Av. Brasil, 1000" value={formColeta.remetenteEndereco} onChange={(e) => setFormColeta({ ...formColeta, remetenteEndereco: e.target.value })} />
            </label>

            <p className="secao">Destinatário (quem recebe)</p>
            <label className="campo">
              <span>Nome *</span>
              <input required placeholder="Ex.: Filial Sul" value={formColeta.destinatarioNome} onChange={(e) => setFormColeta({ ...formColeta, destinatarioNome: e.target.value })} />
            </label>
            <label className="campo">
              <span>Endereço</span>
              <input placeholder="Ex.: Rua D, 80" value={formColeta.destinatarioEndereco} onChange={(e) => setFormColeta({ ...formColeta, destinatarioEndereco: e.target.value })} />
            </label>

            <p className="secao">Detalhes da coleta</p>
            <div className="linha-2">
              <label className="campo">
                <span>Data prevista da coleta *</span>
                <input required type="date" value={formColeta.dataColetaPrevista} onChange={(e) => setFormColeta({ ...formColeta, dataColetaPrevista: e.target.value })} />
              </label>
              <label className="campo">
                <span>Prioridade</span>
                <select value={formColeta.prioridade} onChange={(e) => setFormColeta({ ...formColeta, prioridade: e.target.value })}>
                  <option>Baixa</option>
                  <option>Normal</option>
                  <option>Alta</option>
                </select>
              </label>
            </div>
            <label className="campo">
              <span>Observações</span>
              <input placeholder="Ex.: carga frágil" value={formColeta.observacoes} onChange={(e) => setFormColeta({ ...formColeta, observacoes: e.target.value })} />
            </label>

            <button type="submit" className="full" disabled={salvando}>{salvando ? "Criando..." : "Criar coleta"}</button>
          </form>
        </Modal>
      )}

      {atribuirId && (
        <Modal titulo="Atribuir motorista e veículo" onFechar={() => setAtribuirId(null)}>
          <form className="form" onSubmit={confirmarAtribuir}>
            <label className="campo">
              <span>Motorista *</span>
              <select required value={atribuirForm.motoristaId} onChange={(e) => setAtribuirForm({ ...atribuirForm, motoristaId: e.target.value })}>
                <option value="">Selecione o motorista...</option>
                {motoristas.map((m) => <option key={m.id} value={m.id}>{m.nome}</option>)}
              </select>
            </label>
            <label className="campo">
              <span>Veículo *</span>
              <select required value={atribuirForm.veiculoId} onChange={(e) => setAtribuirForm({ ...atribuirForm, veiculoId: e.target.value })}>
                <option value="">Selecione o veículo...</option>
                {veiculos.map((v) => <option key={v.id} value={v.id}>{v.placa} — {v.modelo}</option>)}
              </select>
            </label>
            <p className="aviso-modal">Ao atribuir, a coleta passa para <strong>Em Coleta</strong>.</p>
            <button type="submit" className="full" disabled={salvando}>{salvando ? "Atribuindo..." : "Atribuir"}</button>
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
          <form className="form" onSubmit={adicionarOcorrencia}>
            <label className="campo">
              <span>Nova ocorrência</span>
              <input required placeholder="Ex.: endereço errado, cliente ausente..." value={novaOcorrencia} onChange={(e) => setNovaOcorrencia(e.target.value)} />
            </label>
            <button type="submit" className="full" disabled={registrando}>{registrando ? "Registrando..." : "Registrar ocorrência"}</button>
          </form>
        </Modal>
      )}

      {erro && <Aviso mensagem={erro} onFechar={() => setErro(null)} />}

      {confirmar && (
        <Confirmacao
          mensagem={confirmar.mensagem}
          textoConfirmar={confirmar.textoConfirmar}
          onConfirmar={() => { confirmar.aoConfirmar(); setConfirmar(null); }}
          onCancelar={() => setConfirmar(null)}
        />
      )}
    </div>
  );
}
