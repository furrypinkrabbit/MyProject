using GameplayFramework.Core;
using GameplayFramework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GameFrameWork.Rules
{

    public class ReTriveModeRule : GameRuleBase
    {

        public override void AssignTeam(PlayerController pc)
        {
            Random rd = new Random();
            int teamId = (int)rd.Next(0,2);
            pc.SetTeam(teamId);
        }

        public override void ExecutePossess(PlayerController pc, IActor target)
        {
            base.ExecutePossess(pc, target);
        }

        public override List<string> GetModeSpecificUIPanels()
        {
            throw new NotImplementedException();
        }

        public override string GetRuleName()
        {
            throw new NotImplementedException();
        }

        public override void PreMatchSetup()
        {
            base.PreMatchSetup();
        }

        public override void StartMatch()
        {
            base.StartMatch();
        }
    }
}
