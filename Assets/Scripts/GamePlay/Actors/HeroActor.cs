using UnityEngine;
using GameplayFramework.Core;
using GameplayFramework.Combat;
using UIFramework.Game;

namespace GameplayFramework.Actors
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(HealthComponent))]
    public class HeroActor : MonoBehaviour, IActor
    {
        public Transform ActorTransform => transform;
        public float walkSpeed = 4.5f;
        public float gravity = 9.81f;

        public GameObject firstPersonCameraObj;
        public float lookSensitivity = 0.2f;

        private CharacterController cc;
        private HeroShowcaseController animController;
        private HealthComponent healthComp;

        private Vector2 currentMoveInput;
        private float cameraPitch = 0f;
        private float verticalVelocity = 0f;

        // ==== 【核心积木】：灵魂标识 ====
        // 只有被玩家附身（上机）后，这具肉体的高级计算才会开始！
        public bool IsPossessed { get; private set; } = false;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            animController = GetComponent<HeroShowcaseController>();
            healthComp = GetComponent<HealthComponent>();

            var animator = GetComponent<Animator>();
            if (animator != null) animator.applyRootMotion = false;

            if (firstPersonCameraObj != null) firstPersonCameraObj.SetActive(false);
        }

        public void OnPossess(PlayerController controller)
        {
            IsPossessed = true; // 灵魂注入！
            if (firstPersonCameraObj != null) firstPersonCameraObj.SetActive(true);
            Transform headTrans = transform.Find("Head") ?? transform;
            controller.CameraController.BindToActor(headTrans);
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnUnpossess()
        {
            IsPossessed = false; // 灵魂抽离，变成死物
            if (firstPersonCameraObj != null) firstPersonCameraObj.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            currentMoveInput = Vector2.zero;
            if (animController != null) animController.ChangeState(AnimState.Idle);
        }

        public void OnMoveInput(Vector2 direction) { currentMoveInput = direction; }

        public void OnLookInput(Vector2 delta)
        {
            float yaw = delta.x * lookSensitivity;
            transform.Rotate(0, yaw, 0);

            if (firstPersonCameraObj != null)
            {
                cameraPitch -= delta.y * lookSensitivity;
                cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);
                firstPersonCameraObj.transform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
            }
        }

        public void OnCombatCommand(CombatInputAction action, bool isPressed)
        {
            if (!isPressed || !IsPossessed) return;

            switch (action)
            {
                case CombatInputAction.PrimaryFire:
                    animController.PlayAction("Shoot");
                    break;
                case CombatInputAction.Melee:
                    animController.PlayAction("Melee");
                    break;
            }
        }

        private void Update()
        {
            // 【楚河汉界】：如果没人附身（比如在选人界面展台罚站），不要执行任何肉体判定逻辑！
            if (!IsPossessed) return;

            if (cc == null || animController == null) return;
            if (healthComp.isDead) return;

            if (cc.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
            verticalVelocity -= gravity * Time.deltaTime;

            Vector3 horizontalMove = transform.forward * currentMoveInput.y + transform.right * currentMoveInput.x;
            Vector3 finalMove = horizontalMove * walkSpeed + Vector3.up * verticalVelocity;
            cc.Move(finalMove * Time.deltaTime);

            if (currentMoveInput.sqrMagnitude < 0.01f)
            {
                animController.ChangeState(AnimState.Idle);
            }
            else
            {
                if (currentMoveInput.y > 0.1f) animController.ChangeState(AnimState.WalkForward);
                else if (currentMoveInput.y < -0.1f) animController.ChangeState(AnimState.WalkBackward);
                else animController.ChangeState(AnimState.WalkForward);
            }
        }

        public void OnActionPressed(int actionId)
        {
            throw new System.NotImplementedException();
        }
    }
}
