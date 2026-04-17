using UnityEngine.UIElements;
using UIFramework.Core;
using UIFramework.Events;
using System.Collections.Generic;
using UnityEngine;
using GameplayFramework.Manager;
using GameplayFramework.Data;
using GameplayFramework.Rules;

namespace GameplayFramework.UIFrame
{
    public class CharacterSelectPanel : UIBase
    {
        private VisualElement heroGridContainer;

        // 名字又换回来用 Label 啦！
        private Label uiHeroNameHugeText;

        private VisualElement uiHeroRoleImage;
        private Button btnConfirm;
        private Label uiRuleNameHuge;

        private List<string> charNames = new List<string>();
        private List<VisualElement> allHeroSlots = new List<VisualElement>();
        private int selectedIdx = -1;

        private IVisualElementScheduledItem replaySchedule;

        protected override void BindUIElements()
        {
            heroGridContainer = RootElement.Q<VisualElement>("HeroGridContainer");

            // 抓取新的文字 Label 坑位
            uiHeroNameHugeText = RootElement.Q<Label>("HeroNameHugeText");
            uiHeroRoleImage = RootElement.Q<VisualElement>("HeroRoleImageArea");

            btnConfirm = RootElement.Q<Button>("BtnConfirm");
            uiRuleNameHuge = RootElement.Q<Label>("RuleNameHuge");
        }

        protected override void RegisterEvents()
        {
            UIEventCenter.AddListener<List<string>>("OnReceiveCharList", OnReceiveData);

            if (btnConfirm != null)
            {
                btnConfirm.clicked += () =>
                {
                    if (replaySchedule != null) replaySchedule.Pause();
                    UIEventCenter.Trigger("OnConfirmChar", selectedIdx);
                };
            }
        }

        protected override void UnregisterEvents()
        {
            UIEventCenter.RemoveListener<List<string>>("OnReceiveCharList", OnReceiveData);
            if (replaySchedule != null) replaySchedule.Pause();
        }

        public override void OnShow()
        {
            base.OnShow();
            if (btnConfirm != null) btnConfirm.style.opacity = 0;

            // 重置时清空大字
            if (uiHeroNameHugeText != null) uiHeroNameHugeText.text = "";
            if (uiHeroRoleImage != null) uiHeroRoleImage.style.display = DisplayStyle.None;

            selectedIdx = -1;

            if (uiRuleNameHuge != null)
            {
                var currentRule = Room.RoomManager.Instance?.CurrentRule as GameRuleBase;
                if (currentRule != null)
                {
                    uiRuleNameHuge.text = currentRule.GetRuleName();
                }
                else
                {
                    uiRuleNameHuge.text = "未知行动区";
                }
            }

            UIEventCenter.Trigger("RequestCharList");
        }

        private void OnReceiveData(List<string> names)
        {
            charNames = names;
            if (heroGridContainer != null)
            {
                heroGridContainer.Clear();
                allHeroSlots.Clear();

                for (int i = 0; i < charNames.Count; i++)
                {
                    int idx = i;
                    string heroName = charNames[idx];

                    VisualElement slot = new VisualElement();
                    slot.AddToClassList("hero-slot");

                    VisualElement card = new VisualElement();
                    card.AddToClassList("hero-card");

                    VisualElement portrait = new VisualElement();
                    portrait.AddToClassList("hero-portrait");

                    Texture2D heroTexture = ResourceManager.Instance.Load<Texture2D>($"AgentPicRes/SelectPic/{heroName}");
                    if (heroTexture != null) portrait.style.backgroundImage = new StyleBackground(heroTexture);
                    else
                    {
                        Label debugNoPic = new Label($"[缺头像]\n{heroName}");
                        debugNoPic.style.color = new StyleColor(new Color(1f, 1f, 1f, 0.4f));
                        debugNoPic.style.unityTextAlign = TextAnchor.MiddleCenter;
                        debugNoPic.style.marginTop = 40;
                        portrait.Add(debugNoPic);
                    }
                    card.Add(portrait);
                    slot.Add(card);

                    VisualElement nameArea = new VisualElement();
                    nameArea.AddToClassList("hero-name-outside");

                    Texture2D nameSmallTex = ResourceManager.Instance.Load<Texture2D>($"AgentPicRes/AgentName/{heroName}");
                    if (nameSmallTex != null) nameArea.style.backgroundImage = new StyleBackground(nameSmallTex);
                    else
                    {
                        Label fallbackLabel = new Label(heroName);
                        fallbackLabel.AddToClassList("fallback-text-outside");
                        nameArea.Add(fallbackLabel);
                    }
                    slot.Add(nameArea);

                    slot.RegisterCallback<ClickEvent>(ev => SelectCharacter(idx));

                    heroGridContainer.Add(slot);
                    allHeroSlots.Add(slot);
                }
            }
        }

        private void SelectCharacter(int index)
        {
            if (selectedIdx == index) return;

            selectedIdx = index;
            string heroName = charNames[index];

            foreach (var slot in allHeroSlots) slot.RemoveFromClassList("active-slot");
            allHeroSlots[index].AddToClassList("active-slot");

            // 【核心改回引擎字渲染】：直接贴大字！
            if (uiHeroNameHugeText != null)
            {
                // 如果你想中文加英文字母后缀显得洋气可以拼接，比如($"{heroName} / AGENT")
                uiHeroNameHugeText.text = heroName.ToUpper();
            }

            if (uiHeroRoleImage != null)
            {
                uiHeroRoleImage.style.display = DisplayStyle.Flex;
                AgentProfile profile = ResourceManager.Instance.Load<AgentProfile>($"AgentProfiles/{heroName}");

                if (profile != null)
                {
                    string roleStr = profile.role.ToString();
                    Texture2D roleTex = ResourceManager.Instance.Load<Texture2D>($"AgentPicRes/Role/{roleStr}");
                    if (roleTex != null) uiHeroRoleImage.style.backgroundImage = new StyleBackground(roleTex);
                    else uiHeroRoleImage.style.backgroundImage = null;
                }
            }

            if (btnConfirm != null) btnConfirm.style.opacity = 1;

            UIEventCenter.Trigger("OnPreviewChar", index);

            if (replaySchedule != null) replaySchedule.Pause();
            replaySchedule = RootElement.schedule.Execute(() =>
            {
                UIEventCenter.Trigger("OnReplayPreviewAction");
            }).Every(5000);
        }
    }
}
