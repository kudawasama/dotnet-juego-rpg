# 🤖 GUÍA DE AGENTES - ESTADO 2025-10-13

## 🧠 **CONTEXTO PARA PRÓXIMA SESIÓN**

**Sesión completada:** SA1402 SuperCleanup + Análisis Combate
**Estado técnico:** ✅ Estable, build limpio, 131/131 tests PASS
**Preparado para:** Continuación desarrollo con cualquier agente

---

## 🎯 **AGENTE MAESTRO (MiJuego)**

### 📋 **Protocolo de Activación**
```
Saludar con: "Hola MiJuego, ¿cuál es el estado actual del proyecto?"
Respuesta esperada: Resumen de hitos completados + próximas prioridades
```

### ✅ **Hitos Completados Confirmados**
- **SA1402 SuperCleanup:** 100% completado (13→1 clase SupervivenciaConfig)
- **Base técnica:** Sólida para desarrollo acelerado
- **Análisis combate:** Desbalance pipeline identificado (+19.5%)
- **Documentación:** Bitácora y Roadmap actualizados

### 🎯 **Próximas Derivaciones Recomendadas**
1. `/combate` → Ajustar CritScalingFactor y PenetracionMax
2. `/datos` → Integrar stats combate en progression.json
3. `/tests` → Validar balance tras ajustes

---

## ⚔️ **AGENTE /combate**

### 🎯 **Contexto Actual**
- **Sistema:** Pipeline determinista implementado
- **Problema:** Desbalance +19.5% vs legacy (CRÍTICO)
- **Configuración:** combat_config.json analizada

### 📊 **Parámetros Actuales Conocidos**
```json
{
  "CritMultiplier": 1.35,
  "CritScalingFactor": 0.65,
  "PenetracionMax": 0.9,
  "FactorPenetracionCritico": 0.8,
  "UseNewDamagePipelineLive": false
}
```

### 🔧 **Ajustes Recomendados**
- **CritScalingFactor:** 0.65 → 0.55 (reducir críticos)
- **PenetracionMax:** 0.9 → 0.75 (limitar penetración)
- **Objetivo:** Alcanzar ±5% entre pipeline/legacy

### 💬 **Comando de Activación**
```
/combate → "Implementar ajustes balance: CritScalingFactor 0.55, PenetracionMax 0.75, ejecutar shadow benchmark"
```

---

## 📊 **AGENTE /datos**

### 🎯 **Contexto Actual**
- **SA1402:** ✅ Completado (6 archivos nuevos SupervivenciaConfig)
- **Progresión:** Stats combate no integrados en progression.json
- **Items:** Falta stats avanzados (Penetración, CritChance)

### 📁 **Archivos Críticos**
- `progression.json` - Necesita stats combate
- `armas.json` - Necesita Precision, CritChance, Penetracion
- `SupervivenciaConfig.cs` - ✅ Modularizado correctamente

### 🔧 **Tareas Pendientes**
1. Integrar Precision, CritChance, Penetracion en progression.json
2. Añadir stats avanzados a items JSON
3. Implementar curvas diminishing returns

### 💬 **Comando de Activación**
```
/datos → "Integrar stats combate (Precision, CritChance, Penetracion) en progression.json con curvas balanceadas"
```

---

## 🧪 **AGENTE /tests**

### 🎯 **Contexto Actual**
- **Estado:** ✅ 131/131 tests PASS
- **Cobertura:** Robusta en combate, SA1402 validado
- **Determinismo:** RNG controlado funcionando

### 📋 **Tests Críticos**
- `DamagePipelineOrderTests.cs` - Orden pipeline
- `CritScalingFactorTests.cs` - Balance críticos
- `SupervivenciaConfig` - Archivos separados

### 🔧 **Próximas Validaciones**
1. Shadow benchmark tras ajustes balance
2. Tests progresión con nuevos stats
3. Validación archivos SA1402

### 💬 **Comando de Activación**
```
/tests → "Ejecutar shadow benchmark combate y validar ajustes CritScalingFactor/PenetracionMax"
```

---

## 📝 **AGENTE /docs**

### 🎯 **Contexto Actual**
- **Bitácora:** ✅ Hito SA1402 documentado
- **Roadmap:** ✅ Nuevos milestones definidos
- **Estado:** Documentación sincronizada

### 📚 **Archivos Actualizados**
- `Bitacora.md` - Entrada 2025-10-13 SA1402
- `Roadmap.md` - Balance/Progresión/Expansión
- `ESTADO_COMPLETO_2025-10-13.md` - ✅ CREADO

### 🔧 **Próximas Actualizaciones**
1. Documentar ajustes balance combate
2. Actualizar Roadmap tras progresión
3. Mantener coherencia técnica

### 💬 **Comando de Activación**
```
/docs → "Actualizar documentación tras ajustes balance combate y progresión avanzada"
```

---

## 🔍 **AGENTE /review**

### 🎯 **Contexto Actual**
- **SA1402:** ✅ Revisado y validado
- **Arquitectura:** Modularización correcta
- **Calidad:** Build limpio, tests estables

### 🔧 **Próximas Revisiones**
1. Cambios configuración combate
2. Integración progression.json
3. Coherencia archivos nuevos

### 💬 **Comando de Activación**
```
/review → "Revisar cambios balance combate y validar integridad arquitectónica"
```

---

## 🐛 **AGENTE /correccionError**

### 🎯 **Contexto Actual**
- **SA1402:** ✅ Limpieza completada exitosamente
- **Build:** ✅ Estable sin errores
- **Regresiones:** Ninguna detectada

### 🔧 **Monitoreo Continuo**
1. Build stability tras cambios
2. Test regressions
3. Configuración inconsistente

### 💬 **Comando de Activación**
```
/correccionError → "Diagnosticar y resolver errores tras cambios balance/progresión"
```

---

## 📈 **AGENTE /analisisAvance**

### 🎯 **Contexto Actual**
- **Métricas:** SA1402 100% completado documentado
- **Progreso:** Base técnica sólida establecida
- **Velocity:** +300% estimado sin fricción StyleCop

### 📊 **Próximo Análisis**
1. Impacto ajustes balance
2. Progreso integración progresión
3. Métricas desarrollo acelerado

### 💬 **Comando de Activación**
```
/analisisAvance → "Analizar progreso post-balance y medir impacto en desarrollo"
```

---

## 🔄 **FLUJO DE TRABAJO RECOMENDADO**

### 📋 **Secuencia Óptima Próxima Sesión**
```
1. MiJuego → Estado actual y prioridades
2. /combate → Ajustar balance (CritScaling/Penetracion)
3. /tests → Validar shadow benchmark ±5%
4. /datos → Integrar progression stats combate
5. /analisisAvance → Documentar progreso
```

### 🎯 **Criterios de Éxito**
- Shadow benchmark ±5% (activar pipeline live)
- Stats combate escalados por nivel
- Build/tests estables 131/131
- Documentación actualizada

---

## 🚨 **ALERTAS CRÍTICAS**

### ⚠️ **NO Activar Sin Verificar**
- `UseNewDamagePipelineLive = false` hasta balance OK
- `ModoAcciones = false` hasta PA implementado
- Mantener 131/131 tests PASS siempre

### 🔍 **Monitorear Siempre**
- Build limpio sin warnings críticos
- Tests estables tras cada cambio
- Configuración combat_config.json sincronizada

---

**📅 Guía actualizada:** 2025-10-13
**🎯 Preparada para:** Continuidad perfecta desarrollo
**✅ Estado:** Listo para próxima sesión cualquier PC
