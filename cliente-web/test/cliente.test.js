import assert from 'node:assert/strict'
import test from 'node:test'
import { ApiError, catalogo } from '../src/api/cliente.js'

test('GET rechaza una respuesta JSON inválida del servidor', async (t) => {
  const fetchOriginal = globalThis.fetch
  t.after(() => { globalThis.fetch = fetchOriginal })
  globalThis.fetch = async () => new Response('{invalido', {
    status: 200,
    headers: { 'content-type': 'application/json' }
  })

  await assert.rejects(catalogo.get('/productos'), (error) =>
    error instanceof ApiError
    && error.status === 200
    && error.message === 'El servidor devolvió una respuesta inválida.')
})
