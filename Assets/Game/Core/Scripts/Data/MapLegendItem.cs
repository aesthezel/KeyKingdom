using System;

namespace Game.Core.Scripts.Data
{
    [Serializable]
    public class MapLegendItem
    {
        public string symbol;
        public string spriteFile;
        public string layer;
        public bool hasCollision;
    }
}