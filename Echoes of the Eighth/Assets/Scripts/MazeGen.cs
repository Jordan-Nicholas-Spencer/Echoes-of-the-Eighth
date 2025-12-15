using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a maze using individual Decrepit Dungeon LITE components:
/// one floor tile per cell, walls around edges, doorways (and doors) where cells connect.
/// Put this on an empty GameObject, assign floor / wall / doorway / door prefabs, hit Play.
/// Offsets are exposed so you can line things up exactly how you like.
/// </summary>
public class DecrepitComponentMazeBuilder : MonoBehaviour
{
    [Header("Decrepit Dungeon Components")]
    [Tooltip("Square floor tile from Prefabs/Floors (e.g. Floor_A).")]
    public GameObject floorPrefab;

    [Tooltip("Straight wall segment from Prefabs/Walls (e.g. Wall_A).")]
    public GameObject wallPrefab;

    [Tooltip("Wall piece with an opening from Prefabs/Doorways (or Arches).")]
    public GameObject doorwayPrefab;

    [Tooltip("Optional: door model from Prefabs/Doors. Used ONLY at entrance & exit.")]
    public GameObject doorPrefab;

    [Header("Snapping / Grid")]
    [Tooltip("Distance between cell centers (horizontal). Match this to your floor tile size.")]
    public float tileSize = 3.5f;

    [Tooltip("Auto-compute tileSize from floor mesh bounds (horizontal X/Z).")]
    public bool autoTileSizeFromFloor = true;

    [Header("Offsets (tune these to fix alignment)")]
    [Tooltip("Offset from cell CENTER to where the floor pivot should be placed.")]
    public Vector3 floorOffset = Vector3.zero;

    [Tooltip("Offset from EDGE MIDPOINT to where the wall pivot should be placed.")]
    public Vector3 wallOffset = Vector3.zero;

    [Tooltip("Offset from EDGE MIDPOINT to where the doorway pivot should be placed.")]
    public Vector3 doorwayOffset = Vector3.zero;

    [Tooltip("Local offset of the door relative to the doorway pivot (entrance/exit only).")]
    public Vector3 doorLocalOffset = Vector3.zero;

    [Header("Maze Size")]
    public int gridWidth = 20;
    public int gridHeight = 20;

    [Header("Layout Controls")]
    [Range(0f, 1f)]
    [Tooltip("Extra connections to create loops (0 = tree, 1 = very loopy).")]
    public float loopChance = 0.12f;

    [Tooltip("Minimum number of branch paths off the main route. (Not used in DFS version, kept for compatibility)")]
    public int minBranches = 6;

    [Tooltip("Maximum number of branch paths off the main route. (Not used in DFS version, kept for compatibility)")]
    public int maxBranches = 12;

    [Header("Root object")]
    public string dungeonRootName = "Decrepit_Maze";
    public Vector3 originOffset = Vector3.zero;
    public bool generateOnStart = true;

    private Transform dungeonRoot;

    // --- internal cell representation ---
    private class Cell
    {
        public Vector2Int pos;
        public List<Cell> neighbors = new List<Cell>();
    }

    private readonly Dictionary<Vector2Int, Cell> grid = new Dictionary<Vector2Int, Cell>();

    // --- Entrance / Exit info ---
    private Vector2Int entranceCellPos = new Vector2Int(-1, -1);
    private Vector2Int entranceDir = Vector2Int.down;   // direction pointing OUT of the maze

    private Vector2Int exitCellPos = new Vector2Int(-1, -1);
    private Vector2Int exitDir = Vector2Int.up;         // direction pointing OUT of the maze

    private Cell startCell;
    private Cell endCell;

    // Optional public accessors (for spawning player, etc.)
    public Vector2Int EntranceCell => entranceCellPos;
    public Vector2Int ExitCell => exitCellPos;

