using UIFramework.Core;
using UIFramework.Events;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace UIFramework.Game
{
    public class HeroGalleryPanel : UIBase
    {
        private ScrollView actionScrollView;
        private Label txtCurrentAction;
        private Button btnBack;
        
        private List<Button> allActionButtons = new List<Button>();
        private List<string> availableActions = new List<string>(); // 不再写死，靠事件动态获取

        protected override void BindUIElements()
        {
            actionScrollView = Get<ScrollView>("ActionScrollView");
            txtCurrentAction = Get<Label>("TxtCurrentAction");
            btnBack = Get<Button>("BtnBack");
        }

        protected override void RegisterEvents()
        {
            btnBack.clicked += () => 
            {
                SceneDirector.Instance.LoadSceneAsync("LoginScene");
                UIManager.Instance.ClosePanel("HeroGalleryPanel");
            };

            // 🌟监听 3D 场景发来的【动作列表数据】
            UIEventCenter.AddListener<List<string>>("OnReceiveAnimationList", OnReceiveAnimationList);
        }

        protected override void UnregisterEvents()
        {
            btnBack.clicked -= null; // 简单清除
            UIEventCenter.RemoveListener<List<string>>("OnReceiveAnimationList", OnReceiveAnimationList);
        }

        public override void OnShow()
        {
            base.OnShow();
            
            // 🌟界面显示时，主动向 3D 场景要数据（全服广播喊话）
            UIEventCenter.Trigger("OnRequestAnimationList");
        }

        private void OnReceiveAnimationList(List<string> actions)
        {
            availableActions = actions;
            GenerateActionList();
            
            if (availableActions.Count > 0)
                SelectAction(availableActions[0]);
        }

        private void GenerateActionList()
        {
            actionScrollView.Clear();
            allActionButtons.Clear();

            foreach (string actionName in availableActions)
            {
                Button btn = new Button();
                btn.text = actionName.ToUpper();
                btn.AddToClassList("action-btn");
                
                string currentName = actionName; 
                btn.clicked += () => SelectAction(currentName);
                
                actionScrollView.Add(btn);
                allActionButtons.Add(btn);
                btn.userData = currentName; 
            }
        }

        private void SelectAction(string actionName)
        {
            txtCurrentAction.text = actionName.ToUpper();
            foreach (var btn in allActionButtons)
            {
                if ((string)btn.userData == actionName)
                    btn.AddToClassList("active");
                else
                    btn.RemoveFromClassList("active");
            }
            UIEventCenter.Trigger("OnPlayHeroAction", actionName);
        }
    }
}
