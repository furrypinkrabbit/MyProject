using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIFramework.Core
{
    /// <summary>
    /// 全局场景管理器，负责异步加载场景、进度回调，配合UIManager完美实现带有Loading界面的切换
    /// </summary>
    public class SceneDirector : MonoBehaviour
    {
        public static SceneDirector Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 异步加载场景的最简调用
        /// </summary>
        public void LoadSceneAsync(string sceneName, Action onComplete = null, Action<float> onProgress = null)
        {
            StartCoroutine(LoadSceneRoutine(sceneName, onComplete, onProgress));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, Action onComplete, Action<float> onProgress)
        {
            // 可以在这里调用 UIManager 打开一个全局的 LoadingPanel
            
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false; // 阻止加载到90%时立刻自动跳场景，让我们有控制权

            while (op.progress < 0.9f)
            {
                onProgress?.Invoke(op.progress);
                yield return null;
            }

            // 加载完成最后的 10%
            onProgress?.Invoke(1f);
            op.allowSceneActivation = true;

            // 等待一帧确保场景真的激活了
            yield return new WaitForEndOfFrame();
            
            // 可以在这里关闭 LoadingPanel
            onComplete?.Invoke();
        }
    }
}
