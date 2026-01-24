using UnityEngine;

namespace Game.Core.Scripts.Systems.EntitySystem
{
    public class GameEntity : MonoBehaviour
    {
        public string ID { get; set; }
        public string Name { get; set; }
        
        public virtual void Initialize() { }

        public void SetPosition(float x, float y)
        {
            transform.position = new Vector3(x, y, 0);
        }

        public virtual void Kill()
        {
            Destroy(gameObject);
        }
    }
}