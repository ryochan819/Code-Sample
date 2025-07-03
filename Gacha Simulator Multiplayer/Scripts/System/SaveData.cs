using System.Collections.Generic;
using UnityEngine;

namespace Gacha.system
{
    public class SaveData
    {
        public bool IsNewSave => localPlayerData == null;

        [Header("Player Data")]
        public PlayerData localPlayerData;
        public List<PlayerData> multiplayerDatas;

        [Header("Shop Data")]
        public int money;
    }
}