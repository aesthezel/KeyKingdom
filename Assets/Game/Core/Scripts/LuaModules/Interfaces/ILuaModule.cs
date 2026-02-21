using Lua;

namespace Game.Core.Scripts.LuaModules.Interfaces
{
    public interface ILuaModule
    {
        void Register(LuaState luaState);
    }
}