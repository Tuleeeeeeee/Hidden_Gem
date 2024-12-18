using Tuleeeeee.Enum;

namespace Tuleeeeee.Model
{
    public class LevelModel
    {
        //public RuneType[,] CloneTable;
        public RuneMap[,] CloneTable;

        public int CrushCount;

        public bool HasClear()
        {
            for (int i = 0; i < CloneTable.GetLength(0); i++)
            {
                for (int j = 0; j < CloneTable.GetLength(1); j++)
                {
                    if (CloneTable[i, j].RuneType != RuneType.None)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}