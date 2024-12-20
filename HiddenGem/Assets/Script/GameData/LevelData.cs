using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tuleeeeee.Enum;
using Tuleeeeee.Runes;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class RuneMap
{
    public RuneType RuneType;
    public int Group;
}

[CreateAssetMenu(fileName = "Level", menuName = "GameData/LevelData", order = 1)]
public class LevelData : SerializedScriptableObject
{
    public int Level;

    public int Rows;
    public int Columns;

    public int CrushCount = 10;
    public Tile tilePrefab;
    public List<Rune> runePrefab;
    public List<RuneMap> runeMaps = new List<RuneMap>();

    //[TableMatrix(HorizontalTitle = "Row", VerticalTitle = "Column")]
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
            case RuneType.Red1x2:
                color = Color.red;
                break;
            case RuneType.Blue1x3:
                color = Color.blue;
                break;
            case RuneType.Green1x4:
                color = Color.green;
                break;
            case RuneType.Black1x5:
                color = Color.black;
                break;
            case RuneType.Cyan2x2:
                color = Color.cyan;
                break;
            case RuneType.Yellow2x3:
                color = Color.yellow;
                break;
            case RuneType.Pink2x4:
                color = Color.magenta;
                break;
            case RuneType.White4x4:
                color = Color.white;
                break;
         
        }
        UnityEditor.EditorGUI.DrawRect(rect.Padding(1),color );
        if(value.Group != 0)
            UnityEditor.EditorGUI.LabelField(rect.Padding(rect.width/3), value.Group.ToString(), new GUIStyle()
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
    
    
    [DictionaryDrawerSettings(KeyLabel = "GemType", ValueLabel = "GemLimited")]
    public Dictionary<RuneType, int> runeLimits = new Dictionary<RuneType, int>
    {
        { RuneType.Red1x2, 0 },
    };

    private Dictionary<RuneType, int> runCount;

    [DictionaryDrawerSettings(KeyLabel = "GemType", ValueLabel = "GemSize")]
    public readonly Dictionary<RuneType, GemSize> gemSize = new Dictionary<RuneType, GemSize>
    {
        { RuneType.Red1x2, new GemSize() },
    };

    [InlineProperty(LabelWidth = 90)]
    public struct GemSize
    {
        public int width;
        public int height;
    }


    [Button]
    public void CreateTable()
    {
        runeMaps.Clear();
        
        Table = new RuneMap[Rows, Columns];
        // init table 
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Table[row, col] = new RuneMap()
                {
                    RuneType = RuneType.None,
                };
            }
        }
    }

    [Button]
    public void RandomRunes()
    {
        runeMaps.Clear();
        var sortedRunes = runeLimits.Keys
            .OrderByDescending(rune => GetRuneHeight(rune) * GetRuneWidth(rune))
            .ToList();

        int groupId = 0; // Start with the first group
        foreach (var rune in sortedRunes)
        {
            int limit = runeLimits[rune];
            for (int i = 0; i < limit; i++)
            {
              //  Debug.Log($"Placing {limit} {rune} runes.");
                PlaceRandomRune(rune, groupId);
                groupId++;
            }
        }
    }


    private void PlaceRandomRune(RuneType rune, int groupId)
    {
       // Debug.Log($"Attempting to place {rune} in group {groupId}...");
        int height = GetRuneHeight(rune);
        int width = GetRuneWidth(rune);

        int maxAttempts = 1000; // Maximum attempts to avoid infinite loop
        int attempts = 0;
        bool isPlaced = false;

        while (!isPlaced && attempts < maxAttempts)
        {
            attempts++;
            // Get random starting position
            int startRow = Random.Range(0, Rows - height + 1);
            int startCol = Random.Range(0, Columns - width + 1);

            // Check if the space is free
            if (CanPlaceRune(startRow, startCol, height, width))
            {
              //  Debug.Log($"Placing {rune} at ({startRow}, {startCol}).");
                // Place the rune
                PlaceRune(startRow, startCol, height, width, rune, groupId);
                isPlaced = true;
            }
        }
    }

    private bool CanPlaceRune(int startRow, int startCol, int height, int width)
    {
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (Table[startRow + row, startCol + col].RuneType != RuneType.None)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceRune(int startRow, int startCol, int height, int width, RuneType rune, int groupId)
    {
        Debug.Log($"Placing rune of type {rune} in group {groupId}.");
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                /*Table[startRow + row, startCol + col] = rune;*/
                Table[startRow + row, startCol + col] = new RuneMap()
                {
                    RuneType = rune,
                    Group = groupId
                };
            }
        }

        RuneMap runeMap = new RuneMap { RuneType = rune, Group = groupId };
        runeMaps.Add(runeMap);
    }

    public int GetRuneHeight(RuneType gem)
    {
        return gemSize.ContainsKey(gem) ? gemSize[gem].height : 1;
    }

    public int GetRuneWidth(RuneType gem)
    {
        return gemSize.ContainsKey(gem) ? gemSize[gem].width : 1;
    }

    public List<RuneMap> GetRuneMaps()
    {
        return runeMaps;
    }
}