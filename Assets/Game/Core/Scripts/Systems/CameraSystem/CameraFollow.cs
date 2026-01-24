using UnityEngine;

namespace Game.Core.Scripts.Systems.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0, 0, -10);
        public float SmoothTime = 0.25f;
        
        private Vector3 _velocity = Vector3.zero;
        private Vector3 _staticTargetPosition;
        private bool _isFollowing = false;

        public void SetTarget(Transform target)
        {
            Target = target;
            _isFollowing = true;
        }

        public void LookAt(Vector3 position)
        {
            Target = null;
            _isFollowing = false;
            _staticTargetPosition = new Vector3(position.x, position.y, Offset.z);
        }

        void LateUpdate()
        {
            Vector3 desiredPosition;

            if (_isFollowing && Target != null)
            {
                desiredPosition = Target.position + Offset;
            }
            else
            {
                if (_isFollowing)
                {
                    _isFollowing = false;
                    _staticTargetPosition = transform.position;
                }
                
                desiredPosition = _staticTargetPosition;
            }
            
            desiredPosition.z = Offset.z;
            
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, SmoothTime);
        }
    }
}
