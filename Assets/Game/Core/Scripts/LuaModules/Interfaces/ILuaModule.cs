using Lua;

namespace Game.Core.Scripts.LuaModules.Interfaces
{
    public interface ILuaModule
    {
        string ModuleName { get; }
        void Register(LuaState luaState);
    }
}