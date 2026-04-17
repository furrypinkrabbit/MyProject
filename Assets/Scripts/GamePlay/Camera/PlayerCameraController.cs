using UnityEngine;
// 假设你项目中安装了 Cinemachine
// using Cinemachine;

namespace GameplayFramework.CameraSys
{
    public enum CameraPerspective { FirstPerson, ThirdPerson, FreeCam }

    /// <summary>
    /// 玩家的主相机管理器。它跟随在 PlayerController 身边，由 Controller 驱动。
    /// 包含不同视角的 Cinemachine Virtual Camera 引用。
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        // 伪代码：在真实项目中，这里拖入你的 Cinemachine Virtual Cameras
        // public CinemachineVirtualCamera firstPersonCam;
        // public CinemachineVirtualCamera thirdPersonCam;
        // public CinemachineVirtualCamera freeCam;

        public void BindToActor(Transform actorTransform)
        {
            // 当附身发生时，把所有虚拟相机的 Follow 和 LookAt 目标全指过去
            // firstPersonCam.Follow = actorTransform;
            // thirdPersonCam.Follow = actorTransform;
            Debug.Log($"[Camera] 摄像机焦点已绑定至肉体: {actorTransform.name}");
        }

        public void SetPerspective(CameraPerspective perspective)
        {
            // 通过调整 Cinemachine 相机的 Priority 来无缝切换视角
            // firstPersonCam.Priority = (perspective == CameraPerspective.FirstPerson) ? 10 : 0;
            // thirdPersonCam.Priority = (perspective == CameraPerspective.ThirdPerson) ? 10 : 0;
            // freeCam.Priority = (perspective == CameraPerspective.FreeCam) ? 10 : 0;
            
            Debug.Log($"[Camera] 视角已切换至: {perspective}");
        }
    }
}
