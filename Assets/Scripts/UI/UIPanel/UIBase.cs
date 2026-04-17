using System;
using UnityEngine.UIElements;

namespace UIFramework.Core
{
    /// <summary>
    /// UI基类，负责处理单个UI面板的生命周期、元素绑定和事件注册
    /// 因为UI Toolkit提倡逻辑与显示分离，这里不继承MonoBehaviour，性能更高
    /// </summary>
    public abstract class UIBase
    {
        public VisualElement RootElement { get; private set; }
        public string PanelName { get; private set; }

        public Action OnPanelClosed; // 面板关闭时的回调支持

        /// <summary>
        /// 框架内部调用的初始化方法
        /// </summary>
        public void Init(VisualElement root, string name)
        {
            RootElement = root;
            PanelName = name;
            
            OnAwake();
            BindUIElements();
            RegisterEvents();
        }

        /// <summary>
        /// 1. 生命周期 - 创建时调用一次
        /// </summary>
        protected virtual void OnAwake() { }
        
        /// <summary>
        /// 2. 生命周期 - 显示时调用
        /// </summary>
        public virtual void OnShow() 
        {
            RootElement.style.display = DisplayStyle.Flex;
        }
        
        /// <summary>
        /// 3. 生命周期 - 隐藏时调用
        /// </summary>
        public virtual void OnHide() 
        {
            RootElement.style.display = DisplayStyle.None;
            OnPanelClosed?.Invoke();
        }
        
        /// <summary>
        /// 4. 生命周期 - 销毁时调用
        /// </summary>
        public virtual void OnDestroy() 
        {
            UnregisterEvents();
        }

        /// <summary>
        /// 绑定UI元素，子类在此处通过 Get<T> 缓存控件引用
        /// </summary>
        protected abstract void BindUIElements();
        
        /// <summary>
        /// 注册UI交互事件（按钮点击、滑动等）
        /// </summary>
        protected virtual void RegisterEvents() { }
        
        /// <summary>
        /// 注销事件，防止内存泄漏
        /// </summary>
        protected virtual void UnregisterEvents() { }

        /// <summary>
        /// 工具方法：快速获取视觉元素
        /// </summary>
        protected T Get<T>(string name) where T : VisualElement
        {
            var element = RootElement.Q<T>(name);
            if (element == null) UnityEngine.Debug.LogWarning($"[{PanelName}] Cannot find an element with name: {name}");
            return element;
        }
    }
}
