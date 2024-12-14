using System.Collections.Generic;
using Tuleeeeee.Enum;
using UnityEngine;

namespace Tuleeeeee.Features.Mission
{
    public class MissionDestination : MonoBehaviour
    {
        public int level = 1;
        public List<Destination> Destinations;
        
        public Transform GetDestination(RuneType runeType)
        {
            return Destinations.Find(x => x.runeType == runeType).transform;
        }
    }
}