using System;
using System.Collections.Generic;
using Game.Core.Scripts.LuaModules;
using Game.Core.Scripts.LuaModules.Interfaces;
using Lua;
using Lua.Standard;
using UnityEngine;

namespace Game.Core.Scripts
{
    public class LuaEngine : MonoBehaviour
    {
        // TODO: ServiceLocator
        public static LuaEngine Global;
        
        private LuaState _luaState;
        private List<ILuaModule> _activeModules;

        private void Awake()
        {
            Global = this;
            
            _activeModules = new List<ILuaModule>
            {
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

        public void Reload()
        {
            _luaState?.Dispose();
            _luaState = null;
            
            InitializeLua();
        }
    }
}
