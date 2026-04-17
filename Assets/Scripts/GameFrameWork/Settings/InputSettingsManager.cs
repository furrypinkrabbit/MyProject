using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GameplayFramework.Combat;

namespace GameplayFramework.Settings
{
    [System.Serializable]
    public class ActionBind
    {
        public CombatInputAction action;
        public string keyPath; // 例如 "<Keyboard>/e" 或 "<Mouse>/leftButton"
    }

    [System.Serializable]
    public class AgentSettingProfile
    {
        public string agentId; // "GLOBAL" 代表全局配置，其他如 "Base" 为单英雄覆写
        public float mouseSensitivity;
        public List<ActionBind> overrides = new List<ActionBind>();
    }

    /// <summary>
    /// 全局输入与按键配置大管家（支持真实连接 Unity New Input System 进行热重载）
    /// </summary>
    public class InputSettingsManager : MonoBehaviour
    {
        public static InputSettingsManager Instance { get; private set; }

        public const string TARGET_GLOBAL = "GLOBAL";

        // 基础默认设置
        public Dictionary<CombatInputAction, string> DefaultBinds = new Dictionary<CombatInputAction, string>
        {
            { CombatInputAction.PrimaryFire, "<Mouse>/leftButton" },
            { CombatInputAction.SecondaryFire, "<Mouse>/rightButton" },
            { CombatInputAction.Skill1, "<Keyboard>/e" },
            { CombatInputAction.Skill2, "<Keyboard>/leftShift" },
            { CombatInputAction.Ultimate, "<Keyboard>/q" },
            { CombatInputAction.Reload, "<Keyboard>/r" },
            { CombatInputAction.Melee, "<Keyboard>/f" }
        };

        public float DefaultSensitivity = 0.2f;
        public Dictionary<string, AgentSettingProfile> userProfiles = new Dictionary<string, AgentSettingProfile>();

        // 【完整系统核心】：持有一份全局的 PlayerInput 映射表供实时覆写
        // 假设你在项目中有一个生成的 InputActionAsset 叫 GameInputActions
        public InputActionAsset globalInputAsset;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }

            if (!userProfiles.ContainsKey(TARGET_GLOBAL)) RestoreGlobalToDefaults();

            // 启动时自动应用一次当前英雄/全局的按键绑定
            ApplyAllBindingsToInputSystem(TARGET_GLOBAL);
        }

        public void RestoreGlobalToDefaults()
        {
            var globalProfile = new AgentSettingProfile { agentId = TARGET_GLOBAL, mouseSensitivity = DefaultSensitivity };
            foreach (var kv in DefaultBinds)
            {
                globalProfile.overrides.Add(new ActionBind { action = kv.Key, keyPath = kv.Value });
            }
            userProfiles[TARGET_GLOBAL] = globalProfile;

            ApplyAllBindingsToInputSystem(TARGET_GLOBAL);
        }

        public void ClearAgentOverride(string agentId)
        {
            if (agentId == TARGET_GLOBAL) return;
            if (userProfiles.ContainsKey(agentId)) userProfiles.Remove(agentId);

            // 清除单人后，立刻重新全盘应用全局覆盖，把特殊烙印洗掉
            ApplyAllBindingsToInputSystem(TARGET_GLOBAL);
        }

        public float GetEffectiveSensitivity(string agentId)
        {
            if (userProfiles.ContainsKey(agentId) && userProfiles[agentId].mouseSensitivity > 0f)
                return userProfiles[agentId].mouseSensitivity;
            return userProfiles[TARGET_GLOBAL].mouseSensitivity;
        }

        public string GetEffectiveBinding(string agentId, CombatInputAction action)
        {
            if (userProfiles.ContainsKey(agentId))
            {
                var specificBind = userProfiles[agentId].overrides.Find(x => x.action == action);
                if (specificBind != null && !string.IsNullOrEmpty(specificBind.keyPath)) return specificBind.keyPath;
            }
            var globalBind = userProfiles[TARGET_GLOBAL].overrides.Find(x => x.action == action);
            if (globalBind != null && !string.IsNullOrEmpty(globalBind.keyPath)) return globalBind.keyPath;
            return DefaultBinds[action];
        }

        public void SaveBinding(string agentId, CombatInputAction action, string newKeyPath)
        {
            if (!userProfiles.ContainsKey(agentId))
                userProfiles[agentId] = new AgentSettingProfile { agentId = agentId, mouseSensitivity = 0f };

            var bind = userProfiles[agentId].overrides.Find(x => x.action == action);
            if (bind != null) bind.keyPath = newKeyPath;
            else userProfiles[agentId].overrides.Add(new ActionBind { action = action, keyPath = newKeyPath });

            // 保存后，如果是当前正在使用的特工或是全局，立刻应用到系统底层
            ApplySingleBindingToInputSystem(action, newKeyPath);
        }

        public void SaveSensitivity(string agentId, float newSens)
        {
            if (!userProfiles.ContainsKey(agentId))
                userProfiles[agentId] = new AgentSettingProfile { agentId = agentId };
            userProfiles[agentId].mouseSensitivity = newSens;
        }

        // ================= 真正连接 Unity Input System 的物理执行层 =================

        /// <summary>
        /// 当玩家切换特工，或者重置特工配置时调用，把一套配置暴力打入 Input System
        /// </summary>
        public void ApplyAllBindingsToInputSystem(string targetAgentId)
        {
            if (globalInputAsset == null) return;

            foreach (CombatInputAction act in System.Enum.GetValues(typeof(CombatInputAction)))
            {
                string path = GetEffectiveBinding(targetAgentId, act);
                ApplySingleBindingToInputSystem(act, path);
            }
            Debug.Log($"[Settings] 已将 {targetAgentId} 的整套定制按键刻入底层输入系统。");
        }

        /// <summary>
        /// 把具体的那个按键拔掉，换成新的 path（例如 "<Keyboard>/x"）
        /// </summary>
        private void ApplySingleBindingToInputSystem(CombatInputAction combatAction, string newPath)
        {
            if (globalInputAsset == null) return;

            // 假设你的 Action Map 叫 "Player"，里面的 Action 名字和枚举名同名，比如 "Skill1"
            var action = globalInputAsset.FindAction("Player/" + combatAction.ToString());
            if (action == null) return;

            // 调用 Input System 自带的覆写黑魔法：强制擦除原键，连入新键
            // 1代表覆盖下标0的Composite或第一条Binding
            action.ApplyBindingOverride(0, newPath);
        }
    }
}
