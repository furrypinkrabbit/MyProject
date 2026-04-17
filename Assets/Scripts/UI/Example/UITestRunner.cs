using UnityEngine;
using UIFramework.Core;
using UIFramework.Constants;
using UIFramework.Game;

namespace UIFramework.Example
{
    /// <summary>
    /// 只需在场景里的管理者挂载此类，即可体验如何调用这套框架
    /// </summary>
    public class UITestRunner : MonoBehaviour
    {
        private void Start()
        {
            // 极其简单的全链路调用方式：
            // 打开面板 -> 绑定回调监听 -> 设置特定初始动作
            UIManager.Instance.OpenPanel<LoginPanel>(UINameConst.LoginPanel, UILayer.Popup, (panel) => 
            {
                Debug.Log("Login 面板成功开启啦");
                
                // 我们之前封装的基于底层委托的面版关闭监听
                panel.OnPanelClosed = () => {
                    Debug.Log("Login 面板已经关闭，可以在这写接下来的流程...");
                };
            });
        }

        private void Update()
        {
            // 运行时按空格键测试销毁功能
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // UI层面上彻底销毁对象
                UIManager.Instance.DestroyPanel(UINameConst.LoginPanel);
            }
        }
    }
}