    private void Start()
    {
        if (generateOnStart)
            GenerateMaze();
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Maze Now (Editor)")]
    private void GenerateMazeInEditor()
    {
        GenerateMaze();
    }
#endif

    public void GenerateMaze()
    {
        if (floorPrefab == null || wallPrefab == null || doorwayPrefab == null)
        {
            Debug.LogError("DecrepitComponentMazeBuilder: Assign floorPrefab, wallPrefab, and doorwayPrefab before generating the maze.");
            return;
        }

        if (autoTileSizeFromFloor)
        {
            AutoTileSizeFromFloorMesh();
        }

        ClearOld();
        CreateRoot();
        BuildGrid();
        BuildMazeWithDFSAndEntrances();  // New: guaranteed connected maze with entrance & exit
        AddLoops();                      // Optional: extra loops to make it less tree-like
        InstantiateFloors();
        InstantiateEdges();

        Debug.Log($"DecrepitComponentMazeBuilder: Maze generated. Entrance at {entranceCellPos}, exit at {exitCellPos}.");
    }

    // ---------- auto tile size from floor mesh ----------

    private void AutoTileSizeFromFloorMesh()
    {
        var mf = floorPrefab.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        var bounds = mf.sharedMesh.bounds;
        // assume roughly square tile; pick max horizontal dimension
        float sizeX = bounds.size.x;
        float sizeZ = bounds.size.z;
        tileSize = Mathf.Max(sizeX, sizeZ);
    }

    // ---------- setup / cleanup ----------

    private void ClearOld()
    {
        if (dungeonRoot == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(dungeonRoot.gameObject);
        else
            Destroy(dungeonRoot.gameObject);
#else
        Destroy(dungeonRoot.gameObject);
#endif
    }

    private void CreateRoot()
    {
        GameObject root = new GameObject(dungeonRootName);
        dungeonRoot = root.transform;
        dungeonRoot.SetParent(transform, false);
        dungeonRoot.localPosition = originOffset;
        dungeonRoot.localRotation = Quaternion.identity;
    }

    // ---------- maze graph generation ----------

    private void BuildGrid()
    {
        grid.Clear();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var cell = new Cell { pos = new Vector2Int(x, y) };
                grid[cell.pos] = cell;
            }
        }
    }

    // ---------- new DFS-based maze with guaranteed path & entrance/exit ----------

    private void BuildMazeWithDFSAndEntrances()
    {
        // 1. Choose a random perimeter cell as the entrance root for the maze.
        (Vector2Int pos, Vector2Int dirOut) entrance = ChooseRandomPerimeterCell();
        entranceCellPos = entrance.pos;
        entranceDir = entrance.dirOut;

        startCell = grid[entranceCellPos];

        // 2. Standard DFS (recursive backtracker) to carve a perfect maze (tree).
        HashSet<Cell> visited = new HashSet<Cell>();
        Stack<Cell> stack = new Stack<Cell>();

        visited.Add(startCell);
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            Cell current = stack.Peek();
            List<Cell> unvisitedNeighbors = GetUnvisitedNeighbors(current, visited);

            if (unvisitedNeighbors.Count == 0)
            {
                stack.Pop(); // backtrack
            }
            else
            {
                Cell next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                Connect(current.pos, next.pos);
                visited.Add(next);
                stack.Push(next);
            }
        }

