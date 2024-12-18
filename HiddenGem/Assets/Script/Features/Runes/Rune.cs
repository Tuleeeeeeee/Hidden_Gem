using DG.Tweening;
using Tuleeeeee.GameSystem;
using Sirenix.OdinInspector;
using Tuleeeeee.Enum;
using UnityEngine;

namespace Tuleeeeee.Runes
{
    public class Rune : MonoBehaviour
    {
        public RuneType RuneType;
        public int RuneId;
        public Vector3 RunSize;

        private void Start()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.RegisterOnClearRune(OnClearRune);
        }

        private void OnDestroy()
        {
            var levelSystem = LevelSystem.Instance;
            levelSystem.UnRegisterOnClearRune(OnClearRune);
        }

        private void OnClearRune(RuneType runeType, int id)
        {
            if (RuneType == runeType && RuneId == id)
            {
                FlyToDestination();
            }
        }

        public void Init(RuneType runeType, Vector3 size, int id)
        {
            RuneType = runeType;
            RunSize = size;
            RuneId = id;
        }


        [Button]
        public void FlyToDestination()
        {
            var levelSystem = LevelSystem.Instance;

            var level = levelSystem.GetDestination(RuneType, RuneId);
            var seq = DOTween.Sequence();

            seq.Append(transform.DOScale(transform.localScale * 1.5f, 1f));
            seq.Append(transform.DOMove(level.position, 0.5f)).Join(transform.DOScale(Vector3.one * 0.6f, 0.5f));
            seq.AppendCallback(() =>
            {
                transform.SetParent(levelSystem.currentMissionDestination.transform);
                levelSystem.CheckLevelClear();
            });
            seq.Play();
        }
    }
}