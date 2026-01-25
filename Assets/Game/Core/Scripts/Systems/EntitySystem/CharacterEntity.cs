using UnityEngine;

namespace Game.Core.Scripts.Systems.EntitySystem
{
    public class CharacterEntity : GameEntity
    {
        public float MoveSpeed = 5f;
        public int Health = 100;

        public void Move(Vector2 direction)
        {
            transform.Translate(direction * MoveSpeed * Time.deltaTime);
        }

        public virtual void TakeDamage(int amount)
        {
            if (LuaEngine.Global != null)
            {
                LuaEngine.Global.Events.TriggerEvent("EntityDamaged", Name, amount);
            }

            if (Health > 0) return;
            
            if (CompareTag("Player"))
            {
                LuaEngine.Global.Events.TriggerEvent("GameOver");
            }
        }
    }
}