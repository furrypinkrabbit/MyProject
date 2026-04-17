namespace GameplayFramework.Rules
{
    /// <summary>
    /// 玩法插件接口：所有的特殊节日活动、变异变种玩法，均通过实现此接口来干预基础规则
    /// </summary>
    public interface IGamePlugin
    {
        void OnAddedToRule(GameRuleBase rule);
        void OnMatchStart();
        
        // 监控或劫持角色附身事件
        void OnPlayerPossessActor(Core.PlayerController player, Core.IActor newActor);
        
        // 可拓展：OnPlayerDeath, OnItemPickedUp 等...
    }
}
