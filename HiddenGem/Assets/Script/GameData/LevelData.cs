using Sirenix.OdinInspector;
using Tuleeeeee.Enum;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "GameData/LevelData",order = 1)]
public class LevelData : SerializedScriptableObject
{
    public int Level;

    public int Rows;
    public int Columns;
     
    [TableMatrix(SquareCells = true)]
    public RuneType[,] Table;

    [Button]
    public void CreateTable()
    {
        Table = new RuneType[Columns, Rows];
        // init table 
        for (int col = 0; col < Columns; col++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Table[col, row] = RuneType.None;
            }
        }
    }
}
