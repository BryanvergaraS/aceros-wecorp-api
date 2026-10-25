import { useState } from 'react'
import { qs } from '../api/cliente'
import { fechaHora } from '../api/formato'
import { Aviso, Encabezado, Paginador } from '../components/Comunes'
import { useDatos } from '../hooks/useDatos'

const TAMANO = 50

export default function Logs() {
  const [pagina, setPagina] = useState(1)
  const { datos, error, cargando, recargar } = useDatos(`/logs-auditoria${qs({ pagina, tamano: TAMANO })}`)

  return (
    <section>
      <Encabezado titulo="Auditoría de peticiones">
        <button type="button" className="btn" onClick={recargar}>Actualizar</button>
      </Encabezado>
      <Aviso error={error} />
      <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`}>
        <table className="tabla tabla-compacta">
          <thead><tr><th>Fecha</th><th>Usuario</th><th>Método</th><th>Ruta</th><th className="num">Código</th><th className="num">Tiempo</th><th>IP</th></tr></thead>
          <tbody>
            {datos?.items.map((l) => (
              <tr key={l.id}>
                <td>{fechaHora(l.fechaRegistro)}</td>
                <td>{l.usuarioEmail ?? 'Sin sesión'}</td>
                <td><span className="sku">{l.metodoHttp}</span></td>
                <td className="ruta">{l.rutaEndpoint}</td>
                <td className={`num ${l.codigoEstadoHttp >= 400 ? 'texto-alerta' : ''}`}>{l.codigoEstadoHttp}</td>
                <td className="num">{l.tiempoRespuestaMs} ms</td>
                <td>{l.ipOrigen}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {datos && <Paginador pagina={pagina} total={datos.total} tamano={TAMANO} onCambiar={setPagina} />}
    </section>
  )
}
