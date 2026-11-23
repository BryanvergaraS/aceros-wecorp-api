# Retrospectivas por sprint

Formato: qué funcionó, qué mejorar y acciones para el siguiente sprint. Se completa al cierre de cada sprint,
con ambos integrantes, y se respalda con una captura del tablero (GitHub Projects) en `docs/gestion/capturas/`.

## Sprint 1 — Caso y requerimientos (Hito 1)
- **Objetivo:** caso validado, ERS v1, backlog inicial, casos de uso y metodología.
- **Qué funcionó:**
- **Qué mejorar:**
- **Acciones para el Sprint 2:**

## Sprint 2 — Diseño (Hito 2)
- **Objetivo:** diagramas de clases, ER, arquitectura, secuencia, despliegue y estados; contratos OpenAPI.
- **Qué funcionó:**
- **Qué mejorar:**
- **Acciones para el Sprint 3:**

## Sprint 3 — Implementación (Hito 3)
- **Objetivo:** APIs en ejecución, cliente integrado y pruebas en curso.
- **Qué funcionó:**
  - Las dos APIs (Catálogo y Ventas) quedaron integradas en `main`, con 57/57 pruebas unitarias pasando y Docker Compose levantando los 4 servicios (SQL Server, Catálogo, Ventas, cliente web) sin intervención manual.
  - La colección de Postman (41 pruebas) terminó validando autenticación, roles, catálogo, ventas e integración entre ambas APIs de punta a punta.
  - La revisión cruzada de Pull Requests funcionó de verdad: se revisó archivo por archivo (appsettings, Program.cs, controladores, servicios) con comentarios concretos antes de aprobar y mergear.
- **Qué mejorar:**
  - Algunas ramas (API de ventas, cliente web, documentación) llegaron a destiempo y sin aviso previo, lo que apretó los plazos de revisión.
  - Faltaban en `main` archivos clave para correr el proyecto completo (`WeCorp.sln`, `docker-compose.yml`); hubo que reconstruirlos y agregarlos aparte.
  - La configuración del entorno de pruebas (Docker Desktop, WSL2, Postman) tomó más tiempo del esperado en Windows.
- **Acciones para el Sprint 4:**
  - Avisar con anticipación cuando se suba una rama nueva, para coordinar la revisión a tiempo.
  - Antes de cerrar un sprint, verificar que el repositorio tenga todo lo necesario para que cualquiera clone y corra el proyecto completo (solución, compose, variables de entorno documentadas).
  - Dejar la suite de Postman completa (22 requests / 41 aserciones) como parte del checklist antes de cualquier entrega.

## Sprint 4 — Entrega final
- **Objetivo:** pruebas, docker compose, README, informe técnico y defensa.
- **Qué funcionó:**
- **Qué mejorar:**
- **Lecciones para próximos proyectos:**
