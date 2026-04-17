using System.Collections.Generic;
using GameplayFramework.Core;
using UnityEngine;

namespace GameplayFramework.Rules
{
    /// <summary>
    /// 具体规则：躲猫猫
    /// </summary>
    public class HideAndSeekRule : GameRuleBase
    {
        public override void StartMatch()
        {
            base.StartMatch();
            Debug.Log("[躲猫猫规则] 比赛开始！寻方闭眼 10 秒钟！");
        }

        /// <summary>
        /// 提供给外部的安全附身接口，规则允许了才能附身
        /// </summary>
        public void ExecutePossess(PlayerController pc, IActor target)
        {
            // 在这里做校验，例如不能附身队友
            pc.Possess(target);
        //    NotifyActorPossessed(pc, target);
        }

        public override List<string> GetModeSpecificUIPanels()
        {
            throw new System.NotImplementedException();
        }

        public override void AssignTeam(PlayerController pc)
        {
            throw new System.NotImplementedException();
        }

        public override string GetRuleName()
        {
            throw new System.NotImplementedException();
        }
    }
}
