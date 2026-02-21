using Lua;
using UnityEngine;

namespace Game.Core.Scripts.LuaModules
{
    public class DebugModule : BaseLuaModule
    {
        public override string ModuleName => "debug";
        
        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "log", (context, ct) =>
            {
                var text = context.GetArgument<string>(0);
                Debug.Log($"[LUA] > {text}");
                return PositiveReturn;
            });

            Bind(table, "error", (context, ct) =>
            {
                var text = context.GetArgument<string>(0);
                Debug.LogError($"[LUA ERROR] > {text}");
                return PositiveReturn;
            });
        }
    }
}