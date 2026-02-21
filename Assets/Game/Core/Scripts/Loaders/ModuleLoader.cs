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
            // 1. Obtener la ruta raíz del proyecto (padre de 'Assets' o 'Game_Data')
            // Esto elimina la necesidad de usar "../" manual
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            
            // 2. Combinar limpiamente
            ModulesRoot = Path.Combine(projectRoot, ModulesFolderName);
            
            // 3. Normalizar separadores (opcional pero recomendado para consistencia en logs)
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
                // Limpieza de ruta del mod específico
                string cleanModDir = modDir.Replace("\\", "/");
                
                string jsonPath = Path.Combine(cleanModDir, ModuleDefinitionFileName);

                if (!File.Exists(jsonPath)) continue;
                
                string jsonContent = File.ReadAllText(jsonPath);
                
                // Asumimos que tienes la clase ModuleDefinition definida
                ModuleDefinition mod = JsonUtility.FromJson<ModuleDefinition>(jsonContent);
                
                // Guardamos la ruta limpia en el objeto del mod
                mod.rootPath = cleanModDir;

                loadedMods.Add(mod);
                Debug.Log($"[ModuleLoader] Module loaded: {mod.name}");
                    
                if (!string.IsNullOrEmpty(mod.entryScript))
                {
                    // Path.Combine acepta multiples argumentos, es mas seguro que concatenar strings
                    string scriptPath = Path.Combine(mod.rootPath, ScriptsFolderName, mod.entryScript);
                    
                    // Cargar el texto del script
                    string luaCode = ResourceLoader.LoadText(scriptPath);
                    
                    if (luaCode != null)
                    {
                        // IMPORTANTE: Pasamos la ruta limpia a LUA para evitar problemas de escape
                        LuaEngine.Global.SetGlobal("CurrentModulePath", mod.rootPath);
                        LuaEngine.Global.ExecuteScript(luaCode);
                    }
                }
            }
        }
    }
}