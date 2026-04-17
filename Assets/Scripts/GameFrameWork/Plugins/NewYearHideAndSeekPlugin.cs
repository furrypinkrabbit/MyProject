using UnityEngine;
using GameplayFramework.Rules;
using GameplayFramework.Core;

namespace GameplayFramework.Plugins
{
    /// <summary>
    /// 乐高式插件体验：新年躲猫猫特别版！
    /// 核心规则毫无察觉的情况下，增加了特效和特殊机制
    /// </summary>
    public class NewYearHideAndSeekPlugin : IGamePlugin
    {
        public void OnAddedToRule(GameRuleBase rule)
        {
            // 插件安装成功
        }

        public void OnMatchStart()
        {
            Debug.Log("🧧【新年插件】全图撒金币！播放新年喜庆 BGM！");
        }

        public void OnPlayerPossessActor(PlayerController player, IActor newActor)
        {
            // 如果玩家附身在一个物品上，触发特别特效
            if (newActor.ActorTransform.name.Contains("Prop"))
            {
                Debug.Log($"🧧【新年插件】玩家 {player.PlayerId} 变身成功！在 {newActor.ActorTransform.position} 爆炸了一个红色的烟花特效！");
            }
        }
    }
}
