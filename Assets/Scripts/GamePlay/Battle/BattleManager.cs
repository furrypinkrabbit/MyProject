using UnityEngine;
using GameplayFramework.Core;
using GameplayFramework.Rules;
using UIFramework.Core;
using UIFramework.Events;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

namespace GameplayFramework.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        public GameRuleBase currentRule; 

        public Transform showcasePoint; 
        public Vector3 mapSpawnPoint = new Vector3(0, 0, 0);
        public GameObject showcaseCamera; 

        private GameObject currentPreviewModel;
        private bool hasInitialized = false;

        private void Awake() 
        {
            if (SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().name.ToLower().Contains("login"))
            {
                Destroy(this.gameObject); return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene() == gameObject.scene) StartCoroutine(InitBattleRoutine());
            else SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene == gameObject.scene && !hasInitialized)
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
                StartCoroutine(InitBattleRoutine());
            }
        }

        private IEnumerator InitBattleRoutine()
        {
            hasInitialized = true;
            yield return null; 
            
            UIManager.Instance.ClosePanel("LoginPanel");
            if (showcaseCamera != null) showcaseCamera.SetActive(true);

            Room.RoomManager.Instance.SetGameRule(currentRule);
            currentRule.PreMatchSetup();
            
            UIEventCenter.AddListener("RequestCharList", OnRequestCharList);
            UIEventCenter.AddListener<int>("OnPreviewChar", (index) =>
            {
                if (currentPreviewModel != null) Destroy(currentPreviewModel);
                currentPreviewModel = Instantiate(currentRule.characterPrefabs[index], showcasePoint.position, showcasePoint.rotation);
                
                var previewActor = currentPreviewModel.GetComponent<GameplayFramework.Actors.HeroActor>();
                if (previewActor != null && previewActor.firstPersonCameraObj != null) previewActor.firstPersonCameraObj.SetActive(false); 

                var showcaseCtrl = currentPreviewModel.GetComponent<UIFramework.Game.HeroShowcaseController>();
                if (showcaseCtrl != null) showcaseCtrl.isUIPreviewMode = true; 
                
                Invoke(nameof(PlayPreviewAnim), 0.1f);
            });

            UIEventCenter.AddListener("OnReplayPreviewAction", PlayPreviewAnim);

            UIEventCenter.AddListener<int>("OnConfirmChar", (index) =>
            {
                if (currentPreviewModel != null) Destroy(currentPreviewModel);
                if (showcaseCamera != null) showcaseCamera.SetActive(false);

                UIManager.Instance.ClosePanel("CharacterSelectPanel");
                
                // 【准星修复核心】：为了应对反射崩溃，根据明确强类型去生成准星
                foreach(var ui in currentRule.GetModeSpecificUIPanels())
                {
                    if (ui == "CrosshairPanel") UIManager.Instance.OpenPanel<GameplayFramework.UIFrame.CrosshairPanel>(ui, UILayer.Default);
                    else UIManager.Instance.OpenPanel<GameplayFramework.UIFrame.SimpleDisplayPanel>(ui, UILayer.Default);
                }

                PlayerController localPlayer = Room.RoomManager.Instance.CreatePlayer("LocalPlayer");
                localPlayer.InitInputMap(); 
                currentRule.AssignTeam(localPlayer);
                
                GameObject realHero = Instantiate(currentRule.characterPrefabs[index], mapSpawnPoint, Quaternion.identity);
                
                var realCtrl = realHero.GetComponent<UIFramework.Game.HeroShowcaseController>();
                if (realCtrl != null)
                {
                    realCtrl.isUIPreviewMode = false;
                    realCtrl.PlayAnim("Fight_idle");
                }

                currentRule.ExecutePossess(localPlayer, realHero.GetComponent<IActor>());
                currentRule.StartMatch();
            });

            UIManager.Instance.OpenPanel<GameplayFramework.UIFrame.CharacterSelectPanel>("CharacterSelectPanel", UILayer.Top);
        }

        private void OnRequestCharList()
        {
            var names = currentRule.characterPrefabs.Select(p => p.name).ToList();
            UIEventCenter.Trigger("OnReceiveCharList", names);
        }

        private void PlayPreviewAnim()
        {
            if (currentPreviewModel != null)
            {
                var showcase = currentPreviewModel.GetComponent<UIFramework.Game.HeroShowcaseController>();
                if (showcase != null) showcase.PlayAnim("Fight_idle");
            }
        }
        
        private void OnDestroy() 
        { 
            UIEventCenter.RemoveListener("RequestCharList", OnRequestCharList); 
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }
    }
}
