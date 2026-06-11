import { useEffect, useState } from "react";
import {
  listarMotoristas,
  criarMotorista,
  listarVeiculos,
  criarVeiculo,
} from "./api";

// Popup de aviso/erro (mesmo estilo do app).
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

const linhaForm = { display: "flex", gap: 8, margin: "0 0 14px", flexWrap: "wrap" };
const campoFlex = { flex: 1, minWidth: 110 };

export default function Cadastros() {
  const [motoristas, setMotoristas] = useState([]);
  const [veiculos, setVeiculos] = useState([]);
  const [mot, setMot] = useState({ nome: "", cnh: "" });
  const [vei, setVei] = useState({ placa: "", modelo: "" });
  const [salvandoMot, setSalvandoMot] = useState(false);
  const [salvandoVei, setSalvandoVei] = useState(false);
  const [aviso, setAviso] = useState("");

  async function carregar() {
    try {
      setMotoristas(await listarMotoristas());
      setVeiculos(await listarVeiculos());
    } catch (e) {
      setAviso(e.message);
    }
  }

  useEffect(() => {
    carregar();
  }, []);

  async function cadastrarMotorista(e) {
    e.preventDefault();
    if (salvandoMot) return;
    setSalvandoMot(true);
    try {
      await criarMotorista(mot);
      setMot({ nome: "", cnh: "" });
      await carregar();
    } catch (err) {
      setAviso(err.message);
    } finally {
      setSalvandoMot(false);
    }
  }

  async function cadastrarVeiculo(e) {
    e.preventDefault();
    if (salvandoVei) return;
    setSalvandoVei(true);
    try {
      await criarVeiculo(vei);
      setVei({ placa: "", modelo: "" });
      await carregar();
    } catch (err) {
      setAviso(err.message);
    } finally {
      setSalvandoVei(false);
    }
  }

  return (
    <div>
      <div className="page-head">
        <h2>Cadastros</h2>
      </div>

      <div className="dash-grid">
        {/* ---- Motoristas ---- */}
        <section className="painel">
          <h3>Motoristas</h3>
          <form style={linhaForm} onSubmit={cadastrarMotorista}>
            <input
              style={campoFlex}
              placeholder="Nome"
              value={mot.nome}
              onChange={(e) => setMot({ ...mot, nome: e.target.value })}
              required
            />
            <input
              style={campoFlex}
              placeholder="CNH"
              value={mot.cnh}
              onChange={(e) => setMot({ ...mot, cnh: e.target.value })}
              required
            />
            <button type="submit" disabled={salvandoMot}>
              {salvandoMot ? "Salvando..." : "+ Cadastrar"}
            </button>
          </form>
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>CNH</th>
              </tr>
            </thead>
            <tbody>
              {motoristas.map((m) => (
                <tr key={m.id}>
                  <td>{m.nome}</td>
                  <td>{m.cnh}</td>
                </tr>
              ))}
              {motoristas.length === 0 && (
                <tr>
                  <td className="vazio" colSpan="2">Nenhum motorista cadastrado.</td>
                </tr>
              )}
            </tbody>
          </table>
        </section>

        {/* ---- Veículos ---- */}
        <section className="painel">
          <h3>Veículos</h3>
          <form style={linhaForm} onSubmit={cadastrarVeiculo}>
            <input
              style={campoFlex}
              placeholder="Placa"
              value={vei.placa}
              onChange={(e) => setVei({ ...vei, placa: e.target.value })}
              required
            />
            <input
              style={campoFlex}
              placeholder="Modelo"
              value={vei.modelo}
              onChange={(e) => setVei({ ...vei, modelo: e.target.value })}
              required
            />
            <button type="submit" disabled={salvandoVei}>
              {salvandoVei ? "Salvando..." : "+ Cadastrar"}
            </button>
          </form>
          <table>
            <thead>
              <tr>
                <th>Placa</th>
                <th>Modelo</th>
              </tr>
            </thead>
            <tbody>
              {veiculos.map((v) => (
                <tr key={v.id}>
                  <td>{v.placa}</td>
                  <td>{v.modelo}</td>
                </tr>
              ))}
              {veiculos.length === 0 && (
                <tr>
                  <td className="vazio" colSpan="2">Nenhum veículo cadastrado.</td>
                </tr>
              )}
            </tbody>
          </table>
        </section>
      </div>

      {aviso && <Aviso mensagem={aviso} onFechar={() => setAviso("")} />}
    </div>
  );
}
