using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.Scripts.Engine;
using Lua;

namespace Game.Core.Scripts.LuaModules
{
    public class GameLoopModule : BaseLuaModule
    {
        public override string ModuleName => "game_loop";
        
        private readonly System.Collections.Generic.List<LuaValue> _updateCallbacks = new();
        private readonly System.Collections.Generic.List<LuaValue> _fixedCallbacks = new();
        private readonly System.Collections.Generic.List<LuaValue> _lateCallbacks = new();

        private readonly System.Collections.Generic.List<TickListener> _tickCallbacks = new();
        private int _nextTickId = 0;

        private class TickListener
        {
            public int Id;
            public float Interval;
            public float Elapsed;
            public LuaValue Callback;
        }

        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "get_time", GetTime);
            Bind(table, "check_update", OnUpdate);
            Bind(table, "check_physics_update", OnFixed);
            Bind(table, "check_late_update", OnLate);
            Bind(table, "configure_tick", ConfigureTick);
            Bind(table, "set_tick", SetTick);
            Bind(table, "clear_tick", ClearTick);
            Bind(table, "clear", ClearAll);
        }

        private ValueTask<int> GetTime(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            context.Return(UnityEngine.Time.time);
            return PositiveReturn;
        }

        private ValueTask<int> OnUpdate(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            return RegisterCallback(context, _updateCallbacks);
        }

        private ValueTask<int> OnFixed(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            return RegisterCallback(context, _fixedCallbacks);
        }

        private ValueTask<int> OnLate(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            return RegisterCallback(context, _lateCallbacks);
        }

        private ValueTask<int> ConfigureTick(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            float interval = context.GetArgument<float>(0);
            LuaValue callback = context.Arguments.Length > 1 ? context.Arguments[1] : LuaValue.Nil;

            if (interval <= 0f || callback.Type != LuaValueType.Function) return NegativeReturn;

            var listener = new TickListener
            {
                Id = ++_nextTickId,
                Interval = interval,
                Elapsed = 0f,
                Callback = callback
            };

            _tickCallbacks.Add(listener);
            context.Return(listener.Id);

            return PositiveReturn;
        }
        
        private ValueTask<int> SetTick(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            int id = context.GetArgument<int>(0);
            float newInterval = context.GetArgument<float>(1);

            if (newInterval <= 0f) return NegativeReturn;

            var tick = _tickCallbacks.Find(t => t.Id == id);
            if (tick == null) return NegativeReturn;

            tick.Interval = newInterval;
            
            tick.Elapsed = 0f;

            return PositiveReturn;
        }

        private ValueTask<int> ClearTick(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            int id = context.GetArgument<int>(0);
            _tickCallbacks.RemoveAll(t => t.Id == id);
            return PositiveReturn;
        }

        private ValueTask<int> ClearAll(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            _updateCallbacks.Clear();
            _fixedCallbacks.Clear();
            _lateCallbacks.Clear();
            _tickCallbacks.Clear();
            return PositiveReturn;
        }

        private ValueTask<int> RegisterCallback(LuaFunctionExecutionContext context, System.Collections.Generic.List<LuaValue> list)
        {
            LuaValue callback = context.Arguments.Length > 0 ? context.Arguments[0] : LuaValue.Nil;
            if (callback.Type != LuaValueType.Function) return NegativeReturn;

            list.Add(callback);
            return PositiveReturn;
        }

        public void Update(float dt)
        {
            RunCallbacks(_updateCallbacks, dt);
            UpdateTicks(dt);
        }

        public void FixedUpdate(float dt)
        {
            RunCallbacks(_fixedCallbacks, dt);
        }

        public void LateUpdate(float dt)
        {
            RunCallbacks(_lateCallbacks, dt);
        }

        private async void RunCallbacks(System.Collections.Generic.List<LuaValue> callbacks, float dt)
        {
            if (callbacks.Count == 0) return;

            var state = LuaEngine.Global.State;
            var snapshot = new System.Collections.Generic.List<LuaValue>(callbacks);

            foreach (var callbackValue in snapshot)
            {
                if (!callbackValue.TryRead<LuaFunction>(out var luaFunc)) continue;

                state.Stack.Push(LuaValue.FromObject(dt));
                await state.RunAsync(luaFunc, 1);
            }
        }

        private async void UpdateTicks(float dt)
        {
            try
            {
                if (_tickCallbacks.Count == 0) return;

                var state = LuaEngine.Global.State;
                var snapshot = new System.Collections.Generic.List<TickListener>(_tickCallbacks);

                foreach (var tick in snapshot)
                {
                    tick.Elapsed += dt;
                
                    int safety = 0;
                    while (tick.Elapsed >= tick.Interval && safety < 3)
                    {
                        tick.Elapsed -= tick.Interval;
                        safety++;

                        if (!tick.Callback.TryRead<LuaFunction>(out var luaFunc)) break;

                        state.Stack.Push(LuaValue.FromObject(tick.Interval));
                        await state.RunAsync(luaFunc, 1);
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