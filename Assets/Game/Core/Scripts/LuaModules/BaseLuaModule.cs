using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.Scripts.LuaModules.Interfaces;
using Lua;

namespace Game.Core.Scripts.LuaModules
{
    public abstract class BaseLuaModule : ILuaModule
    {
        protected static ValueTask<int> PositiveReturn = new(1);
        protected static ValueTask<int> NegativeReturn = new(0);
        
        public virtual string ModuleName { get; }
        
        public void Register(LuaState luaState)
        {
            if (string.IsNullOrEmpty(ModuleName))
            {
                RegisterFunctions(luaState.Environment);
            }
            else
            {
                LuaTable table;
                
                LuaValue existingValue = luaState.Environment[ModuleName];
                
                if (existingValue.Type == LuaValueType.Table)
                {
                    table = existingValue.Read<LuaTable>();
                }
                else
                {
                    table = new LuaTable();
                    luaState.Environment[ModuleName] = table;
                }
                
                RegisterFunctions(table);
            }
        }
        
        protected abstract void RegisterFunctions(LuaTable table);
        
        // Reduce boilerplate
        protected void Bind(LuaTable table, string funcName, Func<LuaFunctionExecutionContext, CancellationToken, ValueTask<int>> func)
        {
            table[funcName] = new LuaFunction(func);
        }
    }
}