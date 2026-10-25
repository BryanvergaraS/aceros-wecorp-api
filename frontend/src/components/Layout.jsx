import { NavLink } from 'react-router'
import { useSesion } from '../context/Sesion'

const MENU = [
  { a: '/productos', texto: 'Catálogo' },
  { a: '/notas/nueva', texto: 'Nueva nota de venta' },
  { a: '/notas', texto: 'Notas de venta', exacto: true },
  { a: '/familias', texto: 'Familias', admin: true },
  { a: '/proveedores', texto: 'Proveedores', admin: true },
  { a: '/usuarios', texto: 'Usuarios', admin: true },
  { a: '/logs', texto: 'Auditoría', admin: true }
]

export default function Layout({ children }) {
  const { usuario, esAdmin, cerrar } = useSesion()
  return (
    <div className="marco">
      <aside className="lateral">
        <div className="marca">
          <span className="marca-nombre">Aceros We-Corp</span>
          <span className="marca-lugar">Nacimiento</span>
        </div>
        <nav aria-label="Principal">
          {MENU.filter((m) => !m.admin || esAdmin).map((m) => (
            <NavLink key={m.a} to={m.a} end={m.exacto} className="nav-item">{m.texto}</NavLink>
          ))}
        </nav>
        <div className="usuario">
          <span>{usuario.nombre} {usuario.apellido}</span>
          <span className="usuario-rol">{usuario.rol}</span>
          <button type="button" className="btn-texto" onClick={cerrar}>Cerrar sesión</button>
        </div>
      </aside>
      <main className="contenido">{children}</main>
    </div>
  )
}
