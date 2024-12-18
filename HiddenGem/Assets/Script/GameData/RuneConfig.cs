using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Script.GameData
{
    [CreateAssetMenu( fileName = "RuneConfig", menuName = "GameData/RuneConfig", order = 1)]
    public class RuneConfig : SerializedScriptableObject
    {
        public List<Rune> runes;
        
        public IEnumerable RuneIds()
        {
            return runes.Select(x => x.id);
        }
    }

    [Serializable]
    public class Rune
    {
        public string id;
        public int width;
        public int height;
    }
}