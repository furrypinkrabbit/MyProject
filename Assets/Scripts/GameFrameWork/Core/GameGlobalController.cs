using UnityEngine;
using UIFramework.Events;

namespace GameplayFramework.Core
{
    public class GameGlobalController : MonoBehaviour
    {
        private bool isControlPanelOpen = false;

        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            UIEventCenter.AddListener("ToggleControlPanel", ToggleControlMenu);
        }

        private void Update()
        {
            // 现在按 ESC 第一个拦截拉起的是 ControlPanel (包含返回、设置、退出)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleControlMenu();
            }
        }

        private void ToggleControlMenu()
        {
            isControlPanelOpen = !isControlPanelOpen;

            // 下方如果连带打开着 SettingsPanel，也要强制关掉保持清爽
            if (isControlPanelOpen)
            {
                UIFramework.Core.UIManager.Instance.OpenPanel<GameplayFramework.UIFrame.ControlPanel>("ControlPanel", UIFramework.Core.UILayer.System);
            }
            else
            {
                UIFramework.Core.UIManager.Instance.ClosePanel("SettingsPanel");
                UIFramework.Core.UIManager.Instance.ClosePanel("ControlPanel");
            }
        }

        private void OnDestroy()
        {
            UIEventCenter.RemoveListener("ToggleControlPanel", ToggleControlMenu);
        }
    }
}
