using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tuleeeeee.GameData
{
    [CreateAssetMenu(fileName = "ManagerGameLevel", menuName = "GameData/ManagerGameLevel", order = 1)]
    public class ManagerGameLevel : SerializedScriptableObject
    {
        public List<LevelData> LevelDatas;
        
        public LevelData GetLevel(int level)
        {
            return LevelDatas.Find(x=> x.Level == level);
        }
    }
}