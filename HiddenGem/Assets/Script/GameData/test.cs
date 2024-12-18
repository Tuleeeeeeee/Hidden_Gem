using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using Script.GameData;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Random = System.Random;

[CreateAssetMenu(fileName = "Le", menuName = "GameData/LevelDat", order = 1)]
public class Test : SerializedScriptableObject
{
    [Serializable]
    public class RuneMap
    {
        public RuneType RuneType;
        public int Group;
    }

    public enum RuneType
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Pink
    }

    [Header("Level Config")] public int Level;
    public int Rows;
    public int Columns;
    public int CrushCount = 10;

    [Header("Rune Config")] public RuneConfig RuneConfig;

    [ValueDropdown("RuneIds")] public List<string> useRune;
    public IEnumerable RuneIds => RuneConfig?.RuneIds();

    [TableMatrix(SquareCells = true, DrawElementMethod = "DrawElementMethod"), TabGroup("Table", "Table")]
    public RuneMap[,] Table;

    public RuneMap DrawElementMethod(Rect rect, RuneMap value)
    {
        Color color = Color.clear;
        switch (value.RuneType)
        {
            case RuneType.None:
                color = Color.grey;
                break;
            case RuneType.Red:
                color = Color.red;
                break;
            case RuneType.Green:
                color = Color.green;
                break;
            case RuneType.Blue:
                color = Color.blue;
                break;
            case RuneType.Yellow:
                color = Color.yellow;
                break;
            case RuneType.Cyan:
                color = Color.cyan;
                break;
            case RuneType.Pink:
                color = Color.magenta;
                break;
        }

        UnityEditor.EditorGUI.DrawRect(rect.Padding(1), color);
        if (value.Group != 0)
            UnityEditor.EditorGUI.LabelField(rect.Padding(rect.width / 3), value.Group.ToString(), new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            });
        return value;
    }

    [Button]
    public void CreateTable()
    {
        Table = new RuneMap[Rows, Columns];
        // init table 
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Table[i, j] = new RuneMap()
                {
                    RuneType = RuneType.None,
                };
            }
        }
    }

    [Button]
    public void RandomRune()
    {
        BoardFiller boardFiller = new BoardFiller(Columns);
        if (!boardFiller.FillBoard(0))
        {
            Debug.Log("Can't fill the board");
            return;
        }

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Table[i, j] = new RuneMap()
                {
                    RuneType = (RuneType)boardFiller.board[i, j],
                };
            }
        }
    }
}


public class BoardFiller
{
    int boardSize = 6;

    Random random;

    public BoardFiller(int w)
    {
        random = new Random(DateTime.Now.GetHashCode());
        boardSize = w;
        board = new int[w, w];
    }

    public int[,] board;

    // Pieces: PieceId, Width, Height
    (int id, int width, int height)[] pieces = new (int, int, int)[]
    {
        (1, 1, 3), // Piece 1: 1x3
        (2, 1, 3), // Piece 2: 1x3
        (3, 1, 2), // Piece 3: 1x2
        (4, 2, 2), // Piece 4: 2x2
        (5, 2, 3), // Piece 5: 2x3
        (6, 2, 3) // Piece 6: 2x3
    };

    public bool FillBoard(int pieceIndex)
    {
        if (pieceIndex >= pieces.Length)
        {
            // All pieces have been successfully placed
            PrintBoard();
            return true;
        }

        var (id, originalWidth, originalHeight) = pieces[pieceIndex];

        // Randomize whether to try rotated version first
        bool tryRotatedFirst = random.Next(0, 2) == 1;

        // Define original and rotated versions
        var orientations = tryRotatedFirst
            ? new[] { (originalHeight, originalWidth), (originalWidth, originalHeight) }
            : new[] { (originalWidth, originalHeight), (originalHeight, originalWidth) };

        foreach (var (width, height) in orientations)
        {
            List<(int row, int col)> positions = GenerateShuffledPositions(width, height);

            foreach (var (row, col) in positions)
            {
                if (CanPlacePiece(row, col, width, height))
                {
                    PlacePiece(row, col, width, height, id);

                    // Recursively try to place the next piece
                    if (FillBoard(pieceIndex + 1))
                        return true;

                    // Backtrack: Remove the piece
                    RemovePiece(row, col, width, height);
                }
            }
        }

        return false; // Couldn't place this piece anywhere
    }

    private List<(int row, int col)> GenerateShuffledPositions(int width, int height)
    {
        List<(int row, int col)> positions = new List<(int row, int col)>();

        // Generate all valid positions
        for (int row = 0; row <= boardSize - height; row++)
        {
            for (int col = 0; col <= boardSize - width; col++)
            {
                positions.Add((row, col));
            }
        }

        // Shuffle the list of positions
        for (int i = 0; i < positions.Count; i++)
        {
            int j = random.Next(i, positions.Count);
            (positions[i], positions[j]) = (positions[j], positions[i]);
        }

        return positions;
    }

    private bool CanPlacePiece(int row, int col, int width, int height)
    {
        for (int r = row; r < row + height; r++)
        {
            for (int c = col; c < col + width; c++)
            {
                if (board[r, c] != 0) return false; // Overlapping or out of bounds
            }
        }

        return true;
    }

    private void PlacePiece(int row, int col, int width, int height, int id)
    {
        for (int r = row; r < row + height; r++)
        {
            for (int c = col; c < col + width; c++)
            {
                board[r, c] = id;
            }
        }
    }

    private void RemovePiece(int row, int col, int width, int height)
    {
        for (int r = row; r < row + height; r++)
        {
            for (int c = col; c < col + width; c++)
            {
                board[r, c] = 0;
            }
        }
    }

    private void PrintBoard()
    {
        Debug.Log("Done:");
    }
}