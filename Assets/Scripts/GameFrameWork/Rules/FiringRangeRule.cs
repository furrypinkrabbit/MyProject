using System.Collections.Generic;
using UnityEngine;
using GameplayFramework.Core;
using GameplayFramework.CameraSys;
using UIFramework.Constants;

namespace GameplayFramework.Rules
{
    public class FiringRangeRule : GameRuleBase
    {
        public GameObject targetDummyPrefab;

        public override string GetRuleName()
        {
            return "训练靶场";
        }

        public override void PreMatchSetup()
        {
            Vector3[] targetPositions = new Vector3[]
            {
                new Vector3(-6, 1, 6),
                new Vector3(6, 1, -6),
                new Vector3(6, 1, 6)
            };

            foreach (var pos in targetPositions)
            {
                if (targetDummyPrefab != null)
                {
                    Instantiate(targetDummyPrefab, pos, Quaternion.identity);
                }
            }
        }

        public override List<string> GetModeSpecificUIPanels()
        {

            return new List<string> { "CrosshairPanel",UINameConst.ControlPanel };
        }

        public override void AssignTeam(PlayerController pc)
        {
            pc.SetTeam(1);
        }

        public override void ExecutePossess(PlayerController pc, IActor target)
        {
            base.ExecutePossess(pc, target);
            pc.CameraController.SetPerspective(CameraPerspective.FirstPerson);
        }
    }
}
