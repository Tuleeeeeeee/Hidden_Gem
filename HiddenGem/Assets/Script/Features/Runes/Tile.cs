using Tuleeeeee.GameSystem;
using Tuleeeeee.Enum;
using UnityEngine;

namespace Tuleeeeee.Runes
{
    public class Tile : MonoBehaviour
    {
        public RuneType runeType;

        public int row { get; private set; }
        public int column { get; private set; }

        public void Init(int column, int row, RuneType runeType)
        {
            this.column = column;
            this.row = row;
            this.runeType = runeType;
        }

        private void OnMouseDown()
        {
            LevelSystem.Instance.Crusher(column, row, runeType);

            Destroy(gameObject);
        }
    }
}