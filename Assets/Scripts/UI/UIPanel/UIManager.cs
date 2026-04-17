using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.Core
{
    /// <summary>
    /// UIManager全局单例，负责加载、缓存、层级管理以及调用控制。
    /// 可以轻松拓展 Addressables 或 AssetBundle 资源加载。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Document Root")]
        [Tooltip("场景中统一的UIDocument容器")]
        [SerializeField] private UIDocument rootDocument;
        
        // 缓存已经实例化过的UI面板对象
        private Dictionary<string, UIBase> activePanels = new Dictionary<string, UIBase>();
        
        // 用于管理各层级的VisualElement容器
        private Dictionary<UILayer, VisualElement> layerContainers = new Dictionary<UILayer, VisualElement>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                if (rootDocument == null) rootDocument = GetComponent<UIDocument>();
                InitializeLayers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化UI层级容器，确保严格的显示遮挡关系
        /// </summary>
        private void InitializeLayers()
        {
            var root = rootDocument.rootVisualElement;
            root.style.width = new Length(100, LengthUnit.Percent);
            root.style.height = new Length(100, LengthUnit.Percent);

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerElement = new VisualElement
                {
                    name = $"Layer_{layer}"
                };
                
                // 覆盖全屏且绝对定位
                layerElement.style.position = Position.Absolute;
                layerElement.style.top = 0;
                layerElement.style.bottom = 0;
                layerElement.style.left = 0;
                layerElement.style.right = 0;
                
                // 透明穿透：避免空白层级阻挡后面层的鼠标事件
                layerElement.pickingMode = PickingMode.Ignore;

                root.Add(layerElement);
                layerContainers.Add(layer, layerElement);
            }
        }

        /// <summary>
        /// 打开面板 (高拓展性：支持泛型返回，支持传入层级和回调)
        /// </summary>
        public T OpenPanel<T>(string panelName, UILayer layer = UILayer.Default, Action<T> onOpened = null) where T : UIBase, new()
        {
            // 如果面板已存在并加载过，直接取出并显示
            if (activePanels.TryGetValue(panelName, out UIBase existingPanel))
            {
                existingPanel.OnShow();
                var panel = existingPanel as T;
                onOpened?.Invoke(panel);
                return panel;
            }

            // 加载UXML资源 (这里默认使用Resources加载，你可轻松替换成 Addressables)
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>($"UI/{panelName}");
            if (asset == null)
            {
                Debug.LogError($"[UIManager] Failed to load UI asset from Resources/UI/{panelName}");
                return null;
            }

            // 实例化并设置基础撑满样式
            VisualElement panelRoot = asset.Instantiate();
            panelRoot.style.flexGrow = 1;
            panelRoot.pickingMode = PickingMode.Ignore; // 面板根节点不阻挡，内部的具体UI背景阻挡即可

            // 加入对应的大层级中
            layerContainers[layer].Add(panelRoot);

            // 绑定C#业务逻辑类
            T panelInstance = new T();
            panelInstance.Init(panelRoot, panelName);
            activePanels.Add(panelName, panelInstance);

            panelInstance.OnShow();
            onOpened?.Invoke(panelInstance);

            return panelInstance;
        }

        /// <summary>
        /// 隐藏并关闭面板（缓存机制）
        /// </summary>
        public void ClosePanel(string panelName)
        {
            if (activePanels.TryGetValue(panelName, out UIBase panel))
            {
                panel.OnHide();
            }
        }

        /// <summary>
        /// 强制摧毁并卸载面板，回收内存
        /// </summary>
        public void DestroyPanel(string panelName)
        {
            if (activePanels.TryGetValue(panelName, out UIBase panel))
            {
                panel.OnDestroy();
                panel.RootElement.RemoveFromHierarchy();
                activePanels.Remove(panelName);
            }
        }
    }
}
