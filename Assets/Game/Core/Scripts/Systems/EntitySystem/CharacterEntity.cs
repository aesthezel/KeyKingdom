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

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0) Kill();
        }
    }
}