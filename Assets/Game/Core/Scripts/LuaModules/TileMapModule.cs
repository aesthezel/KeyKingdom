using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game.Core.Scripts.Data;
using Game.Core.Scripts.Engine;
using Game.Core.Scripts.Loaders;
using Lua;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core.Scripts.LuaModules
{
    public class TileMapModule : BaseLuaModule
    {
        public override string ModuleName => "tilemap";
        private readonly Dictionary<string, Tile> _tileCache = new();

        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "load", LoadMap);
            Bind(table, "clear", ClearMap);
        }

        private async ValueTask<int> LoadMap(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            string relativePath = context.GetArgument<string>(0);
            string mapJsonFile = context.GetArgument<string>(1);

            _tileCache.Clear();

            string fullPath = Path.Combine(relativePath, mapJsonFile.TrimStart('/', '\\'));

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[MapModule] Map not found: {fullPath}");
                return await NegativeReturn;
            }

            string json = await File.ReadAllTextAsync(fullPath, ct);
            MapDefinition mapData = JsonUtility.FromJson<MapDefinition>(json);
            
            GameObject gridGo = new GameObject($"Map_{mapData.id}");
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(mapData.tileSize, mapData.tileSize, 0);
            
            Dictionary<string, LayerContext> layers = new Dictionary<string, LayerContext>();
            var mapLegendItems = mapData.legend.ToDictionary(k => k.symbol, v => v);
            
            int currentSortingOrder = 0;
            var uniqueLayerNames = mapData.legend.Select(x => x.layer).Distinct().ToList();

            // ORDER LAYERS
            foreach (string layerName in uniqueLayerNames)
            {
                var (go, tm) = CreateTilemapLayer(gridGo.transform, layerName, currentSortingOrder);
                currentSortingOrder++;

                var ctx = new LayerContext
                {
                    GameObject = go,
                    Tilemap = tm,
                    NeedsPhysics = false
                };
                
                bool layerHasCollision = mapData.legend.Any(x => x.layer == layerName && x.hasCollision);

                if (layerHasCollision)
                {
                    SetupLayerPhysics(go);
                    ctx.NeedsPhysics = true;
                }

                layers.Add(layerName, ctx);
            }
            
            int rows = mapData.layout.Length;

            // BATCHING
            for (int row = 0; row < rows; row++)
            {
                string line = mapData.layout[row];
                for (int col = 0; col < line.Length; col++)
                {
                    string charKey = line[col].ToString();
                    if (!mapLegendItems.TryGetValue(charKey, out MapLegendItem item)) continue;
                    
                    if (!layers.TryGetValue(item.layer, out LayerContext activeLayer)) 
                    {
                        Debug.LogWarning($"Layer '{item.layer}' not defined in legend discovery.");
                        continue;
                    }

                    Vector3Int gridPos = new Vector3Int(col, -row, 0);
                    Tile tile = GetOrCreateTile(relativePath, item);

                    if (tile == null) continue;
                    
                    activeLayer.Positions.Add(gridPos);
                    activeLayer.Tiles.Add(tile);
                }
                
                if (row % 10 == 0) await Task.Yield();
            }

            // RENDERING
            foreach (var kvp in layers)
            {
                LayerContext ctx = kvp.Value;
                if (ctx.Positions.Count > 0)
                {
                    ctx.Tilemap.SetTiles(ctx.Positions.ToArray(), ctx.Tiles.ToArray());
                    Debug.Log($"[MapModule] Layer '{kvp.Key}' populated with {ctx.Positions.Count} tiles.");
                }
            }
            
            LuaEngine.Global.Events.TriggerEvent("TileMapLoaded", mapData.id);
            
            return await PositiveReturn;
        }

        private (GameObject, Tilemap) CreateTilemapLayer(Transform parent, string layerName, int sortingOrder)
        {
            GameObject gameObject = new GameObject(layerName);
            gameObject.transform.SetParent(parent);

            Tilemap tileMap = gameObject.AddComponent<Tilemap>();
            TilemapRenderer tr = gameObject.AddComponent<TilemapRenderer>();

            tr.sortingOrder = sortingOrder;

            return (gameObject, tileMap);
        }

        private void SetupLayerPhysics(GameObject layerGo)
        {
            var tilemapCollider = layerGo.AddComponent<TilemapCollider2D>();
            var composite = layerGo.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            var rb = layerGo.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        private Tile GetOrCreateTile(string relativePath, MapLegendItem item)
        {
            string fullSpritePath = Path.Combine(relativePath, item.spriteFile.TrimStart('/', '\\'));

            if (_tileCache.TryGetValue(fullSpritePath, out Tile cachedTile))
            {
                return cachedTile;
            }

            Sprite sprite = ResourceLoader.LoadSprite(fullSpritePath);

            if (sprite == null) return null;

            Tile newTile = ScriptableObject.CreateInstance<Tile>();
            newTile.sprite = sprite;
            newTile.name = item.symbol;
            
            newTile.colliderType = item.hasCollision ? Tile.ColliderType.Grid : Tile.ColliderType.None;

            _tileCache.Add(fullSpritePath, newTile);
            return newTile;
        }

        private ValueTask<int> ClearMap(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            _tileCache.Clear();

            var maps = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(g => g.name.StartsWith("Map_"));

            foreach (var m in maps) Object.Destroy(m);

            return new ValueTask<int>(1);
        }
    }
}