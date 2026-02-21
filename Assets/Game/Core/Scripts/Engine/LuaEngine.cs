using System;
using System.Collections.Generic;
using Game.Core.Scripts.LuaModules;
using Game.Core.Scripts.LuaModules.Interfaces;
using Lua;
using Lua.Standard;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.Scripts.Engine
{
    public class LuaEngine : MonoBehaviour
    {
        // TODO: ServiceLocator
        public static LuaEngine Global;
        
        [SerializeField] private InputActionAsset inputActions;
        
        public LuaState State => _luaState;
        
        private LuaState _luaState;
        private List<ILuaModule> _activeModules;
        
        // Global modules
        // TODO: Move to a service locator
        public InputModule Inputs { get; private set; }
        public GameLoopModule GameLoop { get; private set; }
        public EventModule Events { get; private set; }

        private void Awake()
        {
            Global = this;
            
            GameLoop = new GameLoopModule();
            Events = new EventModule();
            Inputs = new InputModule(inputActions);
            
            _activeModules = new List<ILuaModule>
            {
                GameLoop,
                Events,
                Inputs,
                new DebugModule(),
                new EntityModule(),
                new TileMapModule(),
                new CameraModule()
            };
            
            InitializeLua();
        }
    
        private void InitializeLua()
        {
            _luaState = LuaState.Create();
            _luaState.OpenStandardLibraries();
            
            foreach (var module in _activeModules)
            {
                module.Register(_luaState);
                Debug.Log($"[LuaEngine] Register module: {module.GetType().Name}");
            }
        }
        
        public void SetGlobal(string variableName, string value)
        {
            if (_luaState == null) return;
            _luaState.Environment[variableName] = value;
        }
    
        public async void ExecuteScript(string scriptCode)
        {
            try
            {
                if (_luaState == null) return;
                await _luaState.DoStringAsync(scriptCode);
            }
            catch (LuaRuntimeException e)
            {
                Debug.LogError($"[LUA EXECUTION ERROR]: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GENERAL ERROR]: {e}");
            }
        }
        
        // TODO: ubicar una clase runtime que funcione como capa directa con funciones de Unity
        private void Update()
        {
            GameLoop?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            GameLoop?.FixedUpdate(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            GameLoop?.LateUpdate(Time.deltaTime);
        }

        public void Reload()
        {
            _luaState?.Dispose();
            _luaState = null;
            
            InitializeLua();
        }
    }
}
