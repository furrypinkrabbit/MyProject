using System.Collections.Generic;
using UnityEngine;

namespace GameplayFramework.Manager
{
    public class ResourceManager
    {
        private static ResourceManager instance = new ResourceManager();
        public static ResourceManager Instance => instance;

        private Dictionary<string, Object> assetCache = new Dictionary<string, Object>();

        public T Load<T>(string path) where T : Object
        {
            if (assetCache.ContainsKey(path))
            {
                return assetCache[path] as T;
            }

            T asset = Resources.Load<T>(path);

            if (asset != null)
            {
                assetCache.Add(path, asset);
                Debug.Log($"[ResourceManager] 成功动态加载: {path}");
            }
            else
            {
                Debug.LogError($"[ResourceManager] 找不到资源！请检查路径: {path}");
            }

            return asset;
        }

        /// <summary>
        /// 霸气的全量加载：直接把一个文件夹底下的所有同类资源一口气抽干！
        /// 完美适配你的 Agents/Base/Anim 目录结构！
        /// </summary>
        public T[] LoadAll<T>(string folderPath) where T : Object
        {
            // 通过 Resources.LoadAll 直接把文件夹下所有 T 类型的资源全部抓取
            T[] assets = Resources.LoadAll<T>(folderPath);

            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning($"[ResourceManager] 警告！在 {folderPath} 下没有找到任何类型为 {typeof(T).Name} 的资源！");
                return new T[0];
            }

            // 顺手把抓出来的每一个资源都塞进字典缓存，以后单拿也不用读盘了！
            foreach (var asset in assets)
            {
                string fullPath = $"{folderPath}/{asset.name}";
                if (!assetCache.ContainsKey(fullPath))
                {
                    assetCache.Add(fullPath, asset);
                }
            }

            Debug.Log($"[ResourceManager] 霸气全量加载完毕：从 {folderPath} 抽取了 {assets.Length} 个 {typeof(T).Name}");
            return assets;
        }

        public void ClearCache()
        {
            assetCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
