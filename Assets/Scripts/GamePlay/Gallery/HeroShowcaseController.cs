using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using GameplayFramework.Manager;

namespace UIFramework.Game
{
    // 利用枚举规范底层动作，远离字符串拼写错误！
    public enum AnimState { Idle, WalkForward, WalkBackward }

    public class HeroShowcaseController : MonoBehaviour
    {
        public Animator heroAnimator;
        public string characterAnimFolder = "Agents/Base/Anim";

        public List<AnimationClip> animationClips = new List<AnimationClip>();
        public float transitionSpeed = 5f;
        [HideInInspector] public bool isUIPreviewMode = false;

        private PlayableGraph graph;
        private AnimationMixerPlayable locomotionMixer; // 负责腿部/全身走路移动
        private AnimationClipPlayable currentPlayable;
        private AnimationClipPlayable oldPlayable;

        private AnimState currentState = AnimState.Idle; // 当前状态缓存，防止每帧刷新
        private bool isCrossfading = false;
        private float crossfadeProgress = 0f;

        private void Awake()
        {
            if (!string.IsNullOrEmpty(characterAnimFolder))
            {
                var clips = ResourceManager.Instance.LoadAll<AnimationClip>(characterAnimFolder);
                foreach (var c in clips) if (!animationClips.Contains(c)) animationClips.Add(c);
            }
        }

        private void Start()
        {
            if (heroAnimator == null) heroAnimator = GetComponent<Animator>();

            graph = PlayableGraph.Create("HeroCorePlayables_" + gameObject.GetInstanceID());
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(graph, "Output", heroAnimator);

            // 以后如果有 Avatar Mask 控制上半身开火，就把它改成 AnimationLayerMixerPlayable
            locomotionMixer = AnimationMixerPlayable.Create(graph, 2);
            output.SetSourcePlayable(locomotionMixer);

            graph.Play();

            if (isUIPreviewMode && animationClips.Count > 0)
                PlayAnimRaw(animationClips[0].name);
        }

        // ====== 全新接口：提供给外部绝对安全的枚举流 ======
        public void ChangeState(AnimState newState)
        {
            // 防止一直调用导致重播
            if (currentState == newState) return;
            currentState = newState;

            string targetClip = "idle";
            switch (newState)
            {
                case AnimState.Idle: targetClip = "idle"; break;
                case AnimState.WalkForward: targetClip = "walk_f"; break;
                case AnimState.WalkBackward: targetClip = "walk_b"; break;
            }

            PlayAnimRegex(targetClip);
        }

        // 强行盖过当前移动动作的“动作类技能” (比如开火、近战)
        public void PlayAction(string actionKeyword)
        {
            // 未来这里会接入 Avatar Mask 走上半身轨道，本期先强行打断全身
            PlayAnimRegex(actionKeyword);

            // 为了让动作播完能恢复，重置底层兵态标识，逼迫下一帧的移动指令重新拿回控制权
            currentState = AnimState.WalkBackward; // 随便给个不一样的，引起重判定
        }

        // 模糊搜索：策划不管命名叫 Base_Fight_Idle 还是 idle_a，都能找到
        private void PlayAnimRegex(string keyword)
        {
            keyword = keyword.ToLower();
            var clip = animationClips.Find(c => c.name.ToLower().Contains(keyword));
            if (clip != null) PlayAnimRaw(clip.name);
        }

        private void PlayAnimRaw(string clipName)
        {
            var clip = animationClips.Find(c => c != null && c.name == clipName);
            if (clip == null || !graph.IsValid()) return;

            var newPlayable = AnimationClipPlayable.Create(graph, clip);
            newPlayable.SetApplyFootIK(false);

            if (currentPlayable.IsValid())
            {
                oldPlayable = currentPlayable;
                graph.Disconnect(locomotionMixer, 1);
                graph.Connect(newPlayable, 0, locomotionMixer, 1);
                locomotionMixer.SetInputWeight(0, 1f);
                locomotionMixer.SetInputWeight(1, 0f);
                isCrossfading = true;
                crossfadeProgress = 0f;
            }
            else
            {
                graph.Disconnect(locomotionMixer, 0);
                graph.Connect(newPlayable, 0, locomotionMixer, 0);
                locomotionMixer.SetInputWeight(0, 1f);
            }

            currentPlayable = newPlayable;
            newPlayable.Play();
            currentPlayable.SetTime(0);
        }

        private void Update()
        {
            if (!graph.IsValid() || !isCrossfading) return;
            crossfadeProgress += Time.deltaTime * transitionSpeed;
            if (crossfadeProgress >= 1f)
            {
                crossfadeProgress = 1f;
                isCrossfading = false;
                graph.Disconnect(locomotionMixer, 0);
                if (oldPlayable.IsValid()) oldPlayable.Destroy();
                graph.Disconnect(locomotionMixer, 1);
                graph.Connect(currentPlayable, 0, locomotionMixer, 0);
                locomotionMixer.SetInputWeight(0, 1f);
                locomotionMixer.SetInputWeight(1, 0f);
            }
            else
            {
                locomotionMixer.SetInputWeight(0, 1f - crossfadeProgress);
                locomotionMixer.SetInputWeight(1, crossfadeProgress);
            }
        }

        public void PlayAnim(string clipName)
        {
            // 让旧的选人界面的强播动作系统，直接嫁接到我们新的半身打断管道里！
            PlayAction(clipName);
        }

        private void OnDestroy() { if (graph.IsValid()) graph.Destroy(); }
    }
}
