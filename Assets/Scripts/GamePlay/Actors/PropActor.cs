using UnityEngine;
using GameplayFramework.Core;

namespace GameplayFramework.Actors
{
    /// <summary>
    /// 躲猫猫模式中的环境物体（例如一个垃圾桶）
    /// </summary>
    public class PropActor : MonoBehaviour, IActor
    {
        public Transform ActorTransform => transform;

        public void OnPossess(PlayerController controller)
        {
            Debug.Log($"[PropActor] 一个物品被玩家附身了！隐藏血条，不再受重力影响");
        }

        public void OnUnpossess()
        {
            Debug.Log($"[PropActor] 玩家离开了物品");
        }

        public void OnMoveInput(Vector2 direction)
        {
            // 垃圾桶的移动非常慢，只能微调位置
            // transform.Translate(new Vector3(direction.x, 0, direction.y) * 1f * Time.deltaTime);
        }

        public void OnActionPressed(int actionId)
        {
            // 物理的行为与英雄完全不同
            if (actionId == 0) Debug.Log("物品播放了嘲讽音效：来抓我呀！");
            // 垃圾桶没有跳跃，1被忽略
        }

        public void OnLookInput(Vector2 delta)
        {
           // throw new System.NotImplementedException();
        }
    }
}
