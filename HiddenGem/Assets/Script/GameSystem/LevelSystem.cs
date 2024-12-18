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

// A list to hold all instantiated runes
        private List<Rune> allRunes = new List<Rune>();


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

            levelModel.CloneTable = (RuneType[,])currentLevelData.Table.Clone();

            levelModel.CrushCount = currentLevelData.CrushCount;

            // create Tile map
            GenerateTile();

            // create Rune map
            GenerateRune();
        }

        private void GenerateRune()
        {
            foreach (var rune in currentLevelData.runePrefab)
            {
                // Access all RuneMap objects at once (since we don't need to call GetRuneMaps every loop iteration)
                List<RuneMap> runeMaps = currentLevelData.GetRuneMaps();

                // Iterate through each RuneMap
                foreach (var runeMap in runeMaps)
                {
                    // Only proceed if the RuneType matches the current rune being processed
                    if (runeMap.RuneType == rune.RuneType)
                    {
                        Debug.Log($"Rune Type: {runeMap.RuneType}, Group: {runeMap.Group}");

                        // Now, proceed to calculate bounds and handle placement
                        List<Tile>
                            listTile = GetTileHideRune(runeMap.RuneType,
                                runeMap.Group); // You need a method to get tiles for the group
                        BoundsInt bounds = CalculateBounds(listTile);
                        Debug.Log($"Rune bounds: {bounds.size}");

                        Vector3 centerPosition = GetCenterPosition(bounds);
                        Debug.Log($"Center Position: {centerPosition}");

                        // Handle the rune placement based on its bounds
                        HandleRunePlacement(rune, bounds, centerPosition);
                    }
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

        private void HandleRunePlacement(Rune rune, BoundsInt bounds, Vector3 centerPosition)
        {
            int width = bounds.size.x;
            int height = bounds.size.y;

            if (width == height)
            {
                Debug.Log($"Rune {rune.RuneType} is a square {bounds.size.x}x{bounds.size.y}.");
                InstantiateRune(rune, centerPosition, Quaternion.identity);
            }
            else if (width > height)
            {
                Debug.Log($"Rune {rune.RuneType} is aligned horizontally.");
                InstantiateRune(rune, centerPosition, Quaternion.Euler(0, 0, 90));
            }
            else if (width < height)
            {
                Debug.Log($"Rune {rune.RuneType} is aligned vertically.");
                InstantiateRune(rune, centerPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log($"Rune {rune.RuneType} is neither horizontal nor vertical.");
            }
        }

        private void InstantiateRune(Rune rune, Vector3 position, Quaternion rotation)
        {
            Rune newRune = Instantiate(rune, position, rotation);
            newRune.Init(rune.RuneType, Vector3.one, runeIdCounter++);
            allRunes.Add(newRune);
        }

        private Vector3 GetCenterPosition(BoundsInt bounds)
        {
            float centerRow = bounds.min.x + (bounds.size.x - 1) / 2f;
            float centerColumn = bounds.min.y + (bounds.size.y - 1) / 2f;

            return new Vector3(centerRow, -centerColumn, 0);
        }

        /*private List<List<Tile>> GetTileGroupsByRuneType(RuneType targetType)
        {
            var visited = new bool[currentLevelData.Rows, currentLevelData.Columns];
            var tileGroups = new List<List<Tile>>();

            for (int row = 0; row < currentLevelData.Rows; row++)
            {
                for (int col = 0; col < currentLevelData.Columns; col++)
                {
                    // Skip if not the target RuneType or already visited
                    if (currentLevelData.Table[row, col] != targetType || visited[row, col])
                        continue;

                    // Perform flood fill and collect connected tiles
                    var group = new List<Tile>();
                    FloodFill(row, col, targetType, visited, group);
                    if (group.Count > 0)
                        tileGroups.Add(group);
                }
            }

            return tileGroups;
        }*/

        /*private void FloodFill(int row, int col, RuneType targetType, bool[,] visited, List<Tile> group)
        {
            // Check bounds and already visited
            if (row < 0 || row >= currentLevelData.Rows || col < 0 || col >= currentLevelData.Columns ||
                visited[row, col])
                return;

            // Check if the tile matches the RuneType
            if (currentLevelData.Table[row, col] != targetType)
                return;

            // Mark the tile as visited and add it to the group
            visited[row, col] = true;
            var tile = tiles.FirstOrDefault(t => t.row == row && t.column == col);
            if (tile != null)
                group.Add(tile);

            // Recursive calls to check neighboring tiles
            FloodFill(row + 1, col, targetType, visited, group); // Down
            FloodFill(row - 1, col, targetType, visited, group); // Up
            FloodFill(row, col + 1, targetType, visited, group); // Right
            FloodFill(row, col - 1, targetType, visited, group); // Left
        }*/

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
                    RuneMap matchingRuneMap =
                        currentLevelData.runeMaps.FirstOrDefault(rm =>
                            rm.RuneType == currentLevelData.Table[row, col]);
                    int group = matchingRuneMap != null ? matchingRuneMap.Group : -1;

                    var tile = Instantiate(currentLevelData.tilePrefab, new Vector3(row, -col, 0), Quaternion.identity);
                    tile.Init(row, col, currentLevelData.Table[row, col], group);
                    tile.transform.SetParent(tileContainer.transform);
                    tiles.Add(tile);
                    Debug.Log($"Tile ID: {tile.id}, Group: {group}");
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

            if (currentLevelData.Table[row, column] == RuneType.None)
            {
                return;
            }

            levelModel.CloneTable[row, column] = RuneType.None;
            levelModel.CrushCount--;

            if (IsClearType(type))
            {
                OnClearRune?.Invoke(type, id);
                return;
            }
        }

        bool IsClearType(RuneType type)
        {
            foreach (var cell in levelModel.CloneTable)
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