using System.IO;
using UnityEngine;

namespace Game.Core.Scripts.Loaders
{
    public static class ResourceLoader
    {
        public static Sprite LoadSprite(string filePath, float pixelsPerUnit = 16f)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[ResourceLoader] Archivo no encontrado: {filePath}");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); 
            
            texture.filterMode = FilterMode.Point; 
            
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        
        public static string LoadText(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            return File.ReadAllText(filePath);
        }
    }
}