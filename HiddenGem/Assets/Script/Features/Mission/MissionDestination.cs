using System.Collections.Generic;
using DG.Tweening;
using Tuleeeeee.Enum;
using Tuleeeeee.GameSystem;
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

        private void Start()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.RegisterOnLevelClear(OnLevelClear);
        }

        private void OnDestroy()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.UnRegisterOnLevelClear(OnLevelClear);
        }

        private void OnLevelClear()
        {
            var seq = DOTween.Sequence();

            seq.Append(transform.DOScale(Vector3.one * 0.9f, 0.5f));
            seq.Append(transform.DORotate(new Vector3(0, 0, 360), 4f));
            seq.Join(transform.DOMoveX(50, 2f));
        }
    }
}