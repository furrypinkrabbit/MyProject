using UnityEngine;
using UIFramework.Events;

namespace GameplayFramework.Core
{
    /// <summary>
    /// 全局系统管家：挂在一个常驻 GameObject 上，由于本框架解耦，它负责随时通过 ESC 唤起设置面板！
    /// </summary>
    public class GameGlobalController : MonoBehaviour
    {
        private bool isSettingsOpen = false;

        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            UIEventCenter.AddListener("ToggleSettingsPanel", ToggleSettings);
        }

        private void Update()
        {
            // 按下 ESC 强制打开/关闭控制中心
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleSettings();
            }
        }

        private void ToggleSettings()
        {
            isSettingsOpen = !isSettingsOpen;
            if (isSettingsOpen)
            {
                UIFramework.Core.UIManager.Instance.OpenPanel<GameplayFramework.UIFrame.SettingsPanel>("SettingsPanel", UIFramework.Core.UILayer.System);
            }
            else
            {
                UIFramework.Core.UIManager.Instance.ClosePanel("SettingsPanel");
            }
        }

        private void OnDestroy()
        {
            UIEventCenter.RemoveListener("ToggleSettingsPanel", ToggleSettings);
        }
    }
}
