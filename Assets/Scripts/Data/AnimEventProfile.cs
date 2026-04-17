using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayFramework.Data
{
    [Serializable]
    public struct ActionEventMapping
    {
        [Tooltip("触发事件的名称，比如 'PrimaryFire', 'Melee', 'Show'")]
        public string eventName; 
        [Tooltip("对应必须播放的纯净肉体动画")]
        public AnimationClip clip;
    }

    /// <summary>
    /// 【极高自由度】：把状态连线变成事件槽位！
    /// 策划随便拼凑来自十几个不同资源包的动画，只要填在这个表里即可！
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimProfile", menuName = "Gameplay/Anim Event Profile", order = 1)]
    public class AnimEventProfile : ScriptableObject
    {
        [Header("=== 基础腿部力学（常驻）===")]
        public AnimationClip idleClip;
        public AnimationClip walkForwardClip;
        public AnimationClip walkBackwardClip;

        [Header("=== 高级上层动作（事件触发）===")]
        public List<ActionEventMapping> customEvents = new List<ActionEventMapping>();

        public AnimationClip GetClipByEvent(string eventName)
        {
            var match = customEvents.Find(x => x.eventName.Equals(eventName, StringComparison.OrdinalIgnoreCase));
            return match.clip;
        }
    }
}
