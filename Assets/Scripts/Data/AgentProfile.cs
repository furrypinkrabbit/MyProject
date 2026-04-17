using UnityEngine;

namespace GameplayFramework.Data
{
    /// <summary>
    /// 特工绝密档案：这是一个可以用右键 Create 创建的配置文件
    /// 策划不用动代码，只需在编辑器里勾选职责即可！
    /// </summary>
    [CreateAssetMenu(fileName = "NewAgentProfile", menuName = "Gameplay/Agent Profile", order = 1)]
    public class AgentProfile : ScriptableObject
    {
        [Header("=== 特工核心身份 ===")]
        [Tooltip("必须和角色的预制体名字一模一样，比如 Base")]
        public string agentId;
        
        [Tooltip("该特工的战术职责")]
        public AgentRole role;

        [Header("=== 扩展信息 (备用) ===")]
        [TextArea]
        public string description;
        public int difficultyStar = 1; // 难度星级
    }
}
