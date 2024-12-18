using System.Collections.Generic;
using System.Linq;
using Tuleeeeee.Delegate;
using Sirenix.OdinInspector;
using Tuleeeeee.Enum;
using Tuleeeeee.Features.Mission;
using Tuleeeeee.GameData;
using Tuleeeeee.Model;
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

        private LevelModel levelModel;

        [TableMatrix, SerializeField] private RuneType[,] cloneTable;


        public MissionDestination currentMissionDestination;

        private List<Tile> tiles = new List<Tile>();


        private int level = 1;
        private int runeIdCounter = 0;
        private int tileIdCounter = 0;
        private OnClearRune OnClearRune;
        private OnLevelClear OnLevelClear;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            levelModel = new LevelModel();
            GenerateLevel(level);
            GenerateMission(level);
        }

        public void RegisterOnClearRune(OnClearRune onClearRune)
        {
            OnClearRune += onClearRune;
        }

        public void UnRegisterOnClearRune(OnClearRune onClearRune)
        {
            OnClearRune -= onClearRune;
        }

        public void RegisterOnLevelClear(OnLevelClear onLevelClear)
        {
            OnLevelClear += onLevelClear;
        }

        public void UnRegisterOnLevelClear(OnLevelClear onLevelClear)
        {
            OnLevelClear -= onLevelClear;
        }

        private void GenerateMission(int level)
        {
            var prefab = _missionDestinationData.GetMissionDestination(level);
            currentMissionDestination = Instantiate(prefab, new Vector3(0, 7, 0), Quaternion.identity);
        }

        private void GenerateLevel(int level)
        {
            currentLevelData = _managerGameLevel.GetLevel(level);
            
            currentLevelData.CreateTable();
            currentLevelData.RandomRunes();
            
            
            levelModel.CloneTable = (RuneMap[,])currentLevelData.Table.Clone();

            levelModel.CrushCount = currentLevelData.CrushCount;

            // create Tile map
            GenerateTile();

            // create Rune map
            GenerateRune();
        }

        private void GenerateRune()
        {
            List<RuneMap> runeMaps = currentLevelData.GetRuneMaps();
            foreach (var rune in currentLevelData.runePrefab)
            {
                Debug.Log($"Checking Rune {rune.RuneType}:");
                foreach (var runMap in runeMaps)
                {
                    List<Tile> listTile = GetTileHideRune(rune.RuneType, runMap.Group);
                    if (listTile.Count == 0)
                        continue;
                    Debug.Log($"Checking Rune {rune.RuneType} in list:");
                    Debug.Log($"Caculate Rune {rune.RuneType} bound:");
                    BoundsInt bounds = CalculateBounds(listTile);
                    Debug.Log($"Rune bounds: {bounds.size}");
                    Vector3 centerPosition = GetCenterPosition(bounds);
                    Debug.Log($"Center Position: {centerPosition}");
                    // Handle the rune placement based on its bounds
                    HandleRunePlacement(rune, bounds, centerPosition, runMap.Group);
                }
            }
        }

        private BoundsInt CalculateBounds(List<Tile> listTile)
        {
            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;

            int baseRow = listTile[0].row;
            int baseColumn = listTile[0].column;

            foreach (var tile in listTile)
            {
                minRow = Mathf.Min(minRow, tile.row);
                maxRow = Mathf.Max(maxRow, tile.row);

                minColumn = Mathf.Min(minColumn, tile.column);
                maxColumn = Mathf.Max(maxColumn, tile.column);

                Debug.Log($"Tile Position: Row {tile.row}, Column {tile.column}, id {tile.id}");
            }

            return new BoundsInt(new Vector3Int(minRow, minColumn, 0),
                new Vector3Int(maxRow - minRow + 1, maxColumn - minColumn + 1, 0));
        }

        private void HandleRunePlacement(Rune rune, BoundsInt bounds, Vector3 centerPosition, int id)
        {
            int width = bounds.size.x;
            int height = bounds.size.y;

            if (width == height)
            {
                Debug.Log($"Rune {rune.RuneType} is a square {bounds.size.x}x{bounds.size.y}.");
                InstantiateRune(rune, centerPosition, Quaternion.identity, id);
            }
            else if (width > height)
            {
                Debug.Log($"Rune {rune.RuneType} is aligned horizontally.");
                InstantiateRune(rune, centerPosition, Quaternion.Euler(0, 0, 90), id);
            }
            else if (width < height)
            {
                Debug.Log($"Rune {rune.RuneType} is aligned vertically.");
                InstantiateRune(rune, centerPosition, Quaternion.identity, id);
            }
            else
            {
                Debug.Log($"Rune {rune.RuneType} is neither horizontal nor vertical.");
            }
        }

        private void InstantiateRune(Rune rune, Vector3 position, Quaternion rotation, int id)
        {
            Rune newRune = Instantiate(rune, position, rotation);
            newRune.Init(rune.RuneType, Vector3.one, id);
        }

        private Vector3 GetCenterPosition(BoundsInt bounds)
        {
            float centerRow = bounds.min.x + (bounds.size.x - 1) / 2f;
            float centerColumn = bounds.min.y + (bounds.size.y - 1) / 2f;

            return new Vector3(centerRow, -centerColumn, 0);
        }
        private List<Tile> GetTileHideRune(RuneType runeRuneType, int groupId)
        {
            return tiles.Where(x => x.runeType == runeRuneType && x.id == groupId).ToList();
        }

        private void GenerateTile()
        {
            var tileContainer = new GameObject("TileContainer");
            tileContainer.transform.SetParent(transform);
            for (int row = 0; row < currentLevelData.Rows; row++)
            {
                for (int col = 0; col < currentLevelData.Columns; col++)
                {
                    var tile = Instantiate(currentLevelData.tilePrefab, new Vector3(row, -col, 0),
                        Quaternion.identity);
                    tile.Init(row, col, currentLevelData.Table[row, col].RuneType,
                        currentLevelData.Table[row, col].Group);
                    tile.transform.SetParent(tileContainer.transform);
                    tiles.Add(tile);
                }
            }
        }

        void CenterParent(int row, int col, Transform parentTransform)
        {
            float totalWidth = row;
            // float totalHeight = col;
            float offsetX = (totalWidth - 1f) / 2f;


            parentTransform.position = new Vector3(-offsetX, 0, parentTransform.position.z);
        }


        public void Crusher(int row, int column, RuneType type, int id)
        {
            if (row < 0 || row >= currentLevelData.Rows || column < 0 || column >= currentLevelData.Columns)
            {
                return;
            }

            if (currentLevelData.Table[row, column].RuneType == RuneType.None)
            {
                return;
            }

            levelModel.CloneTable[row, column].RuneType = RuneType.None;
            levelModel.CrushCount--;

            if (IsClearType(type, id))
            {
                OnClearRune?.Invoke(type, id);
                return;
            }
        }

        bool IsClearType(RuneType type, int id)
        {
            foreach (var cell in levelModel.CloneTable)
            {
                if (cell.RuneType == type && cell.Group == id)
                    return false;
            }

            return true;
        }
        public List<RuneMap> GetRuneMaps()
        {
            return currentLevelData.GetRuneMaps();
        }
        public Transform GetDestination(RuneType runeType, int id)
        {
            return currentMissionDestination.GetDestination(runeType, id);
        }
        public void CheckLevelClear()
        {
            if (!levelModel.HasClear()) return;
            Debug.Log("Level clear");
            OnLevelClear?.Invoke();
            // clear curent level
            DestroyRestTiles();
            level++;
        }

        private void DestroyRestTiles()
        {
        }
    }
}