using System;

namespace Game.Core.Scripts.Data
{
    [Serializable]
    public class ModuleDefinition
    {
        public string id;
        public string name;
        public string version;
        public string description;  
        public string entryScript;
        
        [NonSerialized] public string rootPath;
    }
}