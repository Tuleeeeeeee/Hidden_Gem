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
        private Vector3 initialPosition;
        private Vector3 initialScale;

        private void Start()
        {
            // Lưu giá trị ban đầu
            initialPosition = transform.localPosition;
            initialScale = transform.localScale;
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
            seq.Play();
        }
    }
}