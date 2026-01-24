using Game.Core.Scripts.Systems.EntitySystem;
using Lua;
using UnityEngine;

namespace Game.Core.Scripts.LuaObjects
{
    [LuaObject]
    public partial class EntityBaseLua
    {
        private GameObject _gameObject;
        private GameEntity _gameEntity;
        
        private Transform _transform;
        private SpriteRenderer _spriteRenderer;
        
        public GameObject GameObject => _gameObject;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        public EntityBaseLua(GameObject go, string name, SpriteRenderer spriteRenderer, int layerOrder)
        {
            _gameObject = go;
            
            _gameEntity = go.AddComponent<GameEntity>();
            
            _transform = go.transform;
            _gameObject.name = name;
            
            _spriteRenderer = spriteRenderer;
            _spriteRenderer.sortingOrder = layerOrder;
        }
        
        [LuaMember("set_position")]
        public void SetPosition(float x, float y)
        {
            _gameEntity.SetPosition(x, y);
        }

        [LuaMember("destroy")]
        public void Destroy()
        {
            _gameEntity.Kill();
        }
        
        public float X => _transform.position.x;
        public float Y => _transform.position.y;
    }
}