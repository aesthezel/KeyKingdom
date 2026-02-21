using System.Collections.Generic;
using System.IO;
using Game.Core.Scripts.Data;
using Game.Core.Scripts.Engine;
using UnityEngine;

namespace Game.Core.Scripts.Loaders
{
    public class ModuleLoader : MonoBehaviour
    {
        public static string ModulesRoot;
        public static readonly string ModulesFolderName = "Modules";
        public static readonly string ScriptsFolderName = "Scripts";
        public static readonly string ModuleDefinitionFileName = "module.json";
        
        public List<ModuleDefinition> loadedMods = new();

        void Start()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            ModulesRoot = Path.Combine(projectRoot, ModulesFolderName);
            ModulesRoot = ModulesRoot.Replace("\\", "/");

            if (!Directory.Exists(ModulesRoot))
                Directory.CreateDirectory(ModulesRoot);
            
            Debug.Log($"[ModuleLoader] Root Path: {ModulesRoot}");

            LoadAllMods();
        }

        void LoadAllMods()
        {
            if (!Directory.Exists(ModulesRoot)) return;

            string[] modDirectories = Directory.GetDirectories(ModulesRoot);

            foreach (string modDir in modDirectories)
            {
                string cleanModDir = modDir.Replace("\\", "/");
                string jsonPath = Path.Combine(cleanModDir, ModuleDefinitionFileName);

                if (!File.Exists(jsonPath)) continue;
                
                string jsonContent = File.ReadAllText(jsonPath);
                
                ModuleDefinition mod = JsonUtility.FromJson<ModuleDefinition>(jsonContent);
                
                mod.rootPath = cleanModDir;

                loadedMods.Add(mod);
                Debug.Log($"[ModuleLoader] Module loaded: {mod.name}");
                    
                if (!string.IsNullOrEmpty(mod.entryScript))
                {
                    string scriptPath = Path.Combine(mod.rootPath, ScriptsFolderName, mod.entryScript);
                    string luaCode = ResourceLoader.LoadText(scriptPath);
                    
                    if (luaCode != null)
                    {
                        // IMPORTANTE: Pasamos la ruta limpia a LUA para evitar problemas con la ubicación del modulo
                        LuaEngine.Global.SetGlobal("CurrentModulePath", mod.rootPath);
                        LuaEngine.Global.ExecuteScript(luaCode);
                    }
                }
            }
        }
    }
}