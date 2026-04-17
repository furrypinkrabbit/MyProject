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
            // 点击返回战局，关闭自己
            btnResume.clicked += () => 
            {
                UIFramework.Core.UIManager.Instance.ClosePanel("ControlPanel");
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                Time.timeScale = 1;
            };

            // 点击设置：【极其重要】必须先关掉自己，再打开Settings，绝对不能挤在一起！
            btnSettings.clicked += () => 
            {
                UIFramework.Core.UIManager.Instance.ClosePanel("ControlPanel");
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
        }
    }
}
