using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core.Scripts.Data
{
    public class LayerContext
    {
        public GameObject GameObject;
        public Tilemap Tilemap;
        public bool NeedsPhysics;
        public readonly List<Vector3Int> Positions = new();
        public readonly List<TileBase> Tiles = new();
    }
}