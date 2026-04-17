using UIFramework.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.Game
{
    /// <summary>
    /// 【绝对锁定版】登录界面
    /// 只有在玩家用鼠标实打实按下登录按钮后，才会放行切图！
    /// </summary>
    public class LoginPanel : UIBase
    {
        private Button btnLogin;

        protected override void BindUIElements()
        {
            // 注意：请确保你在 UIExtra/UIBuilder 里把按钮的 Name 设为了 BtnLogin
            btnLogin = Get<Button>("BtnLogin");

            // 兼容你截图里的中文或者英文命名
            if (btnLogin == null) btnLogin = RootElement.Q<Button>();

            if (btnLogin == null)
            {
                Debug.LogError("[Login] 没找到登录按钮！请检查UXML里面的按钮名字！");
            }
        }

        protected override void RegisterEvents()
        {
            if (btnLogin != null)
            {
                // 【核心命脉】：只有发生 clicked 事件，才会执行里面这层括号！绝对不能写在外面！
                btnLogin.clicked += OnClickLogin;
            }
        }

        protected override void UnregisterEvents()
        {
            if (btnLogin != null)
            {
                btnLogin.clicked -= OnClickLogin;
            }
        }

        private void OnClickLogin()
        {
            Debug.Log("[Login] 玩家实控点击了按钮！允许放行！开始切图！");

            // 1. 关闭自己，防止把 UI 鬼影带进下一个场景
            UIManager.Instance.ClosePanel("LoginPanel");

            // 2. 呼叫场景大管家，跨关加载靶场 (请确保这里填的是你真正的靶场场景名字)
            SceneDirector.Instance.LoadSceneAsync("FiringRange"); // <--- 修改此处的场景名！
        }
    }
}