        // 3. BFS from start to find the farthest border cell as exit (long path).
        PickExitCell();
    }

    private List<Cell> GetUnvisitedNeighbors(Cell cell, HashSet<Cell> visited)
    {
        List<Cell> result = new List<Cell>();

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            Vector2Int nPos = cell.pos + d;
            if (grid.TryGetValue(nPos, out Cell neighbor) && !visited.Contains(neighbor))
            {
                result.Add(neighbor);
            }
        }

        return result;
    }

    private (Vector2Int pos, Vector2Int dirOut) ChooseRandomPerimeterCell()
    {
        int side = Random.Range(0, 4);

        switch (side)
        {
            // bottom edge: y = 0, outside is "down"
            case 0:
                return (new Vector2Int(Random.Range(0, gridWidth), 0), Vector2Int.down);

            // top edge: y = gridHeight - 1, outside is "up"
            case 1:
                return (new Vector2Int(Random.Range(0, gridWidth), gridHeight - 1), Vector2Int.up);

            // left edge: x = 0, outside is "left"
            case 2:
                return (new Vector2Int(0, Random.Range(0, gridHeight)), Vector2Int.left);

            // right edge: x = gridWidth - 1, outside is "right"
            default:
                return (new Vector2Int(gridWidth - 1, Random.Range(0, gridHeight)), Vector2Int.right);
        }
    }

    private void PickExitCell()
    {
        // BFS from startCell over the DFS-generated tree.
        Queue<Cell> q = new Queue<Cell>();
        Dictionary<Cell, int> dist = new Dictionary<Cell, int>();

        q.Enqueue(startCell);
        dist[startCell] = 0;

        Cell farthest = startCell;
        int farthestDist = 0;

        Cell farthestBorder = null;
        int farthestBorderDist = -1;

        while (q.Count > 0)
        {
            Cell c = q.Dequeue();
            int d = dist[c];

            if (d > farthestDist)
            {
                farthestDist = d;
                farthest = c;
            }

            if (IsBorder(c.pos) && d > farthestBorderDist)
            {
                farthestBorder = c;
                farthestBorderDist = d;
            }

            foreach (Cell n in c.neighbors)
            {
                if (!dist.ContainsKey(n))
                {
                    dist[n] = d + 1;
                    q.Enqueue(n);
                }
            }
        }

        // Prefer a border cell as the exit, otherwise just use the farthest cell.
        endCell = farthestBorder ?? farthest;
        exitCellPos = endCell.pos;
        exitDir = GetOutwardDirection(exitCellPos);
    }

    private bool IsBorder(Vector2Int pos)
    {
        return pos.x == 0 || pos.y == 0 || pos.x == gridWidth - 1 || pos.y == gridHeight - 1;
    }

    private Vector2Int GetOutwardDirection(Vector2Int pos)
    {
        // For border cells, return a direction that points OUT of the maze.
        if (pos.y == 0) return Vector2Int.down;
        if (pos.y == gridHeight - 1) return Vector2Int.up;
        if (pos.x == 0) return Vector2Int.left;
        if (pos.x == gridWidth - 1) return Vector2Int.right;

        // Fallback (shouldn't happen if we pick border cells).
        return Vector2Int.up;
    }

    private void AddLoops()
    {
        foreach (var cell in grid.Values)
        {
            if (Random.value < loopChance)
            {
                Vector2Int neighborPos = cell.pos + RandomDirection();
                if (grid.ContainsKey(neighborPos))
                    Connect(cell.pos, neighborPos);
            }
        }
    }

    private void Connect(Vector2Int aPos, Vector2Int bPos)
    {
        if (!grid.ContainsKey(aPos) || !grid.ContainsKey(bPos)) return;

        Cell a = grid[aPos];
        Cell b = grid[bPos];

        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
    }

    private Vector2Int RandomDirection()
    {
        switch (Random.Range(0, 4))
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            default: return Vector2Int.right;
        }
    }

    // ---------- instantiation ----------

    private void InstantiateFloors()
    {
        foreach (var cell in grid.Values)
        {
            Vector3 pos = GridToLocal(cell.pos);
            GameObject floor = Instantiate(floorPrefab, dungeonRoot);
            floor.name = $"Floor_{cell.pos.x}_{cell.pos.y}";
            floor.transform.localPosition = pos + floorOffset;
            floor.transform.localRotation = Quaternion.identity;
        }
    }

    private void InstantiateEdges()
    {
        HashSet<string> processedEdges = new HashSet<string>();

        foreach (var cell in grid.Values)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var d in dirs)
            {
                Vector2Int otherPos = cell.pos + d;
                string key = EdgeKey(cell.pos, otherPos);
                if (processedEdges.Contains(key))
                    continue;

                processedEdges.Add(key);

                Vector3 dirVec = DirToVector3(d);
                Vector3 aLocal = GridToLocal(cell.pos);
                Vector3 mid;
                bool otherInGrid = grid.ContainsKey(otherPos);

                if (otherInGrid)
                {
                    // Interior edge between two cells
                    Vector3 bLocal = GridToLocal(otherPos);
                    mid = (aLocal + bLocal) * 0.5f;

                    Cell otherCell = grid[otherPos];
                    bool connected = cell.neighbors.Contains(otherCell);

                    if (connected)
                    {
                        // Interior connection: doorway ONLY (no door)
                        GameObject dw = Instantiate(doorwayPrefab, dungeonRoot);
                        dw.name = $"Doorway_{cell.pos.x}_{cell.pos.y}_{otherPos.x}_{otherPos.y}";
                        dw.transform.localPosition = mid + doorwayOffset;
                        dw.transform.localRotation = Quaternion.LookRotation(dirVec, Vector3.up);

                        // NOTE: no doorPrefab here; interior is always open.
                    }
                    else
                    {
                        // solid wall between two unconnected cells
                        GameObject wall = Instantiate(wallPrefab, dungeonRoot);
                        wall.name = $"Wall_{cell.pos.x}_{cell.pos.y}_{otherPos.x}_{otherPos.y}";
                        wall.transform.localPosition = mid + wallOffset;
                        wall.transform.localRotation = Quaternion.LookRotation(dirVec, Vector3.up);
                    }
                }
                else
                {
                    // Outer boundary (outside the grid)
                    mid = aLocal + DirToVector3(d) * (tileSize * 0.5f);

                    bool isEntrance = (cell.pos == entranceCellPos && d == entranceDir);
                    bool isExit = (cell.pos == exitCellPos && d == exitDir);

                    if (isEntrance || isExit)
                    {
                        // Boundary entrance/exit doorway WITH door
                        GameObject dw = Instantiate(doorwayPrefab, dungeonRoot);
                        dw.name = isEntrance
                            ? $"Entrance_{cell.pos.x}_{cell.pos.y}"
                            : $"Exit_{cell.pos.x}_{cell.pos.y}";
                        dw.transform.localPosition = mid + doorwayOffset;
                        dw.transform.localRotation = Quaternion.LookRotation(dirVec, Vector3.up);

                        if (doorPrefab != null)
                        {
                            GameObject door = Instantiate(doorPrefab, dw.transform);
                            door.name = "Door";
                            door.transform.localPosition = doorLocalOffset;
                            door.transform.localRotation = Quaternion.identity;
                        }
                    }
                    else
                    {
                        // Solid border wall everywhere else.
                        GameObject wall = Instantiate(wallPrefab, dungeonRoot);
                        wall.name = $"Wall_{cell.pos.x}_{cell.pos.y}_Border";
                        wall.transform.localPosition = mid + wallOffset;
                        wall.transform.localRotation = Quaternion.LookRotation(dirVec, Vector3.up);
                    }
                }
            }
        }
    }

    // ---------- helpers ----------

    private string EdgeKey(Vector2Int a, Vector2Int b)
    {
        if (a.x < b.x || (a.x == b.x && a.y <= b.y))
            return $"{a.x},{a.y}-{b.x},{b.y}";
        return $"{b.x},{b.y}-{a.x},{a.y}";
    }

    private Vector3 GridToLocal(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, 0f, gridPos.y * tileSize);
    }

    private Vector3 DirToVector3(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return new Vector3(0, 0, 1);
        if (dir == Vector2Int.down) return new Vector3(0, 0, -1);
        if (dir == Vector2Int.left) return new Vector3(-1, 0, 0);
        return new Vector3(1, 0, 0);
    }
}