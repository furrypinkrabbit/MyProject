using UnityEngine;
using System.Collections.Generic;
using GameplayFramework.Core;
using GameplayFramework.Rules;

namespace GameplayFramework.Room
{
    /// <summary>
    /// 房间驱动器：管理 MapId, SubMapId，玩家列表，以及挂载当前的游戏规则 (Rule)
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        public string CurrentMapId { get; private set; }
        public string CurrentSubMapId { get; private set; }

        // 当前房间里所有的玩家灵魂
        private Dictionary<string, PlayerController> players = new Dictionary<string, PlayerController>();

        // 当前房间的规则
        public GameRuleBase CurrentRule { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public GameplayFramework.Core.PlayerController GetLocalPlayer()
        {
            // 如果你有存本地玩家的变量，这里就 return localPlayer;
            // 如果你是本地靶机游戏，你可以写最省事的强取法：
            return GameObject.FindObjectOfType<GameplayFramework.Core.PlayerController>();
        }

        public void InitRoom(string mapId, string subMapId)
        {
            CurrentMapId = mapId;
            CurrentSubMapId = subMapId;
            Debug.Log($"[Room] 初始化房间, 大地图:{mapId}, 子地图:{subMapId}");
            
            // 这里可以结合我们之前的 SceneDirector 去异步加载场景...
        }

        public PlayerController CreatePlayer(string playerId)
        {
            GameObject pcObj = new GameObject($"PC_{playerId}");
            var pc = pcObj.AddComponent<PlayerController>();
            pc.Init(playerId);
            players.Add(playerId, pc);
            return pc;
        }

        public void SetGameRule(GameRuleBase rule)
        {
            CurrentRule = rule;
        }
    }
}
