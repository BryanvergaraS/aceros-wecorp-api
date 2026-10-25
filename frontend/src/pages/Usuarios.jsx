import { useState } from 'react'
import { api } from '../api/cliente'
import { fechaHora } from '../api/formato'
import { Aviso, Campo, Dialogo, Encabezado } from '../components/Comunes'
import { useDatos } from '../hooks/useDatos'

export default function Usuarios() {
  const { datos, error, recargar } = useDatos('/usuarios')
  const [creando, setCreando] = useState(false)

  return (
    <section>
      <Encabezado titulo="Usuarios">
        <button type="button" className="btn btn-principal" onClick={() => setCreando(true)}>Nuevo usuario</button>
      </Encabezado>
      <Aviso error={error} />
      <div className="tabla-marco">
        <table className="tabla">
          <thead><tr><th>Nombre</th><th>Email</th><th>Rol</th><th>Teléfono</th><th>Registro</th></tr></thead>
          <tbody>
            {datos?.map((u) => (
              <tr key={u.id} className={u.estado ? '' : 'inactivo'}>
                <td>{u.nombre} {u.apellido}</td>
                <td>{u.email}</td>
                <td><strong>{u.rol}</strong></td>
                <td>{u.telefono}</td>
                <td>{fechaHora(u.fechaRegistro)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {creando && <UsuarioForm onCerrar={() => setCreando(false)} onGuardado={() => { setCreando(false); recargar() }} />}
    </section>
  )
}

function UsuarioForm({ onCerrar, onGuardado }) {
  const { datos: roles } = useDatos('/usuarios/roles')
  const [f, setF] = useState({ rolId: '2', nombre: '', apellido: '', email: '', password: '', telefono: '' })
  const [error, setError] = useState(null)
  const set = (k) => (e) => setF({ ...f, [k]: e.target.value })

  async function guardar(e) {
    e.preventDefault()
    try {
      await api.post('/usuarios', { ...f, rolId: Number(f.rolId), telefono: f.telefono || null })
      onGuardado()
    } catch (err) { setError(err) }
  }

  return (
    <Dialogo abierto titulo="Nuevo usuario" onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <div className="rejilla">
          <Campo etiqueta="Nombre"><input required maxLength={60} value={f.nombre} onChange={set('nombre')} /></Campo>
          <Campo etiqueta="Apellido"><input required maxLength={60} value={f.apellido} onChange={set('apellido')} /></Campo>
        </div>
        <Campo etiqueta="Email"><input type="email" required maxLength={150} value={f.email} onChange={set('email')} /></Campo>
        <div className="rejilla">
          <Campo etiqueta="Contraseña (mínimo 8)"><input type="password" required minLength={8} autoComplete="new-password" value={f.password} onChange={set('password')} /></Campo>
          <Campo etiqueta="Rol">
            <select value={f.rolId} onChange={set('rolId')}>
              {roles?.map((r) => <option key={r.id} value={r.id}>{r.nombre}</option>)}
            </select>
          </Campo>
        </div>
        <Campo etiqueta="Teléfono"><input maxLength={20} value={f.telefono} onChange={set('telefono')} /></Campo>
        <Aviso error={error} />
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal">Crear usuario</button>
        </div>
      </form>
    </Dialogo>
  )
}
