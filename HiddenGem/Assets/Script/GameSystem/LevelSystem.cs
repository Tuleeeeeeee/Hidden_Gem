using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tuleeeeee.Enum;
using Tuleeeeee.Features.Mission;
using Tuleeeeee.GameData;
using Tuleeeeee.Runes;
using UnityEngine;

namespace Tuleeeeee.GameSystem
{
    public class LevelSystem : SerializedMonoBehaviour
    {
        public static LevelSystem Instance;

        [Header("Data configs")] [SerializeField]
        private ManagerGameLevel _managerGameLevel;

        [SerializeField] private MissionDestinationData _missionDestinationData;

        private LevelData currentLevelData;

        [TableMatrix, SerializeField] private RuneType[,] cloneTable;

        [SerializeField] private Tile tilePrefab;
        [SerializeField] private List<Rune> runePrefab;
        public MissionDestination currentMissionDestination;

        private List<Tile> tiles = new List<Tile>();
        private List<Rune> runPrefabsIntances = new List<Rune>();

        private int level = 1;

        private bool isHorizontal; 
        private bool isVertical;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GenerateLevel(level);
            GenerateMission(level);
        }

        private void GenerateMission(int level)
        {
            var prefab = _missionDestinationData.GetMissionDestination(level);
            currentMissionDestination = Instantiate(prefab);
        }

        private void GenerateLevel(int level)
        {
            currentLevelData = _managerGameLevel.GetLevel(level);
            cloneTable = (RuneType[,])currentLevelData.Table.Clone();

            // create Tile map
            GenerateTile();

            // create Rune map
            GenerateRune();
        }

        private void GenerateRune()
        {
            foreach (var rune in runePrefab)
            {
                var listTile = new List<Tile>();
                listTile = GetTileHideRune(rune.RuneType);
                Debug.Log($"Checking Rune {rune.RuneType}:");
                if (listTile.Count > 0)
                {
                    int totalRows = 0;
                    int totalColumns = 0;

                    isHorizontal = true;
                    isVertical = true;

                    int minRow = listTile[0].row;
                    int maxRow = listTile[0].row;
                    int minColumn = listTile[0].column;
                    int maxColumn = listTile[0].column;


                    for (int i = 0; i < listTile.Count; i++)
                    {
                        int currentRow = listTile[i].row;
                        int currentColumn = listTile[i].column;

                        totalRows += listTile[i].row;
                        totalColumns += listTile[i].column;


                        Debug.Log($"{listTile[i].row} , {listTile[i].column}");


                        minRow = Mathf.Min(minRow, currentRow);
                        maxRow = Mathf.Max(maxRow, currentRow);
                        minColumn = Mathf.Min(minColumn, currentColumn);
                        maxColumn = Mathf.Max(maxColumn, currentColumn);


                        if (listTile[i].row != listTile[0].row)
                        {
                            isHorizontal = false;
                        }

                        else if (listTile[i].column != listTile[0].column)
                        {
                            isVertical = false;
                        }
                    }

                    int middleRow = totalRows / listTile.Count;
                    int middleColumn = totalColumns / listTile.Count;

                    int rowDifference = maxRow - minRow + 1;
                    int columnDifference = maxColumn - minColumn + 1;

                    Debug.Log($"Middle Row: {middleRow}, Middle Column: {middleColumn}");

                    if (rowDifference == columnDifference)
                    {
                        Debug.Log($"Rune {rune.RuneType} is a squere {rowDifference}X{columnDifference}.");
                        var runeObject =
                            Instantiate(rune, GetCenterPosition(listTile),
                                Quaternion.identity); // Xoay 90 độ quanh trục Z;
                        runPrefabsIntances.Add(runeObject);
                    }
                    else if (isHorizontal)
                    {
                        Debug.Log($"Rune {rune.RuneType} is aligned horizontally.");
                        var runeObject = Instantiate(rune, new Vector3(middleColumn, middleRow),
                            Quaternion.Euler(0, 0, 90)); // Xoay 90 độ quanh trục Z;
                        runPrefabsIntances.Add(runeObject);
                    }
                    else if (isVertical)
                    {
                        Debug.Log($"Rune {rune.RuneType} is aligned vertically.");
                        var runeObject = Instantiate(rune, new Vector3(middleColumn, middleRow), Quaternion.identity);
                        runPrefabsIntances.Add(runeObject);
                    }
                    else
                    {
                        Debug.Log($"Rune {rune.RuneType} is neither purely horizontal nor vertical.");
                    }
                }
            }
        }

        float CalculateCenterRow(List<Tile> tiles)
        {
            int minRow = tiles[0].row;
            int maxRow = tiles[0].row;

            foreach (var tile in tiles)
            {
                minRow = Mathf.Min(minRow, tile.row);
                maxRow = Mathf.Max(maxRow, tile.row);
            }

            return (minRow + maxRow) / 2f; // Center Row
        }

        float CalculateCenterColumn(List<Tile> tiles)
        {
            int minColumn = tiles[0].column;
            int maxColumn = tiles[0].column;

            foreach (var tile in tiles)
            {
                minColumn = Mathf.Min(minColumn, tile.column);
                maxColumn = Mathf.Max(maxColumn, tile.column);
            }

            return (minColumn + maxColumn) / 2f; // Center Column
        }

        Vector3 GetCenterPosition(List<Tile> tiles, float tileSize = 1f)
        {
            float centerRow = CalculateCenterRow(tiles);
            float centerColumn = CalculateCenterColumn(tiles);

            // Adjust for world space if needed (tileSize)
            float worldX = centerColumn * tileSize;
            float worldY = centerRow * tileSize;

            return new Vector3(worldX, worldY, 0f);
        }

        private List<Tile> GetTileHideRune(RuneType runeRuneType)
        {
            return tiles.Where(x => x.runeType == runeRuneType).ToList();
        }

        private void GenerateTile()
        {
            for (int col = 0; col < currentLevelData.Columns; col++)
            {
                for (int row = 0; row < currentLevelData.Rows; row++)
                {
                    var tile = Instantiate(tilePrefab, new Vector3(col, row, 0), Quaternion.identity);
                    tile.Init(col, row, currentLevelData.Table[col, row]);
                    tiles.Add(tile);
                }
            }
        }

        public void Crusher(int column, int row, RuneType type)
        {
            if (row < 0 || row >= currentLevelData.Rows || column < 0 || column >= currentLevelData.Columns)
            {
                return;
            }

            if (currentLevelData.Table[column, row] == RuneType.None)
            {
                return;
            }

            cloneTable[column, row] = RuneType.None;

            if (IsClearType(type))
            {
                // set animation fly
                Debug.Log("Fly");
                foreach (var rune in runPrefabsIntances)
                {
                    if (rune.RuneType == type)
                    {
                        rune.FlyToDestination();
                    }
                }
            }
        }

        public bool IsClearType(RuneType type)
        {
            foreach (var cell in cloneTable)
            {
                if (cell == type)
                    return false;
            }

            return true;
        }

        public Transform GetDestination(RuneType runeType)
        {
            return currentMissionDestination.GetDestination(runeType);
        }
    }
}