using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using GameplayFramework.Data;

namespace UIFramework.Game
{
    public enum AnimState { AnimNone, Idle, WalkForward, WalkBackward, ActionPlaying }

    public class HeroShowcaseController : MonoBehaviour
    {
        [Header("动作控制大脑：把配好的资产档案拖给它！")]
        public AnimEventProfile configProfile;
        public Animator heroAnimator;
        public float transitionSpeed = 5f;

        [Header("是否是选人展台模式（UI不附身）")]
        public bool isUIPreviewMode = false;
        private float uiIdleTimer = 0f;

        private PlayableGraph graph;
        private AnimationMixerPlayable locomotionMixer;
        private AnimationClipPlayable currentPlayable;
        private AnimationClipPlayable oldPlayable;

        public AnimState currentState = AnimState.AnimNone;
        private bool isCrossfading = false;
        private float crossfadeProgress = 0f;

        private void Start()
        {
            if (heroAnimator == null) heroAnimator = GetComponent<Animator>();

            graph = PlayableGraph.Create("HeroCorePlayables_" + gameObject.GetInstanceID());
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(graph, "Output", heroAnimator);
            locomotionMixer = AnimationMixerPlayable.Create(graph, 2);
            output.SetSourcePlayable(locomotionMixer);

            graph.Play();

            if (isUIPreviewMode)
            {
                TriggerActionEvent("show");
            }
            else
            {
                ChangeState(AnimState.Idle);
            }
        }

        public void ChangeState(AnimState newState)
        {
            if (newState == AnimState.AnimNone)
            {
                currentState = AnimState.AnimNone;
                return;
            }

            if (currentState == AnimState.ActionPlaying) return;
            if (currentState == newState) return;

            currentState = newState;
            if (configProfile == null) return;

            AnimationClip targetClip = configProfile.idleClip;
            switch (newState)
            {
                case AnimState.Idle: targetClip = configProfile.idleClip; break;
                case AnimState.WalkForward: targetClip = configProfile.walkForwardClip; break;
                case AnimState.WalkBackward: targetClip = configProfile.walkBackwardClip; break;
            }

            if (targetClip != null) PlayAnimRaw(targetClip);
        }

        public void TriggerActionEvent(string eventName)
        {
            if (configProfile == null) return;

            AnimationClip targetClip = configProfile.GetClipByEvent(eventName);
            if (targetClip == null)
            {
                Debug.LogWarning($"[Anim Event] 当前英雄的档案里没有配置事件：{eventName}");
                return;
            }

            currentState = AnimState.ActionPlaying;
            PlayAnimRaw(targetClip);

            float duration = (float)targetClip.length;
            CancelInvoke(nameof(ResetActionState));
            Invoke(nameof(ResetActionState), duration - 0.1f);
        }

        private void ResetActionState()
        {
            if (currentState == AnimState.ActionPlaying)
            {
                currentState = AnimState.AnimNone;
                uiIdleTimer = 0f;
            }
        }

        private void PlayAnimRaw(AnimationClip clip)
        {
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
            if (isUIPreviewMode && currentState != AnimState.ActionPlaying)
            {
                uiIdleTimer += Time.deltaTime;
                if (uiIdleTimer >= 5f)
                {
                    TriggerActionEvent("show");
                    uiIdleTimer = 0f;
                }
            }

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

        // ================= 修复问题 2 =================
        // 为了防止那个远古的选人画廊(BattleManager.cs)因为重写找不到方法炸锅，强加这个向下兼容的旧口子！
        public void PlayAnim(string clipName)
        {
            // 旧口子现在接管到新的事件大网上去！
            TriggerActionEvent(clipName);
        }

        private void OnDestroy() { if (graph.IsValid()) graph.Destroy(); }
    }
}
