using UnityEngine;
using UIFramework.Core;
using UIFramework.Game;
using UIFramework.Constants;

namespace UIFramework.Game
{
    /// <summary>
    /// 英雄展示场景的启动入口
    /// </summary>
    public class HeroGalleryStarter : MonoBehaviour
    {
        private void Start()
        {
            // 如果UIManager在之前的场景未被销毁并带过来了，这里直接调用
            if (UIManager.Instance != null)
            {
                Debug.Log("加载GalleryUI");

                UIManager.Instance.OpenPanel<HeroGalleryPanel>(UINameConst.HeroGallery, UILayer.Default);
            }
            else
            {
                Debug.LogError("没有找到UIManager单例！请确保游戏从LoginScene启动，或者在本场景挂载一个");
            }
        }
    }
}
