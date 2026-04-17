using System.Collections.Generic;
using UnityEngine;

namespace GameplayFramework.Manager
{
    /// <summary>
    /// 对象池管理器：专门用来回收和分发肉体、子弹、开枪火焰、粒子特效。
    /// 坚决抵制 Destroy 和 Instantiate 的连环滥用！
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        // 多重对象池：键名是路径，值是那个路径下所有被回收在休息的物体排成的队列
        private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();
        
        // 所有在睡觉的特效都会被统一塞进这个垃圾桶底下关禁闭，保持Hierarchy干净
        private Transform poolRoot; 

        private void Awake()
        {
            Instance = this;
            poolRoot = new GameObject("--- Object Pool Root ---").transform;
            poolRoot.SetParent(this.transform);
        }

        /// <summary>
        /// 预热资源池（比如英雄刚出生，提前把他的专属大招特效拉5个进池子备用）
        /// </summary>
        public void Prewarm(string prefabPath, int count)
        {
            if (!poolDict.ContainsKey(prefabPath))
                poolDict[prefabPath] = new Queue<GameObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject obj = CreateNewObj(prefabPath);
                Recycle(obj); // 刚造出来就弄晕扔进池子
            }
            Debug.Log($"[PoolManager] 预热完毕！造了 {count} 个 {prefabPath} 在池子里睡大觉。");
        }

        /// <summary>
        /// 从池子里捞东西，如果没有，就让 ResourceManager 临时造
        /// </summary>
        public GameObject Spawn(string prefabPath, Vector3 position, Quaternion rotation)
        {
            if (poolDict.ContainsKey(prefabPath) && poolDict[prefabPath].Count > 0)
            {
                GameObject obj = poolDict[prefabPath].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true); // 唤醒
                return obj;
            }
            else
            {
                // 池子里吸干了！临时造一个！
                GameObject obj = CreateNewObj(prefabPath);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj;
            }
        }

        /// <summary>
        /// 用完了，打扫干净扔回对应的池子里
        /// </summary>
        public void Recycle(GameObject obj)
        {
            PoolItem itemInfo = obj.GetComponent<PoolItem>();
            if (itemInfo == null)
            {
                Debug.LogWarning($"[PoolManager] 试图回收一个没有 PoolItem 身份证的黑户：{obj.name}，已将其直接销毁！");
                Destroy(obj);
                return;
            }

            obj.SetActive(false); // 弄晕关闭渲染和逻辑
            obj.transform.SetParent(poolRoot); 
            poolDict[itemInfo.prefabPath].Enqueue(obj);
        }

        private GameObject CreateNewObj(string path)
        {
            GameObject prefab = ResourceManager.Instance.Load<GameObject>(path);
            GameObject obj = Instantiate(prefab);
            
            // 给它贴上身份证，以后好回家
            PoolItem item = obj.AddComponent<PoolItem>();
            item.prefabPath = path;
            
            return obj;
        }
    }

    /// <summary>
    /// 池中物的身份证，挂载在被生成的特效/物体身上
    /// </summary>
    public class PoolItem : MonoBehaviour
    {
        public string prefabPath;
    }
}
