import { useEffect, useState } from 'react'
import { catalogo, errorDe } from '../api/cliente'
import { fechaHora } from '../api/formato'
import { Aviso, Campo, Cargando, Dialogo, Encabezado } from '../components/Comunes'
import { useSesion } from '../context/Sesion'
import { useDatos } from '../hooks/useDatos'

// Usuarios del sistema (RF05, solo Administrador): crear, cambiar rol y activar o desactivar.
export default function Usuarios() {
  const { usuario: yo } = useSesion()
  const { datos, error, cargando, recargar } = useDatos(catalogo, '/usuarios')
  const { datos: roles } = useDatos(catalogo, '/usuarios/roles')
  const [editando, setEditando] = useState(null) // null = cerrado, {} = nuevo, usuario = editar

  return (
    <section>
      <Encabezado titulo="Usuarios">
        <button type="button" className="btn btn-principal" onClick={() => setEditando({})}>Nuevo usuario</button>
      </Encabezado>
      <Aviso error={error} />
      {cargando && !datos && <Cargando />}
      {datos && (
        <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`}>
          <table className="tabla">
            <thead><tr><th>Nombre</th><th>Email</th><th>Rol</th><th>Estado</th><th>Registro</th><th><span className="oculto">Acciones</span></th></tr></thead>
            <tbody>
              {datos.map((u) => (
                <tr key={u.id} className={u.activo ? '' : 'inactivo'}>
                  <td>{u.nombre} {u.apellido}{u.id === yo.id && <span className="etiqueta-inactivo">Tú</span>}</td>
                  <td>{u.email}</td>
                  <td><strong>{u.rol}</strong></td>
                  <td>{u.activo ? 'Activo' : 'Inactivo'}</td>
                  <td className="nowrap">{fechaHora(u.fechaRegistro)}</td>
                  <td className="acciones"><button type="button" className="btn-texto" onClick={() => setEditando(u)}>Editar</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {editando && (
        <UsuarioForm usuario={editando} roles={roles ?? []} esYo={editando.id === yo.id}
          onCerrar={() => setEditando(null)} onGuardado={() => { setEditando(null); recargar() }} />
      )}
    </section>
  )
}

function UsuarioForm({ usuario, roles, esYo, onCerrar, onGuardado }) {
  const nuevo = !usuario.id
  const rolInicial = roles.find((r) => r.nombre === usuario.rol)?.id ?? roles.find((r) => r.nombre === 'Vendedor')?.id ?? ''
  const [f, setF] = useState({
    rolId: String(rolInicial),
    nombre: usuario.nombre ?? '',
    apellido: usuario.apellido ?? '',
    email: usuario.email ?? '',
    password: '',
    telefono: usuario.telefono ?? '',
    activo: usuario.activo ?? true
  })
  const [error, setError] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const set = (k) => (e) => setF({ ...f, [k]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })

  // Si el diálogo se abrió antes de que llegaran los roles, se completa el rol al recibirlos.
  useEffect(() => {
    if (rolInicial !== '') setF((x) => (x.rolId === '' ? { ...x, rolId: String(rolInicial) } : x))
  }, [rolInicial])

  async function guardar(e) {
    e.preventDefault()
    setGuardando(true)
    setError(null)
    const comunes = {
      rolId: Number(f.rolId),
      nombre: f.nombre.trim(),
      apellido: f.apellido.trim(),
      telefono: f.telefono.trim() || null
    }
    try {
      if (nuevo) await catalogo.post('/usuarios', { ...comunes, email: f.email.trim(), password: f.password })
      else await catalogo.put(`/usuarios/${usuario.id}`, { ...comunes, activo: f.activo })
      onGuardado()
    } catch (err) {
      setError(err)
      setGuardando(false)
    }
  }

  const e = error?.errores
  return (
    <Dialogo abierto titulo={nuevo ? 'Nuevo usuario' : `Editar ${usuario.nombre} ${usuario.apellido}`} onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <div className="rejilla">
          <Campo etiqueta="Nombre" error={errorDe(e, 'nombre')}><input required maxLength={60} value={f.nombre} onChange={set('nombre')} /></Campo>
          <Campo etiqueta="Apellido" error={errorDe(e, 'apellido')}><input required maxLength={60} value={f.apellido} onChange={set('apellido')} /></Campo>
        </div>
        {nuevo ? (
          <>
            <Campo etiqueta="Email" error={errorDe(e, 'email')}><input type="email" required maxLength={150} value={f.email} onChange={set('email')} /></Campo>
            <Campo etiqueta="Contraseña (mínimo 8 caracteres)" error={errorDe(e, 'password')}>
              <input type="password" required minLength={8} maxLength={100} autoComplete="new-password" value={f.password} onChange={set('password')} />
            </Campo>
          </>
        ) : (
          <p className="texto-suave">Email: {usuario.email}</p>
        )}
        <div className="rejilla">
          <Campo etiqueta="Rol" error={errorDe(e, 'rolId')}>
            <select required value={f.rolId} onChange={set('rolId')}>
              <option value="">Seleccione</option>
              {roles.map((r) => <option key={r.id} value={r.id}>{r.nombre}</option>)}
            </select>
          </Campo>
          <Campo etiqueta="Teléfono" error={errorDe(e, 'telefono')}><input maxLength={20} value={f.telefono} onChange={set('telefono')} /></Campo>
        </div>
        {!nuevo && (
          <label className="check">
            <input type="checkbox" checked={f.activo} onChange={set('activo')} disabled={esYo} /> Usuario activo
            {esYo && <span className="texto-suave">(no puedes desactivarte a ti mismo)</span>}
          </label>
        )}
        {error && !e && <Aviso error={error} />}
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal" disabled={guardando}>
            {guardando ? 'Guardando…' : nuevo ? 'Crear usuario' : 'Guardar cambios'}
          </button>
        </div>
      </form>
    </Dialogo>
  )
}
