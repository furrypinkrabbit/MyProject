using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.Core
{
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Document Root")]
        [SerializeField] private UIDocument rootDocument;

        private Dictionary<string, UIBase> activePanels = new Dictionary<string, UIBase>();
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

                layerElement.style.position = Position.Absolute;
                layerElement.style.top = 0;
                layerElement.style.bottom = 0;
                layerElement.style.left = 0;
                layerElement.style.right = 0;
                layerElement.pickingMode = PickingMode.Ignore;

                root.Add(layerElement);
                layerContainers.Add(layer, layerElement);
            }
        }

        public T OpenPanel<T>(string panelName, UILayer layer = UILayer.Default, Action<T> onOpened = null) where T : UIBase, new()
        {
            if (activePanels.TryGetValue(panelName, out UIBase existingPanel))
            {
                existingPanel.OnShow();
                var panel = existingPanel as T;
                onOpened?.Invoke(panel);
                return panel;
            }

            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>($"UI/{panelName}");
            if (asset == null)
            {
                Debug.LogError($"[UIManager] Failed to load UI asset from Resources/UI/{panelName}");
                return null;
            }

            VisualElement panelRoot = asset.Instantiate();
            panelRoot.style.flexGrow = 1;
            panelRoot.pickingMode = PickingMode.Ignore;

            layerContainers[layer].Add(panelRoot);

            T panelInstance = new T();
            panelInstance.Init(panelRoot, panelName);
            activePanels.Add(panelName, panelInstance);

            panelInstance.OnShow();
            onOpened?.Invoke(panelInstance);

            return panelInstance;
        }

        // ============ 新增的高级查询管线 ============
        // 专门修复 ControlPanel 等顶层系统想要监控某个子面板状态的问题！
        public UIBase GetPanel(string panelName)
        {
            if (activePanels.TryGetValue(panelName, out UIBase panel))
            {
                return panel;
            }
            return null;
        }

        public void ClosePanel(string panelName)
        {
            if (activePanels.TryGetValue(panelName, out UIBase panel))
            {
                panel.OnHide();
            }
        }

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
