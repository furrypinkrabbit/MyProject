using System.Collections.Generic;
using UnityEngine;

namespace GameplayFramework.Rules
{
    public abstract class GameRuleBase : MonoBehaviour
    {
        public List<GameObject> characterPrefabs = new List<GameObject>();

        // 【新增】：获取当前模式的中文名称，用于显示在左上角
        public abstract string GetRuleName();

        public virtual void PreMatchSetup() { }

        public abstract List<string> GetModeSpecificUIPanels();
        public abstract void AssignTeam(Core.PlayerController pc);
        
        public virtual void ExecutePossess(Core.PlayerController pc, Core.IActor target)
        {
            pc.Possess(target);
        }
        
        public virtual void StartMatch() { }
    }
}
