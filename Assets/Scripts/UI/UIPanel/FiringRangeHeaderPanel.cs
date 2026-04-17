using UIFramework.Core;
using UnityEngine.UIElements;
using GameplayFramework.Rules;

namespace GameplayFramework.UIFrame
{
    /// <summary>
    /// 左上角模式展示面板：专门读取当前的 GameRule 名字并展示
    /// </summary>
    public class FiringRangeHeaderPanel : UIBase
    {
        private Label ruleNameLabel;
        
        protected override void BindUIElements() 
        { 
            ruleNameLabel = RootElement.Q<Label>("RuleNameLabel");
        }

        public override void OnShow()
        {
            base.OnShow();
            
            // 从当前的房管处获取规则名字
            var rule = Room.RoomManager.Instance?.CurrentRule as GameRuleBase;
            if (rule != null && ruleNameLabel != null)
            {
                ruleNameLabel.text = rule.GetRuleName();
            }
        }
    }
}
