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
            btnClose.clicked += () => 
            {
                UIFramework.Core.UIManager.Instance.ClosePanel("SettingsPanel");
                UIFramework.Core.UIManager.Instance.OpenPanel<ControlPanel>("ControlPanel", UIFramework.Core.UILayer.System);
            };

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
            
            // 【极其重要修复】：解决全屏没撑开的问题！
            // 很多时候外层容器的 Flex 属性被丢失了，导致它变成半截，我们强行让 Panel 本尊 100% 覆盖流！
            RootElement.style.position = Position.Absolute;
            RootElement.style.top = 0;
            RootElement.style.bottom = 0;
            RootElement.style.left = 0;
            RootElement.style.right = 0;

            if (rebindOverlay != null) rebindOverlay.style.display = DisplayStyle.None;
            
            var myPc = Room.RoomManager.Instance?.GetLocalPlayer();
            if (myPc != null && myPc.CurrentActor != null)
                currentEditingAgent = myPc.CurrentActor.gameObject.name.Replace("(Clone)", "").Trim();
            else
                currentEditingAgent = InputSettingsManager.TARGET_GLOBAL;

            BuildLeftHeroList();
            RefreshActionList();
        }

        public override void OnHide()
        {
            base.OnHide();
            InputSettingsManager.Instance.ApplyAllBindingsToInputSystem(currentEditingAgent);
        }

        private void BuildLeftHeroList()
        {
            heroListContainer.Clear();
            heroListContainer.Add(CreateMenuBtn(InputSettingsManager.TARGET_GLOBAL, "ALL HEROES"));

            var rule = Room.RoomManager.Instance?.CurrentRule as GameplayFramework.Rules.GameRuleBase;
            if (rule != null)
            {
                foreach(var prefab in rule.characterPrefabs)
                {
                    heroListContainer.Add(CreateMenuBtn(prefab.name, prefab.name));
                }
            }
        }

        private Button CreateMenuBtn(string agentId, string displayName)
        {
            var btn = new Button();
            btn.text = displayName.ToUpper();
            
            bool isActive = (currentEditingAgent == agentId);
            // OW 风格：选中项极其素净但具有质感，不发光不带框，只有左侧有一条微弱的指示线
            btn.style.backgroundColor = isActive ? new StyleColor(new Color(1f, 1f, 1f, 0.05f)) : new StyleColor(Color.clear);
            btn.style.color = isActive ? new StyleColor(new Color(1f, 1f, 1f)) : new StyleColor(new Color(0.4f, 0.4f, 0.4f));
            
            btn.style.borderLeftWidth = isActive ? 3 : 0;
            btn.style.borderLeftColor = isActive ? new StyleColor(new Color(1f, 0.56f, 0f)) : new StyleColor(Color.clear);
            btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0; btn.style.borderRightWidth = 0;
            
            btn.style.paddingLeft = 40; btn.style.paddingTop = 15; btn.style.paddingBottom = 15;
            btn.style.fontSize = 18; 
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;
            if (isActive) btn.style.unityFontStyleAndWeight = FontStyle.Bold;

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
                "ALL HEROES" : currentEditingAgent.ToUpper();

            btnResetSubConfig.text = currentEditingAgent == InputSettingsManager.TARGET_GLOBAL ? 
                "RESTORE DEFAULTS" : "DELETE OVERRIDE";

            // 如果是被覆写的（比如源氏的特定键），按钮点亮为醒目的删除红以表警告
            if (currentEditingAgent != InputSettingsManager.TARGET_GLOBAL && InputSettingsManager.Instance.userProfiles.ContainsKey(currentEditingAgent))
            {
                btnResetSubConfig.style.color = new StyleColor(new Color(1f, 0.3f, 0.3f));
            }
            else
            {
                btnResetSubConfig.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
            }

            float currentSens = InputSettingsManager.Instance.GetEffectiveSensitivity(currentEditingAgent);
            sensSlider.SetValueWithoutNotify(currentSens);

            var actContainer = RootElement.Q<VisualElement>("ActionBindsContainer");
            actContainer.Clear();

            foreach (CombatInputAction act in System.Enum.GetValues(typeof(CombatInputAction)))
            {
                actContainer.Add(CreateBindRow(act));
            }
        }

        // OW的键值对排版规则：左侧名字极其干净，右侧按键是一个巨大的扁平灰块。修改过的按键会显示淡黄色星号*
        private VisualElement CreateBindRow(CombatInputAction act)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 12; row.style.paddingBottom = 12;
            row.style.paddingLeft = 30; row.style.paddingRight = 30;
            row.style.marginBottom = 5;
            
            // 隔行斑马线风格（列表极简）
            row.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 0.02f));

            var label = new Label(ModifyActionName(act.ToString()));
            label.style.width = 250;
            label.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            label.style.fontSize = 16;
            row.Add(label);

            string currentBindStr = InputSettingsManager.Instance.GetEffectiveBinding(currentEditingAgent, act);
            
            bool isOverride = (currentEditingAgent != InputSettingsManager.TARGET_GLOBAL && 
                InputSettingsManager.Instance.userProfiles.ContainsKey(currentEditingAgent) &&
                InputSettingsManager.Instance.userProfiles[currentEditingAgent].overrides.Exists(x => x.action == act));
            
            var btnRebind = new Button();
            btnRebind.text = FormatKeyPath(currentBindStr) + (isOverride ? " *" : "");
            
            // 按键槽设定为极其方正呆板的设计，这就是经典的电竞感
            btnRebind.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 1f)); 
            btnRebind.style.color = isOverride ? new StyleColor(new Color(1f, 0.8f, 0.2f)) : new StyleColor(Color.white);
            btnRebind.style.borderTopWidth = 1; btnRebind.style.borderBottomWidth = 1; 
            btnRebind.style.borderLeftWidth = 1; btnRebind.style.borderRightWidth = 1;
            
            StyleColor bdColor = new StyleColor(new Color(1f, 1f, 1f, 0.1f));
            btnRebind.style.borderTopColor = bdColor; btnRebind.style.borderBottomColor = bdColor;
            btnRebind.style.borderLeftColor = bdColor; btnRebind.style.borderRightColor = bdColor;

            StyleLength bdRadius = new StyleLength(2); // 只给 2px 圆角，越方越硬核
            btnRebind.style.borderTopLeftRadius = bdRadius; btnRebind.style.borderTopRightRadius = bdRadius;
            btnRebind.style.borderBottomLeftRadius = bdRadius; btnRebind.style.borderBottomRightRadius = bdRadius;

            btnRebind.style.width = 300;
            btnRebind.style.height = 40;
            btnRebind.style.fontSize = 16; 
            btnRebind.style.unityFontStyleAndWeight = FontStyle.Bold;

            btnRebind.clicked += () => StartRebindProcess(act, btnRebind);
            row.Add(btnRebind);
            
            return row;
        }

        private string ModifyActionName(string original)
        {
            if (original == "PrimaryFire") return "Primary Fire";
            if (original == "SecondaryFire") return "Secondary Fire";
            if (original == "Skill1") return "Ability 1";
            if (original == "Skill2") return "Ability 2";
            return original;
        }

        private void StartRebindProcess(CombatInputAction act, Button sourceBtn)
        {
            if (InputSettingsManager.Instance.globalInputAsset == null) return;

            if (rebindOverlay != null) 
            {
                rebindOverlay.style.display = DisplayStyle.Flex;
                rebindPromptText.text = $"AWAITING INPUT FOR [ {ModifyActionName(act.ToString()).ToUpper()} ]\nPress Esc to cancel";
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
            if (string.IsNullOrEmpty(path)) return "UNBOUND";
            var parts = path.Split('/');
            return parts.Length > 1 ? parts[1].ToUpper() : path.ToUpper();
        }
    }
}
