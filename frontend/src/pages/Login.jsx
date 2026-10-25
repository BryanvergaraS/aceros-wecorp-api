import { useState } from 'react'
import { useSesion } from '../context/Sesion'

export default function Login() {
  const { iniciar } = useSesion()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  async function enviar(e) {
    e.preventDefault()
    setError('')
    setEnviando(true)
    try {
      await iniciar(email, password)
    } catch (err) {
      setError(err.status === 401 ? 'El email o la contraseña no son correctos.' : err.message)
      setEnviando(false)
    }
  }

  return (
    <div className="login">
      <div className="login-placa">
        <p className="login-marca">Aceros We-Corp</p>
        <h1 className="login-titulo">Catálogo y notas de venta</h1>
        <p className="login-sub">Barraca de fierros, Nacimiento</p>
      </div>
      <form className="login-form" onSubmit={enviar}>
        <label className="campo">
          <span className="campo-etiqueta">Email</span>
          <input type="email" autoComplete="username" required value={email}
            onChange={(e) => setEmail(e.target.value)} autoFocus />
        </label>
        <label className="campo">
          <span className="campo-etiqueta">Contraseña</span>
          <input type="password" autoComplete="current-password" required value={password}
            onChange={(e) => setPassword(e.target.value)} />
        </label>
        {error && <p className="aviso" role="alert">{error}</p>}
        <button className="btn btn-principal" disabled={enviando}>
          {enviando ? 'Ingresando…' : 'Ingresar'}
        </button>
      </form>
    </div>
  )
}
