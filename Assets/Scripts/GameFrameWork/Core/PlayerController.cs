using UnityEngine;
using UnityEngine.InputSystem;
using GameplayFramework.CameraSys;
using GameplayFramework.Combat;

namespace GameplayFramework.Core
{
    public class PlayerController : MonoBehaviour
    {
        public string PlayerId { get; private set; }
        public int TeamId { get; private set; }
        public GameplayFramework.Actors.HeroActor CurrentActor { get; private set; }
        public PlayerCameraController CameraController { get; private set; }

        [Header("基础移动")]
        public InputActionReference moveAction;
        public InputActionReference lookAction;

        [Header("战斗技能 mappings")]
        public InputActionReference primaryFire;
        public InputActionReference secondaryFire;
        public InputActionReference skill1_E;
        public InputActionReference skill2_LShift;
        public InputActionReference ultimate_Q;
        public InputActionReference reload_R;
        public InputActionReference melee_F;

        private void SetupCombatAction(InputActionReference reference, CombatInputAction combatInput)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.Enable();
                reference.action.performed += ctx => SendCombatSignal(combatInput, true);
                reference.action.canceled += ctx => SendCombatSignal(combatInput, false);
            }
        }

        private void SendCombatSignal(CombatInputAction action, bool isPressed)
        {
            if (CurrentActor != null) CurrentActor.OnCombatCommand(action, isPressed);
        }

        public void Init(string playerId)
        {
            PlayerId = playerId;
            var camObj = new GameObject($"Camera_{playerId}");
            camObj.transform.SetParent(this.transform);
            CameraController = camObj.AddComponent<PlayerCameraController>();
        }

        public void InitInputMap()
        {
            if (moveAction != null && moveAction.action != null) moveAction.action.Enable();
            if (lookAction != null && lookAction.action != null) lookAction.action.Enable();

            SetupCombatAction(primaryFire, CombatInputAction.PrimaryFire);
            SetupCombatAction(secondaryFire, CombatInputAction.SecondaryFire);
            SetupCombatAction(skill1_E, CombatInputAction.Skill1);
            SetupCombatAction(skill2_LShift, CombatInputAction.Skill2);
            SetupCombatAction(ultimate_Q, CombatInputAction.Ultimate);
            SetupCombatAction(reload_R, CombatInputAction.Reload);
            SetupCombatAction(melee_F, CombatInputAction.Melee);
        }

        public void SetTeam(int teamId) { TeamId = teamId; }

        public void Possess(IActor newActor)
        {
            if (CurrentActor != null) CurrentActor.OnUnpossess();

            CurrentActor = newActor as GameplayFramework.Actors.HeroActor;

            if (CurrentActor != null)
            {
                CurrentActor.OnPossess(this);
                CameraController.BindToActor(newActor.ActorTransform);
            }
        }

        private void Update()
        {
            if (CurrentActor == null) return;

            // ================= 模块自适应保底机制 =================
            // 防呆设计：如果策划忘记在这上面拖入高级动作图表（New Input System），
            // 绝不报错停机，而是无缝退回到 Unity 原生经典按键监听法！

            Vector2 moveVec = Vector2.zero;
            if (moveAction != null && moveAction.action != null)
                moveVec = moveAction.action.ReadValue<Vector2>();
            else
                moveVec = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            CurrentActor.OnMoveInput(moveVec);

            Vector2 lookVec = Vector2.zero;
            if (lookAction != null && lookAction.action != null)
                lookVec = lookAction.action.ReadValue<Vector2>();
            else
                lookVec = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            CurrentActor.OnLookInput(lookVec);

            // 老经典强检测补漏
            if ((primaryFire == null || primaryFire.action == null) && Input.GetMouseButtonDown(0)) SendCombatSignal(CombatInputAction.PrimaryFire, true);
            if ((melee_F == null || melee_F.action == null) && Input.GetKeyDown(KeyCode.F)) SendCombatSignal(CombatInputAction.Melee, true);
        }
    }
}
