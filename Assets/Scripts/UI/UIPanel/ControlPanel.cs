using UnityEngine.UIElements;
using UIFramework.Core;
using UnityEngine;

namespace GameplayFramework.UIFrame
{
    public class ControlPanel : UIBase
    {
        private Button btnResume;
        private Button btnSettings;
        private Button btnQuit;

        protected override void BindUIElements()
        {
            btnResume = RootElement.Q<Button>("BtnResume");
            btnSettings = RootElement.Q<Button>("BtnSettings");
            btnQuit = RootElement.Q<Button>("BtnQuit");
        }

        protected override void RegisterEvents()
        {
            btnResume.clicked += () => UIFramework.Events.UIEventCenter.Trigger("ToggleControlPanel");

            btnSettings.clicked += () =>
            {
                // 注意这里要传 SettingsPanel 进去
                UIFramework.Core.UIManager.Instance.OpenPanel<SettingsPanel>("SettingsPanel", UIFramework.Core.UILayer.System);
            };

            btnQuit.clicked += () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            };
        }

        public override void OnShow()
        {
            base.OnShow();
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Time.timeScale = 0;
        }

        public override void OnHide()
        {
            base.OnHide();
            // 使用 UIManager 真正的高级查询！
            var settingsPanel = UIFramework.Core.UIManager.Instance.GetPanel("SettingsPanel");
            bool isSettingsOpen = settingsPanel != null && settingsPanel.RootElement != null && settingsPanel.RootElement.style.display == DisplayStyle.Flex;

            if (!isSettingsOpen)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                Time.timeScale = 1;
            }
        }
    }
}
