using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game.Core.Scripts.Data;
using Game.Core.Scripts.Loaders;
using Lua;
using UnityEngine;

namespace Game.Core.Scripts.LuaModules
{
    public class BasicMapModule : BaseLuaModule
    {
        public override string ModuleName => "map";
        
        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "load", LoadMap);
            Bind(table, "clear", ClearMap);
        }
        
        private async ValueTask<int> LoadMap(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            string relativePath = context.GetArgument<string>(0);
            string mapJsonFile = context.GetArgument<string>(1);
            
            _spriteCache.Clear();
            
            string fullPath = Path.Combine(relativePath, mapJsonFile.TrimStart('/', '\\'));
            
            Debug.Log(relativePath);
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[MapModule] Map not found: {fullPath}");
                return await NegativeReturn;
            }
            
            string json = await File.ReadAllTextAsync(fullPath, ct);
            MapDefinition mapData = JsonUtility.FromJson<MapDefinition>(json);
            
            GameObject mapRoot = new GameObject($"Map_{mapData.id}");
            var mapLegendItems = mapData.legend.ToDictionary(k => k.symbol, v => v);
            float tileSize = mapData.tileSize;
            
            for (int row = 0; row < mapData.layout.Length; row++)
            {
                string line = mapData.layout[row];
                for (int col = 0; col < line.Length; col++)
                {
                    string charKey = line[col].ToString();
                    if (!mapLegendItems.TryGetValue(charKey, out MapLegendItem item)) continue;

                    Vector3 position = new Vector3(col * tileSize, -row * tileSize, 0);
                    
                    SpawnTile(relativePath, item, position, mapRoot.transform);
                }
                
                if (row % 2 == 0)
                {
                    await Task.Yield(); 
                }
            }

            Debug.Log($"[MapModule] Map loaded: {mapData.id} ({mapData.layout.Length} rows)");
            return await PositiveReturn;
        }

        private void SpawnTile(string relativePath, MapLegendItem item, Vector3 position, Transform parent)
        {
            GameObject go = new GameObject(item.spriteFile);
            go.transform.position = position;
            go.transform.SetParent(parent);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            
            string fullSpritePath = Path.Combine(relativePath, item.spriteFile.TrimStart('/', '\\'));

            Sprite sprite;
            
            if (_spriteCache.TryGetValue(fullSpritePath, out Sprite cachedSprite))
            {
                sprite = cachedSprite;
            }
            else
            {
                sprite = ResourceLoader.LoadSprite(fullSpritePath);
                
                if (sprite != null)
                {
                    _spriteCache.Add(fullSpritePath, sprite);
                }
            }
            
            if (sprite != null) sr.sprite = sprite;
            
            if (item.hasCollision)
            {
                go.AddComponent<BoxCollider2D>();
            }
        }

        private ValueTask<int> ClearMap(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            _spriteCache.Clear();

            var maps = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                             .Where(g => g.name.StartsWith("Map_"));
            
            foreach (var m in maps) Object.Destroy(m);
            
            return new ValueTask<int>(1);
        }
    }
}