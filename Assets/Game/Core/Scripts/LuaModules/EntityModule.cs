using System.Threading.Tasks;
using Game.Core.Scripts.Loaders;
using Game.Core.Scripts.LuaObjects;
using Game.Core.Scripts.Systems.EntitySystem;
using Lua;
using UnityEngine;

namespace Game.Core.Scripts.LuaModules
{
    public class EntityModule : BaseLuaModule
    {
        public override string ModuleName => "entity";
        
        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "spawn", SpawnEntity);
            Bind(table, "destroy", DestroyEntity);
        }

        private ValueTask<int> SpawnEntity(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            string type = context.GetArgument<string>(0).ToLower();
            string imageName = context.GetArgument<string>(1);
            string name = context.GetArgument<string>(2);
            float x = context.GetArgument<float>(3);
            float y = context.GetArgument<float>(4);
            int layerOrder = context.GetArgument<int>(5);

            GameObject go = new GameObject($"LuaEntity::{name}_{type}");
            go.transform.position = new Vector3(x, y, 0);

            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            
            Sprite sprite = ResourceLoader.LoadSprite(imageName);
            sprite.name = name;
            if (sprite != null) spriteRenderer.sprite = sprite;

            EntityBaseLua wrapper = type switch
            {
                "character" => new CharacterEntityLua(go, name, spriteRenderer, layerOrder),
                _ => new EntityBaseLua(go, name, spriteRenderer, layerOrder)
            };

            context.Return(wrapper);
            
            return PositiveReturn;
        }
        
        private ValueTask<int> DestroyEntity(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            var entity = context.GetArgument<EntityBaseLua>(0);
            entity.Destroy();
            return PositiveReturn;
        }
    }
}