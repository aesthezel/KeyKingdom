using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Core.Scripts.Engine;
using Lua;
using UnityEngine;

namespace Game.Core.Scripts.LuaModules
{
    public class EventModule : BaseLuaModule
    {
        public override string ModuleName => "events";
    
        private readonly Dictionary<string, List<LuaValue>> _listeners = new();
    
        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "on", Subscribe);
            Bind(table, "clear", ClearAll);
        }
        
        private ValueTask<int> Subscribe(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            string eventName = context.GetArgument<string>(0);
            LuaValue callback = context.Arguments.Length > 1 ? context.Arguments[1] : LuaValue.Nil;

            if (callback.Type != LuaValueType.Function) return NegativeReturn;

            if (!_listeners.ContainsKey(eventName)) _listeners[eventName] = new List<LuaValue>();
            _listeners[eventName].Add(callback);
            
            return PositiveReturn;
        }

        private ValueTask<int> ClearAll(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            _listeners.Clear();
            return PositiveReturn;
        }
        
        public async void TriggerEvent(string eventName, params object[] args)
        {
            try
            {
                if (!_listeners.TryGetValue(eventName, out var callbacks)) return;
            
                var state = LuaEngine.Global.State;
                var callbacksToRun = new List<LuaValue>(callbacks);
                
                foreach (var callbackValue in callbacksToRun)
                {
                    try 
                    {
                        if (!callbackValue.TryRead<LuaFunction>(out var luaFunc)) continue;
                    
                        foreach (var arg in args)
                        {
                            LuaValue val = LuaValue.FromObject(arg);
                            state.Stack.Push(val);
                        }
                    
                        await state.RunAsync(luaFunc, args.Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventModule] Error crítico en evento '{eventName}': {e}");
                    }
                }
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
            }
        }
    }
}