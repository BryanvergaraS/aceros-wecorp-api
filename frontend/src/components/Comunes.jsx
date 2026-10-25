import { memo, useEffect, useRef } from 'react'
import { numero } from '../api/formato'

export function Encabezado({ titulo, children }) {
  return (
    <header className="encabezado">
      <h1>{titulo}</h1>
      <div className="encabezado-acciones">{children}</div>
    </header>
  )
}

export function Aviso({ error }) {
  if (!error) return null
  return <p className="aviso" role="alert">{error.message ?? String(error)}</p>
}

// Diálogo nativo <dialog>: accesible, con Esc para cerrar y sin librerías.
export function Dialogo({ abierto, titulo, onCerrar, children }) {
  const ref = useRef(null)
  useEffect(() => {
    const d = ref.current
    if (abierto && !d.open) d.showModal()
    if (!abierto && d.open) d.close()
  }, [abierto])
  return (
    <dialog ref={ref} className="dialogo" onClose={onCerrar} aria-labelledby="dialogo-titulo">
      {abierto && (
        <>
          <header className="dialogo-cabeza">
            <h2 id="dialogo-titulo">{titulo}</h2>
            <button type="button" className="btn-texto" onClick={onCerrar} aria-label="Cerrar">Cerrar</button>
          </header>
          {children}
        </>
      )}
    </dialog>
  )
}

export function Campo({ etiqueta, error, children }) {
  return (
    <label className="campo">
      <span className="campo-etiqueta">{etiqueta}</span>
      {children}
      {error && <span className="campo-error">{error}</span>}
    </label>
  )
}

export function Paginador({ pagina, total, tamano, onCambiar }) {
  const paginas = Math.max(1, Math.ceil(total / tamano))
  if (paginas <= 1) return <p className="paginador">{numero(total)} resultados</p>
  return (
    <nav className="paginador" aria-label="Paginación">
      <button type="button" className="btn" disabled={pagina <= 1} onClick={() => onCambiar(pagina - 1)}>Anterior</button>
      <span>Página {pagina} de {paginas}</span>
      <span className="paginador-total">{numero(total)} resultados</span>
      <button type="button" className="btn" disabled={pagina >= paginas} onClick={() => onCambiar(pagina + 1)}>Siguiente</button>
    </nav>
  )
}

// Barra de stock: muestra el nivel contra el stock mínimo, como una marca en la barra.
export const BarraStock = memo(function BarraStock({ stock, minimo }) {
  const tope = Math.max(minimo * 3, stock, 1)
  const bajo = stock <= minimo
  return (
    <div className={`stock ${bajo ? 'stock-bajo' : ''}`} title={`Stock ${stock}, mínimo ${minimo}`}>
      <span className="stock-numero">{numero(stock)}</span>
      <span className="stock-barra" aria-hidden="true">
        <span className="stock-relleno" style={{ width: `${(stock / tope) * 100}%` }} />
        <span className="stock-minimo" style={{ left: `${(minimo / tope) * 100}%` }} />
      </span>
      {bajo && <span className="stock-texto">Bajo el mínimo</span>}
    </div>
  )
})

export function EstadoNota({ estado }) {
  return <span className={`estado estado-${estado.toLowerCase()}`}>{estado}</span>
}
