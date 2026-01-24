using UnityEngine;

namespace Game.Core.Scripts.Systems.EntitySystem
{
    public class ItemEntity : GameEntity
    {
        public bool IsConsumable = true;
        
        public void Interact(CharacterEntity user)
        {
            Debug.Log($"[Item] {Name} used by {user.Name}");
            if (IsConsumable) Kill();
        }
    }
}