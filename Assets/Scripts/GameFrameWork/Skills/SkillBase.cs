using UnityEngine;
using GameplayFramework.Core;

namespace GameplayFramework.Skills
{
    /// <summary>
    /// 最强大的乐高积木！
    /// 策划以后可以右键 Create 创建无数个技能数据，然后托给技能执行器
    /// </summary>
    public abstract class SkillBase : ScriptableObject
    {
        public string skillName;
        public Sprite skillIcon;
        public float cooldown;
        
        // 当玩家按下按键时，核心触发逻辑！
        public abstract void OnExecute(IActor caster);
    }
}
