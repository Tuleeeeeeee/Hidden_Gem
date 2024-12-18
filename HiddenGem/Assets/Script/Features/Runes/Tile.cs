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
        public int id { get; private set; }

        public void Init(int row, int column, RuneType runeType, int id)
        {
            this.row = row;
            this.column = column;
            this.runeType = runeType;
            this.id = id;
        }

        private void OnMouseDown()
        {
            LevelSystem.Instance.Crusher(row, column, runeType, id);

            Destroy(gameObject);
        }
    }
}