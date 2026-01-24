using UnityEngine;
using Lua;
using System.Threading.Tasks;
using Game.Core.Scripts.LuaObjects;
using Game.Core.Scripts.Systems.CameraSystem;

namespace Game.Core.Scripts.LuaModules
{
    public class CameraModule : BaseLuaModule
    {
        public override string ModuleName => "camera";
        
        private CameraFollow _cameraController;

        protected override void RegisterFunctions(LuaTable table)
        {
            Bind(table, "follow", FollowTarget);
            Bind(table, "look_at", LookAtPosition);
            Bind(table, "set_smooth", SetSmooth);
        }
        
        private void EnsureCamera()
        {
            if (_cameraController != null) return;

            var mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[CameraModule] No Main Camera found via Camera.main!");
                return;
            }
            
            if (!mainCam.TryGetComponent(out _cameraController))
            {
                _cameraController = mainCam.gameObject.AddComponent<CameraFollow>();
            }
        }
        
        private ValueTask<int> FollowTarget(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            EnsureCamera();
            
            var entityWrapper = context.GetArgument<EntityBaseLua>(0);

            if (entityWrapper != null && entityWrapper.GameObject != null)
            {
                _cameraController.SetTarget(entityWrapper.GameObject.transform);
            }
            else
            {
                Debug.LogWarning("[Camera] Entity is nil or destroyed.");
                return NegativeReturn;
            }

            return PositiveReturn;
        }
        
        private ValueTask<int> LookAtPosition(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            EnsureCamera();
            
            float x = context.GetArgument<float>(0);
            float y = context.GetArgument<float>(1);

            _cameraController.LookAt(new Vector3(x, y, 0));

            return PositiveReturn;
        }
        
        private ValueTask<int> SetSmooth(LuaFunctionExecutionContext context, System.Threading.CancellationToken ct)
        {
            EnsureCamera();
            
            float val = context.GetArgument<float>(0);
            _cameraController.SmoothTime = val;
            
            return PositiveReturn;
        }
    }
}