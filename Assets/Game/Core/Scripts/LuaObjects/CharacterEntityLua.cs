using Game.Core.Scripts.Systems.EntitySystem;
using Lua;
using UnityEngine;

namespace Game.Core.Scripts.LuaObjects
{
    [LuaObject]
    public partial class CharacterEntityLua : EntityBaseLua
    {
        private CharacterEntity _character;
        
        public CharacterEntityLua(GameObject go, string name, SpriteRenderer spriteRenderer, int layerOrder) : base(go, name, spriteRenderer, layerOrder)
        {
            _character = go.AddComponent<CharacterEntity>();
        }
        
        [LuaMember("move")]
        public void Move(float x, float y)
        {
            _character.Move(new Vector2(x, y));
        }

        [LuaMember("set_stats")]
        public void SetStats(int hp, float speed)
        {
            _character.Health = hp;
            _character.MoveSpeed = speed;
        }

        [LuaMember("health")]
        public int Health => _character.Health;
    }
}