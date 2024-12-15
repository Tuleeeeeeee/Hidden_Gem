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
        private void OnClearRune(RuneType runetype)
        {
            if (RuneType == runetype)
            {
                FlyToDestination();
            }
        }
        public void Init(RuneType runeType)
        {
            RuneType = runeType;
        }

        [Button]
        public void FlyToDestination()
        {
            var levelSystem = LevelSystem.Instance;

            var level = levelSystem.GetDestination(RuneType);
            var seq = DOTween.Sequence();

            seq.Append(transform.DOScale(Vector3.one * 1.2f, 0.5f));
            seq.Append(transform.DOMove(level.position, 0.5f)).Join(transform.DOScale(Vector3.one * 0.5f, 0.5f));
            seq.AppendCallback(() =>
            {
                transform.SetParent(levelSystem.currentMissionDestination.transform);
                levelSystem.CheckLevelClear();
            });
            seq.Play();
        }
    }
}