# Guía de Arquitectura y Modding - KeyKingdom

Este documento detalla la arquitectura del sistema de modding basado en Lua, los patrones de diseño utilizados, una evaluación técnica y la guía para nuevos desarrolladores.

---

## 1. Arquitectura del Sistema

El sistema utiliza **Lua** como lenguaje de scripting para extender las funcionalidades del juego sin modificar el código fuente de C#. La integración se basa en tres pilares:

1.  **LuaEngine**: El orquestador que gestiona el estado de Lua y el ciclo de vida de los scripts.
2.  **Módulos (Modules)**: APIs estáticas expuestas a Lua (ej. `events`, `entity`, `tilemap`).
3.  **Objetos de Lua (LuaObjects)**: Representaciones de objetos de Unity/C# dentro del entorno Lua.

---

## 2. Patrones de Diseño Utilizados

### A. Patrón Bridge (Puente)
Se utiliza para desacoplar la lógica de alto nivel en Lua de la implementación de bajo nivel en Unity/C#. 
- **Implementación**: `BaseLuaModule` y sus derivados actúan como el puente, traduciendo llamadas de Lua a operaciones de C#.

### B. Patrón Observer (Observador)
Implementado a través del `EventModule`.
- **Implementación**: Los scripts de Lua pueden suscribirse a eventos (`events.on("name", callback)`) que C# dispara (`TriggerEvent`). Esto permite un acoplamiento laxo entre el motor y los mods.

### C. Patrón Wrapper / Proxy
Utilizado para exponer objetos complejos de C# a Lua de forma segura.
- **Implementación**: Clases como `EntityBaseLua` envuelven un `GameObject` y exponen solo los métodos marcados con `[LuaMember]`. Esto previene que el script de Lua acceda a miembros sensibles de Unity no autorizados.

### D. Patrón de Registro de Módulos (Service Provider)
`LuaEngine` mantiene una lista de `ILuaModule` que se registran al inicializar el estado.
- **Implementación**: Cada módulo define su nombre y registra sus funciones en una tabla global de Lua, manteniendo la API organizada y modular.

---

## 3. Evaluación del Sistema

### Puntos Fuertes (Pros)
- **Aislamiento**: Los mods no pueden romper el motor fácilmente gracias a la capa de abstracción de `LuaObjects`.
- **Asincronía**: El uso de `ValueTask` y `async/await` en la ejecución de scripts permite que las operaciones de Lua no bloqueen el hilo principal de Unity (necesario para operaciones pesadas).
- **Extensibilidad**: Añadir una nueva funcionalidad es tan simple como heredar de `BaseLuaModule` y registrar el nuevo módulo en `LuaEngine`.

### Áreas de Mejora (Contras / Deuda Técnica)
- **Registro Manual**: Actualmente, los módulos deben añadirse manualmente a la lista en `LuaEngine.Awake()`. Se recomienda evolucionar hacia un sistema de *Reflection* o *Dependency Injection* para auto-descubrimiento de módulos.
- **Boilerplate en Módulos**: Cada función de módulo requiere una firma específica (`ValueTask<int>`). Aunque `Bind` ayuda, el mapeo de argumentos es manual.
- **Gestión de Memoria**: Es crucial asegurar que los `LuaTable` y callbacks se limpien correctamente al descargar mods para evitar fugas de memoria.

---

## 4. Guía para Nuevos Desarrolladores

### Cómo crear un nuevo Módulo de API
Para añadir nuevas funciones globales a Lua (ej: `myapi.do_something()`):

1.  Crea una clase en `Assets/Game/Core/Scripts/LuaModules/` que herede de `BaseLuaModule`.
2.  Define el `ModuleName`.
3.  Implementa `RegisterFunctions` usando `Bind`.
4.  Añade tu módulo a la lista `_activeModules` en `LuaEngine.cs`.

```csharp
public class MyModule : BaseLuaModule {
    public override string ModuleName => "myapi";
    protected override void RegisterFunctions(LuaTable table) {
        Bind(table, "hello", HelloFunc);
    }
    private ValueTask<int> HelloFunc(LuaFunctionExecutionContext context, CancellationToken ct) {
        Debug.Log("Hola desde Lua!");
        return PositiveReturn;
    }
}
```

### Cómo exponer un Objeto C# a Lua
Si quieres que una clase de C# sea manipulable desde Lua:

1.  Usa el atributo `[LuaObject]` en la clase.
2.  Usa `[LuaMember("nombre_en_lua")]` en los métodos o propiedades que quieras exponer.
3.  Asegúrate de que la clase sea `partial` (requerido por el generador de bindings si se usa).

```csharp
[LuaObject]
public partial class PlayerWrapper {
    [LuaMember("jump")]
    public void Jump() { /* lógica */ }
}
```

### Estructura de un Mod
Los mods deben residir en la carpeta `/Modules` y seguir esta estructura:
- `nombre_del_mod/`
    - `module.json`: Metadatos y punto de entrada.
    - `init.lua`: Script principal.
    - `Assets/`: Recursos (sprites, sonidos).
