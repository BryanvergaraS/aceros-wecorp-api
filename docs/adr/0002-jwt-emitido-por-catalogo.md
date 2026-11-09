# ADR 0002 — JWT emitido por la API de Catálogo y validado por ambas APIs con llave simétrica

- **Estado:** aceptada
- **Fecha:** semana 2 (Hito 2)
- **Requerimientos relacionados:** RF01, RF12, RNF02, RNF03, RNF04

## Contexto
El enunciado pide autenticación con JWT emitido por una de las APIs y validado por las demás, y autorización por al menos dos roles. Los secretos no pueden estar en el repositorio.

## Decisión
- La API de Catálogo e Identidad, dueña de los usuarios, expone `POST /api/v1/auth/login`, verifica la contraseña con **BCrypt** y emite un JWT firmado con **HMAC-SHA256** que dura **60 minutos** y contiene el id, nombre, email y rol del usuario.
- Ambas APIs validan firma, emisor (`WeCorp.Catalogo`), audiencia (`WeCorp.Sistema`) y expiración con la **misma llave**, que llega por la variable de entorno `Jwt__Key` (nunca en `appsettings.json` ni en Git).
- Autorización con `[Authorize(Roles = ...)]`: roles **Administrador** y **Vendedor**.
- Cuando la API de Ventas llama a la de Catálogo, **reenvía el JWT del usuario** (`ReenviarTokenHandler`), de modo que Catálogo también aplica sus reglas de autorización.

## Alternativas descartadas
- **Llave asimétrica (RS256) con JWKS:** más segura para muchos servicios, pero agrega infraestructura que no se justifica con dos APIs.
- **Token de servicio aparte para Ventas → Catálogo:** evita depender del token del usuario, pero agrega un segundo secreto y otro flujo de emisión.

## Consecuencias
- (+) Una sola fuente de identidad; configuración simple.
- (−) Quien tenga la llave puede emitir tokens válidos: debe protegerse como secreto y rotarse si se filtra.
- (−) Un Vendedor puede llamar directamente a `/productos/stock/descontar`; se acepta porque es la misma operación que hace al vender, y queda como mejora futura restringirlo a un token de servicio.
