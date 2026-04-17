namespace UIFramework.Core
{
    /// <summary>
    /// UI层级定义，确保弹窗能够遮盖普通面板，背景在最下层
    /// </summary>
    public enum UILayer 
    {
        Background = 0,     // 背景层（如主城背景UI）
        Default = 1,        // 普通界面层（如背包、角色面板）
        Popup = 2,          // 弹出层（如二次确认框）
        Top = 3,            // 顶层（如跑马灯，全屏特效）
        System = 4          // 系统层（如加载界面，断层重连，绝不能被遮挡）
    }
}
