# Extensión local: Agentes Juego RPG

Provee comandos que copian al portapapeles prompts especializados para usarlos en Copilot Chat.

## Instalación local
1. Ir a carpeta `extensions/agentes`.
2. Instalar dependencias (requiere Node 18+): `npm install` (no se crea automáticamente aquí para no hacer commits sin consentimiento).
3. Ejecutar build: `npm run build`.
4. Abrir la carpeta del proyecto en VS Code y ejecutar la paleta de comandos > `Developer: Load Extension` (o usar modo `Run Extension` con F5 si añades un `launch.json`).

## Comandos
- Agente: Prompt Combate
- Agente: Prompt Datos
- Agente: Prompt Tests
- Agente: Prompt Infra
- Agente: Prompt Review

Selecciona código C# antes de lanzar el comando para que lo inserte dentro del prompt.

## Flujo sugerido
1. Selecciona fragmento relevante.
2. Lanza comando (ej: Combate).
3. Ctrl+V en Copilot Chat para pegar el prompt enriquecido.
4. Recibe respuesta estructurada.

## Futuro
- Integrar apertura automática del panel de chat (cuando API pública lo permita).
- Añadir reporte de `@Agente*` con vista dedicada.
- Exponer como participante si GitHub abre API de chat participants.
