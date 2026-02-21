using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.Scripts.Engine;
using Lua;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.Scripts.LuaModules
{
    public class InputModule : BaseLuaModule
    {
        public override string ModuleName => "inputs";

        private readonly InputActionAsset _asset;
        private readonly Dictionary<string, InputAction> _actionCache = new();
        private readonly Dictionary<InputAction, List<ActionListener>> _listeners = new();

        private class ActionListener
        {
            public string Phase;
            public LuaValue Callback;
        }

        public InputModule(InputActionAsset asset)
        {
            _asset = asset;
        }

        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "enable_map", EnableMap);
            Bind(table, "disable_map", DisableMap);
            
            Bind(table, "is_pressed", IsPressed);
            Bind(table, "was_pressed", WasPressed);
            Bind(table, "was_released", WasReleased);
            
            Bind(table, "get_float", GetFloat);
            Bind(table, "get_vector2", GetVector2);
            Bind(table, "on_action", OnAction);
            
            Bind(table, "debug_maps", DebugMaps);
            Bind(table, "clear", ClearAll);
        }

        private ValueTask<int> EnableMap(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            string map = context.GetArgument<string>(0);
            var actionMap = _asset?.FindActionMap(map, true);
            if (actionMap == null) return NegativeReturn;

            actionMap.Enable();
            return PositiveReturn;
        }

        private ValueTask<int> DisableMap(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            string map = context.GetArgument<string>(0);
            var actionMap = _asset?.FindActionMap(map, true);
            if (actionMap == null) return NegativeReturn;

            actionMap.Disable();
            return PositiveReturn;
        }

        private ValueTask<int> IsPressed(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            var action = GetAction(context);
            if (action == null) return NegativeReturn;

            context.Return(action.IsPressed());
            return PositiveReturn;
        }

        private ValueTask<int> WasPressed(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            var action = GetAction(context);
            if (action == null) return NegativeReturn;

            context.Return(action.WasPressedThisFrame());
            return PositiveReturn;
        }

        private ValueTask<int> WasReleased(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            var action = GetAction(context);
            if (action == null) return NegativeReturn;

            context.Return(action.WasReleasedThisFrame());
            return PositiveReturn;
        }

        private ValueTask<int> GetFloat(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            var action = GetAction(context);
            if (action == null) return NegativeReturn;

            context.Return(action.ReadValue<float>());
            return PositiveReturn;
        }

        private ValueTask<int> GetVector2(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            var action = GetAction(context);

            float x = 0f;
            float y = 0f;

            if (action != null)
            {
                Vector2 v = action.ReadValue<Vector2>();
                x = v.x;
                y = v.y;
            }

            var table = new LuaTable
            {
                ["x"] = x,
                ["y"] = y
            };

            context.Return(table);
            return PositiveReturn;
        }

        private ValueTask<int> OnAction(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            string map = context.GetArgument<string>(0);
            string actionName = context.GetArgument<string>(1);
            string phase = context.GetArgument<string>(2).ToLower();
            LuaValue callback = context.Arguments.Length > 3 ? context.Arguments[3] : LuaValue.Nil;

            if (callback.Type != LuaValueType.Function) return NegativeReturn;

            var action = GetAction(map, actionName);
            if (action == null) return NegativeReturn;

            if (!_listeners.TryGetValue(action, out var list))
            {
                list = new List<ActionListener>();
                _listeners[action] = list;

                action.started += ctx => Trigger(action, "started");
                action.performed += ctx => Trigger(action, "performed");
                action.canceled += ctx => Trigger(action, "canceled");
            }

            list.Add(new ActionListener { Phase = phase, Callback = callback });
            return PositiveReturn;
        }

        private void Trigger(InputAction action, string phase)
        {
            if (!_listeners.TryGetValue(action, out var list)) return;

            var state = LuaEngine.Global.State;
            var snapshot = new List<ActionListener>(list);

            foreach (var l in snapshot)
            {
                if (l.Phase != "any" && l.Phase != phase) continue;
                if (!l.Callback.TryRead<LuaFunction>(out var luaFunc)) continue;

                state.Stack.Push(LuaValue.FromObject(phase));
                state.RunAsync(luaFunc, 1);
            }
        }

        private ValueTask<int> DebugMaps(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            if (_asset == null)
            {
                Debug.LogWarning("[InputModule] InputActionAsset no asignado.");
                return PositiveReturn;
            }

            Debug.Log("[InputModule] ==== ACTION MAPS ====");
            foreach (var map in _asset.actionMaps)
            {
                Debug.Log($"[InputModule] Map: {map.name}");
                foreach (var action in map.actions)
                {
                    Debug.Log($"[InputModule]   - Action: {action.name} | Type: {action.type}");
                }
            }
            Debug.Log("[InputModule] =====================");
            return PositiveReturn;
        }

        private ValueTask<int> ClearAll(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            _listeners.Clear();
            _actionCache.Clear();
            return PositiveReturn;
        }

        private InputAction GetAction(LuaFunctionExecutionContext context)
        {
            string map = context.GetArgument<string>(0);
            string action = context.GetArgument<string>(1);
            return GetAction(map, action);
        }

        private InputAction GetAction(string map, string action)
        {
            if (_asset == null) return null;

            string key = $"{map}/{action}";
            if (_actionCache.TryGetValue(key, out var cached)) return cached;

            var actionMap = _asset.FindActionMap(map, true);
            var inputAction = actionMap?.FindAction(action, true);

            if (inputAction == null) return null;

            _actionCache[key] = inputAction;
            return inputAction;
        }
    }
}