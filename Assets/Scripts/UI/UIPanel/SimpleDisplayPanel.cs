using UIFramework.Core;

namespace GameplayFramework.UIFrame
{
    /// <summary>
    /// 用于只展示画面、不需要后台绑定特殊交互逻辑的界面（如纯粹的准星面板）
    /// 用来解决 UIBase 是抽象类无法被泛型 T 实例化的安全垫。
    /// </summary>
    public class SimpleDisplayPanel : UIBase
    {
        protected override void BindUIElements() { }
    }
}
