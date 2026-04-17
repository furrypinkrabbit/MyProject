using UnityEngine;
using GameplayFramework.Core;

namespace GameplayFramework.Actors
{
    public class TargetActor : MonoBehaviour, IActor
    {
        public Transform ActorTransform => transform;
        public void OnPossess(PlayerController controller) { }
        public void OnUnpossess() { }
        public void OnMoveInput(Vector2 direction) { }
        public void OnLookInput(Vector2 delta) { } // 同步接口扩充
        public void OnActionPressed(int actionId) { }
    }
}
