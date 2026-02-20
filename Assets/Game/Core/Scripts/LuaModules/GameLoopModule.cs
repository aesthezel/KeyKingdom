using System.Threading;
using System.Threading.Tasks;
using Lua;

namespace Game.Core.Scripts.LuaModules
{
    public class GameLoopModule : BaseLuaModule
    {
        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "getTime", GetTime);
        }
        
        private ValueTask<int> GetTime(LuaFunctionExecutionContext context, CancellationToken ct)
        {
            return NegativeReturn;
        }
    }
}