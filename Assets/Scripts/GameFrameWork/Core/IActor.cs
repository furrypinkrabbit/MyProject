using UnityEngine;

namespace GameplayFramework.Core
{
    public interface IActor
    {
        Transform ActorTransform { get; }

        void OnPossess(PlayerController controller);
        void OnUnpossess();

        void OnMoveInput(Vector2 direction);

        // 【新增】：接受鼠标/右摇杆的视角滑动输入
        void OnLookInput(Vector2 delta);

        void OnActionPressed(int actionId);
    }
}
