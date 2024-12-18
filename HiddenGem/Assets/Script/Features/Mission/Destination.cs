using System.Collections.Generic;
using Tuleeeeee.Enum;
using Tuleeeeee.GameSystem;
using UnityEngine;

namespace Tuleeeeee.Features.Mission
{
    public class Destination : MonoBehaviour
    {
        public RuneType runeType;
        public int id;

        public void Init(RuneType runeType, int id)
        {
            this.runeType = runeType;
            this.id = id;
        }
    }
}