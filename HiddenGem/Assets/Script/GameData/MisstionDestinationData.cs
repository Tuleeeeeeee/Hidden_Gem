using System.Collections.Generic;
using Tuleeeeee.Features.Mission;
using UnityEngine;

namespace Tuleeeeee.GameData
{
    [CreateAssetMenu(fileName = "MissionDestinationData", menuName = "GameData/MissionDestinationData",order = 1)]
    public class MissionDestinationData  : ScriptableObject
    {
        public List<MissionDestination> MissionDestinations;
        
        public MissionDestination GetMissionDestination(int level)
        {
            return MissionDestinations.Find(x=> x.level == level);
        }
    }
}