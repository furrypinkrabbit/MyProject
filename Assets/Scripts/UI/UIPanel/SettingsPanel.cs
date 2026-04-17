using UnityEngine.UIElements;
using UIFramework.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using GameplayFramework.Settings;
using GameplayFramework.Combat;
using System.Collections.Generic;

namespace GameplayFramework.UIFrame
{
    public class SettingsPanel : UIBase
    {
        private VisualElement heroListContainer;
        private Label currentSettingTargetLabel;
        private Slider sensSlider;
        private Button btnClose;
        private Button btnResetSubConfig;

        private string currentEditingAgent = InputSettingsManager.TARGET_GLOBAL;
        private VisualElement rebindOverlay;
        private Label rebindPromptText;
        private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

        protected override void BindUIElements()
        {
            heroListContainer = RootElement.Q<VisualElement>("HeroListContainer");
            currentSettingTargetLabel = RootElement.Q<Label>("CurrentTargetLabel");
            sensSlider = RootElement.Q<Slider>("SensSlider");
            btnClose = RootElement.Q<Button>("BtnClose");
            btnResetSubConfig = RootElement.Q<Button>("BtnResetConfig");

            rebindOverlay = RootElement.Q<VisualElement>("RebindOverlay");
            rebindPromptText = RootElement.Q<Label>("RebindPromptText");
        }

        protected override void RegisterEvents()
        {
            btnClose.clicked += () => UIFramework.Events.UIEventCenter.Trigger("ToggleSettingsPanel");

            btnResetSubConfig.clicked += () =>
            {
                if (currentEditingAgent == InputSettingsManager.TARGET_GLOBAL)
                    InputSettingsManager.Instance.RestoreGlobalToDefaults();
                else
                    InputSettingsManager.Instance.ClearAgentOverride(currentEditingAgent);

                RefreshActionList();
            };

            sensSlider.RegisterValueChangedCallback(evt =>
            {
                InputSettingsManager.Instance.SaveSensitivity(currentEditingAgent, evt.newValue);
            });
        }

        public override void OnShow()
        {
            base.OnShow();
            if (rebindOverlay != null) rebindOverlay.style.display = DisplayStyle.None;

            var myPc = Room.RoomManager.Instance?.GetLocalPlayer();
            if (myPc != null && myPc.CurrentActor != null)
                currentEditingAgent = myPc.CurrentActor.gameObject.name.Replace("(Clone)", "").Trim();
            else
                currentEditingAgent = InputSettingsManager.TARGET_GLOBAL;

            BuildLeftHeroList();
            RefreshActionList();

            // 修复引用歧义 (CS0104)：明确告诉编译器，这里的 Cursor 是管物理鼠标大箭头的，不是 UI Toolkit 的光标样式！
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Time.timeScale = 0;
        }

        public override void OnHide()
        {
            base.OnHide();
            // 修复引用歧义 (CS0104)
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            Time.timeScale = 1;

            InputSettingsManager.Instance.ApplyAllBindingsToInputSystem(currentEditingAgent);
        }

        private void BuildLeftHeroList()
        {
            heroListContainer.Clear();
            heroListContainer.Add(CreateMenuBtn(InputSettingsManager.TARGET_GLOBAL, "所有英雄 (全局)"));

            var rule = Room.RoomManager.Instance?.CurrentRule as GameplayFramework.Rules.GameRuleBase;
            if (rule != null)
            {
                foreach (var prefab in rule.characterPrefabs)
                {
                    heroListContainer.Add(CreateMenuBtn(prefab.name, prefab.name));
                }
            }
        }

        private Button CreateMenuBtn(string agentId, string displayName)
        {
            var btn = new Button();
            btn.text = displayName;
            btn.AddToClassList("side-menu-btn");
            if (currentEditingAgent == agentId) btn.AddToClassList("active-menu");

            btn.clicked += () =>
            {
                if (rebindingOperation != null) return;
                currentEditingAgent = agentId;
                BuildLeftHeroList();
                RefreshActionList();
            };
            return btn;
        }

        private void RefreshActionList()
        {
            currentSettingTargetLabel.text = currentEditingAgent == InputSettingsManager.TARGET_GLOBAL ?
                "正在编辑：全战区配置" : $"正在为您单独定制：【{currentEditingAgent}】";

            btnResetSubConfig.text = currentEditingAgent == InputSettingsManager.TARGET_GLOBAL ?
                "恢复全服出厂设置" : "删除单独覆写，沿用全局配置";

            float currentSens = InputSettingsManager.Instance.GetEffectiveSensitivity(currentEditingAgent);
            sensSlider.SetValueWithoutNotify(currentSens);

            var actContainer = RootElement.Q<VisualElement>("ActionBindsContainer");
            actContainer.Clear();

            foreach (CombatInputAction act in System.Enum.GetValues(typeof(CombatInputAction)))
            {
                actContainer.Add(CreateBindRow(act));
            }
        }

        private VisualElement CreateBindRow(CombatInputAction act)
        {
            var row = new VisualElement();
            row.AddToClassList("bind-row");

            var label = new Label(act.ToString());
            label.style.width = 200;
            label.style.color = Color.white;
            row.Add(label);

            string currentBindStr = InputSettingsManager.Instance.GetEffectiveBinding(currentEditingAgent, act);
            var btnRebind = new Button();
            btnRebind.text = FormatKeyPath(currentBindStr);
            btnRebind.AddToClassList("rebind-btn");

            if (currentEditingAgent != InputSettingsManager.TARGET_GLOBAL &&
                InputSettingsManager.Instance.userProfiles.ContainsKey(currentEditingAgent) &&
                InputSettingsManager.Instance.userProfiles[currentEditingAgent].overrides.Exists(x => x.action == act))
            {
                btnRebind.style.backgroundColor = new StyleColor(new Color(1f, 0.5f, 0f, 0.5f));
            }

            btnRebind.clicked += () => StartRebindProcess(act, btnRebind);

            row.Add(btnRebind);
            return row;
        }

        private void StartRebindProcess(CombatInputAction act, Button sourceBtn)
        {
            if (InputSettingsManager.Instance.globalInputAsset == null)
            {
                Debug.LogError("没有挂载 InputActionAsset，无法执行实体改键！");
                return;
            }

            if (rebindOverlay != null)
            {
                rebindOverlay.style.display = DisplayStyle.Flex;
                rebindPromptText.text = $"正在为 [{act}] 倾听新的指令\n请按下键盘上您想要的键...";
            }

            var actionToRebind = InputSettingsManager.Instance.globalInputAsset.FindAction("Player/" + act.ToString());
            if (actionToRebind == null) return;

            actionToRebind.Disable();

            rebindingOperation = actionToRebind.PerformInteractiveRebinding(0)
                .WithControlsExcluding("Mouse/position")
                .WithControlsExcluding("Mouse/delta")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    string newPath = operation.action.bindings[0].effectivePath;
                    actionToRebind.Enable();
                    operation.Dispose();
                    rebindingOperation = null;

                    if (rebindOverlay != null) rebindOverlay.style.display = DisplayStyle.None;

                    InputSettingsManager.Instance.SaveBinding(currentEditingAgent, act, newPath);
                    RefreshActionList();
                })
                .OnCancel(operation =>
                {
                    actionToRebind.Enable();
                    operation.Dispose();
                    rebindingOperation = null;
                    if (rebindOverlay != null) rebindOverlay.style.display = DisplayStyle.None;
                })
                .Start();
        }

        private string FormatKeyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "未绑定";
            var parts = path.Split('/');
            return parts.Length > 1 ? parts[1].ToUpper() : path.ToUpper();
        }
    }
}
