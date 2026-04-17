using UnityEngine;
using UIFramework.Core;
using UIFramework.Constants;
using UIFramework.Example;
using UIFramework.Game;

public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        // 游戏启动后，延迟或直接调用UIManager打开登录界面
        Debug.Log("游戏启动，准备加载登录界面...");
        
        // 此处你可以传回调做进一步处理，在弹出界面的同时绑定事件，非常优雅
        UIManager.Instance.OpenPanel<LoginPanel>(UINameConst.LoginPanel, UILayer.Popup, (panel) => 
        {
            Debug.Log("LoginPanel 成功打开并回调！");
            SceneDirector.Instance.LoadSceneAsync(SceneNameConst.FiringRangeScene);
            // 绑定面板关闭时的后续流程控制
            panel.OnPanelClosed = () => {
                Debug.Log("玩家点击了退出或者登录成功关闭面板，可以写切场景的方法...");

            };
        });
    }
}
