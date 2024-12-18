using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Tuleeeeee.Enum;
using Tuleeeeee.GameSystem;
using UnityEngine;

namespace Tuleeeeee.Features.Mission
{
    public class MissionDestination : MonoBehaviour
    {
        public int level = 1;
        public Destination destination;
        public List<Destination> Destination;
        public List<Transform> targetsPosition;

        public Transform GetDestination(RuneType runeType, int id)
        {
            return Destination.Find(x => x.runeType == runeType && x.id == id).transform;
        }

        private void InstantiateDestination(Destination des, RuneType runeType, Vector3 position, Quaternion rotation,
            int id, Transform parent)
        {
            Destination newDestination = Instantiate(des, position, rotation);
            newDestination.Init(runeType, id);
            newDestination.transform.SetParent(parent);
            Destination.Add(newDestination);
        }

        private void Start()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.RegisterOnLevelClear(OnLevelClear);
            List<RuneMap> runeMaps = levelSystem.GetRuneMaps();

            for (int i = 0; i < runeMaps.Count; i++)
            {
                var rune = runeMaps[i];
                // Get the target position from the list (use i for the index)
                Transform targetTransform = targetsPosition[i];

                // Instantiate the destination with the correct position and rotation
                InstantiateDestination(destination, rune.RuneType, targetTransform.position, targetTransform.rotation,
                    rune.Group, targetTransform);
            }
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