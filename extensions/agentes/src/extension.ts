import * as vscode from 'vscode';

// Plantillas de prompts por agente
const templates: Record<string, string> = {
  combate: `[AgenteCombate]\nContexto: pipeline daño (Base → Hit/Evasión → Penetración → Defensa → Mitigación% → Crítico → Vulnerabilidad → Redondeo).\nInstrucción: Revisar fragmento y detectar desvíos, redundancias, riesgos de performance y tests a ajustar.\nFormato respuesta:\n- Observaciones\n- Riesgos\n- Patch mínimo (diff)\n- Tests afectados\n`,
  datos: `[AgenteDatos]\nContexto: Carga/merge de catálogos JSON (primer archivo base gana, overlay reemplaza por Nombre case-insensitive).\nInstrucción: Validar normalización, fallback rarezas, logs no ruidosos.\nFormato respuesta:\n- Issues\n- Correcciones propuestas\n- Validaciones extra sugeridas\n`,
  tests: `[AgenteTests]\nContexto: determinismo (RandomService.SetSeed), mínimo feliz + edge + fallback.\nInstrucción: Revisar fragmento, detectar test smells y proponer casos faltantes.\nFormato respuesta:\n- Smells\n- Casos faltantes\n- Refactor mínimo\n`,
  infra: `[AgenteInfra]\nContexto: build, pipeline, performance loops críticos (evitar LINQ pesado), carga única de catálogos.\nInstrucción: Señalar potencial deuda y acciones de hardening.\nFormato respuesta:\n- Riesgos infra\n- Hardening\n- Prioridad (Alta/Media/Baja)\n`,
  review: `[AgenteReview]\nContexto: Auditoría general coherencia, SRP, duplicaciones, manejo de null, logs.\nInstrucción: Revisar fragmento y devolver resumen conciso + focos.\nFormato respuesta:\n- Hallazgos\n- Refactor sugerido\n- Impacto\n`
};

function insertTemplate(key: keyof typeof templates) {
  const editor = vscode.window.activeTextEditor;
  const selText = editor ? editor.document.getText(editor.selection) : '';
  const base = templates[key];
  const finalPrompt = selText ? `${base}\nCódigo:\n\n\n${'```'}csharp\n${selText}\n${'```'}\n` : base;
  // Copia al clipboard y muestra info para que el usuario lo pegue en Copilot Chat
  vscode.env.clipboard.writeText(finalPrompt).then(()=>{
    vscode.window.showInformationMessage(`Plantilla '${key}' copiada al portapapeles. Pega en Copilot Chat (Ctrl+V).`);
  });
}

export function activate(context: vscode.ExtensionContext) {
  context.subscriptions.push(
    vscode.commands.registerCommand('agentes.promptCombate', ()=> insertTemplate('combate')),
    vscode.commands.registerCommand('agentes.promptDatos', ()=> insertTemplate('datos')),
    vscode.commands.registerCommand('agentes.promptTests', ()=> insertTemplate('tests')),
    vscode.commands.registerCommand('agentes.promptInfra', ()=> insertTemplate('infra')),
    vscode.commands.registerCommand('agentes.promptReview', ()=> insertTemplate('review'))
  );
}

export function deactivate() {}
