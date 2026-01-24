using System;
using System.Collections.Generic;

namespace Game.Core.Scripts.Data
{
    [Serializable]
    public class MapDefinition
    {
        public string id;
        public float tileSize = 1.0f;
        public List<MapLegendItem> legend;
        public string[] layout;
    }
}