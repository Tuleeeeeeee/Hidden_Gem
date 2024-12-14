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
                if (listTile.Count == 0)
                    continue;

                Debug.Log($"Checking Rune {rune.RuneType}:");

                BoundsInt bounds = CalculateBounds(listTile, out isHorizontal, out isVertical);

                Vector3 centerPosition = GetCenterPosition(bounds);
                HandleRunePlacement(rune, bounds, isHorizontal, isVertical, centerPosition);
            }
        }

        private BoundsInt CalculateBounds(List<Tile> listTile, out bool isHorizontal, out bool isVertical)
        {
            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;

            isHorizontal = true;
            isVertical = true;

            int baseRow = listTile[0].row;
            int baseColumn = listTile[0].column;

            foreach (var tile in listTile)
            {
                minRow = Mathf.Min(minRow, tile.row);
                maxRow = Mathf.Max(maxRow, tile.row);
                minColumn = Mathf.Min(minColumn, tile.column);
                maxColumn = Mathf.Max(maxColumn, tile.column);

                if (tile.row != baseRow)
                    isHorizontal = false;
                if (tile.column != baseColumn)
                    isVertical = false;

                Debug.Log($"Tile Position: Row {tile.row}, Column {tile.column}");
            }

            return new BoundsInt(new Vector3Int(minColumn, minRow, 0),
                new Vector3Int(maxColumn - minColumn + 1, maxRow - minRow + 1, 0));
        }

        private void HandleRunePlacement(Rune rune, BoundsInt bounds, bool isHorizontal, bool isVertical,
            Vector3 centerPosition)
        {
            if (bounds.size.x == bounds.size.y)
            {
                Debug.Log($"Rune {rune.RuneType} is a square {bounds.size.x}x{bounds.size.y}.");
                InstantiateRune(rune, centerPosition, Quaternion.identity);
            }
            else if (isHorizontal)
            {
                Debug.Log($"Rune {rune.RuneType} is aligned horizontally.");
                InstantiateRune(rune, centerPosition, Quaternion.Euler(0, 0, 90));
            }
            else if (isVertical)
            {
                Debug.Log($"Rune {rune.RuneType} is aligned vertically.");
                InstantiateRune(rune, centerPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log($"Rune {rune.RuneType} is neither purely horizontal nor vertical.");
            }
        }

        private void InstantiateRune(Rune rune, Vector3 position, Quaternion rotation)
        {
            var runeObject = Instantiate(rune, position, rotation);
            runPrefabsIntances.Add(runeObject);
        }

        private Vector3 GetCenterPosition(BoundsInt bounds)
        {
            float centerRow = bounds.min.y + (bounds.size.y - 1) / 2f;
            float centerColumn = bounds.min.x + (bounds.size.x - 1) / 2f;

            return new Vector3(centerColumn, -centerRow, 0);
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
                    var tile = Instantiate(tilePrefab, new Vector3(col, -row, 0), Quaternion.identity);
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