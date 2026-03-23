using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Board : MonoBehaviour
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private ItemBase[,] items;
    private Vector3 gridOrigin;
    private const float CellSize = 1f;
    private const float FallSpeed = 12f;
    private const float BlastDuration = 0.15f;
    private const float MergeDuration = 0.2f;

    private bool isProcessing;
    private int movesLeft;
    private int totalObstacles;

    public event Action<int> OnMovesChanged;
    public event Action OnWin;
    public event Action OnLose;

    private static readonly Vector2Int[] Directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public void Initialize(LevelData data)
    {
        Width = data.grid_width;
        Height = data.grid_height;
        movesLeft = data.move_count;
        items = new ItemBase[Width, Height];

        gridOrigin = new Vector3(
            -(Width - 1) * CellSize / 2f,
            -(Height - 1) * CellSize / 2f - 0.5f,
            0f
        );

        CreateBackground();

        totalObstacles = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                string cell = data.GetCell(x, y);
                CreateItemFromData(cell, x, y);
            }
        }

        UpdateRocketIndicators();
    }

    private void CreateBackground()
    {
        Sprite bgSprite = SpriteLoader.Get("Sprites/UI/grid_background");
        if (bgSprite == null) return;

        GameObject bg = new GameObject("GridBackground");
        bg.transform.SetParent(transform);
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -10;

        float targetW = Width * CellSize + 0.6f;
        float targetH = Height * CellSize + 0.6f;
        Vector2 spriteSize = sr.sprite.bounds.size;
        bg.transform.localScale = new Vector3(targetW / spriteSize.x, targetH / spriteSize.y, 1f);
        bg.transform.position = new Vector3(
            gridOrigin.x + (Width - 1) * CellSize / 2f,
            gridOrigin.y + (Height - 1) * CellSize / 2f,
            1f
        );
    }

    private void CreateItemFromData(string cellType, int x, int y)
    {
        switch (cellType)
        {
            case "r":
            case "g":
            case "b":
            case "y":
                CreateCube(x, y, CubeItem.StringToColor(cellType));
                break;
            case "rand":
                CreateCube(x, y, CubeItem.RandomColor());
                break;
            case "bo":
                CreateObstacle(x, y, ObstacleType.Box);
                totalObstacles++;
                break;
            case "s":
                CreateObstacle(x, y, ObstacleType.Stone);
                totalObstacles++;
                break;
            case "v":
                CreateObstacle(x, y, ObstacleType.Vase);
                totalObstacles++;
                break;
        }
    }

    private CubeItem CreateCube(int x, int y, CubeColor color)
    {
        GameObject go = new GameObject("Cube_" + x + "_" + y);
        go.transform.SetParent(transform);
        go.transform.position = GridToWorld(x, y);
        go.AddComponent<SpriteRenderer>();
        CubeItem cube = go.AddComponent<CubeItem>();
        cube.InitCube(x, y, color);
        cube.FitToCell(CellSize);
        cube.SetSortingOrder(1);
        items[x, y] = cube;
        return cube;
    }

    private RocketItem CreateRocket(int x, int y, RocketDirection dir)
    {
        GameObject go = new GameObject("Rocket_" + x + "_" + y);
        go.transform.SetParent(transform);
        go.transform.position = GridToWorld(x, y);
        go.AddComponent<SpriteRenderer>();
        RocketItem rocket = go.AddComponent<RocketItem>();
        rocket.InitRocket(x, y, dir);
        rocket.FitToCell(CellSize);
        rocket.SetSortingOrder(1);
        items[x, y] = rocket;
        return rocket;
    }

    private ObstacleItem CreateObstacle(int x, int y, ObstacleType type)
    {
        GameObject go = new GameObject("Obstacle_" + x + "_" + y);
        go.transform.SetParent(transform);
        go.transform.position = GridToWorld(x, y);
        go.AddComponent<SpriteRenderer>();
        ObstacleItem obs = go.AddComponent<ObstacleItem>();
        obs.InitObstacle(x, y, type);
        obs.FitToCell(CellSize);
        obs.SetSortingOrder(1);
        items[x, y] = obs;
        return obs;
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(
            gridOrigin.x + x * CellSize,
            gridOrigin.y + y * CellSize,
            0f
        );
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    void Update()
    {
        if (isProcessing) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt((worldPos.x - gridOrigin.x) / CellSize);
            int y = Mathf.RoundToInt((worldPos.y - gridOrigin.y) / CellSize);

            if (InBounds(x, y) && items[x, y] != null)
            {
                OnCellTapped(x, y);
            }
        }
    }

    private void OnCellTapped(int x, int y)
    {
        ItemBase item = items[x, y];

        if (item is CubeItem)
        {
            List<Vector2Int> group = FindGroup(x, y);
            if (group.Count < 2) return;
            isProcessing = true;
            StartCoroutine(ProcessCubeBlast(group, x, y));
        }
        else if (item is RocketItem)
        {
            isProcessing = true;
            StartCoroutine(ProcessRocketTap(x, y));
        }
    }

    // ==================== GROUP FINDING ====================

    private List<Vector2Int> FindGroup(int x, int y)
    {
        List<Vector2Int> group = new List<Vector2Int>();
        ItemBase item = items[x, y];
        if (!(item is CubeItem cube)) return group;

        CubeColor color = cube.Color;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int start = new Vector2Int(x, y);
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            group.Add(pos);

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int neighbor = pos + dir;
                if (!InBounds(neighbor.x, neighbor.y)) continue;
                if (visited.Contains(neighbor)) continue;
                if (items[neighbor.x, neighbor.y] is CubeItem neighborCube && neighborCube.Color == color)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return group;
    }

    private List<List<Vector2Int>> FindAllGroups()
    {
        List<List<Vector2Int>> allGroups = new List<List<Vector2Int>>();
        HashSet<Vector2Int> processed = new HashSet<Vector2Int>();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (processed.Contains(pos)) continue;
                if (!(items[x, y] is CubeItem)) continue;

                List<Vector2Int> group = FindGroup(x, y);
                if (group.Count >= 2)
                {
                    allGroups.Add(group);
                    foreach (var p in group)
                        processed.Add(p);
                }
                else
                {
                    processed.Add(pos);
                }
            }
        }

        return allGroups;
    }

    // ==================== CUBE BLAST ====================

    private IEnumerator ProcessCubeBlast(List<Vector2Int> group, int tapX, int tapY)
    {
        movesLeft--;
        OnMovesChanged?.Invoke(movesLeft);

        bool createRocket = group.Count >= 4;

        if (createRocket)
        {
            // Animate cubes moving to tap position
            yield return AnimateCubesToPoint(group, tapX, tapY);
        }
        else
        {
            yield return AnimateBlast(group);
        }

        // Damage adjacent obstacles (track vases to limit 1 damage per blast)
        HashSet<Vector2Int> damagedVases = new HashSet<Vector2Int>();
        foreach (Vector2Int pos in group)
        {
            DamageAdjacentObstacles(pos, damagedVases);
        }

        // Remove blasted cubes
        foreach (Vector2Int pos in group)
        {
            DestroyItem(pos.x, pos.y);
        }

        // Create rocket if applicable
        if (createRocket)
        {
            RocketDirection dir = UnityEngine.Random.value > 0.5f
                ? RocketDirection.Horizontal
                : RocketDirection.Vertical;
            CreateRocket(tapX, tapY, dir);
        }

        // Remove destroyed obstacles
        RemoveDestroyedObstacles();

        // Apply gravity and fill
        yield return ApplyGravityAndFill();

        UpdateRocketIndicators();
        CheckWinLose();
        isProcessing = false;
    }

    private IEnumerator AnimateBlast(List<Vector2Int> group)
    {
        float elapsed = 0f;
        List<Transform> transforms = new List<Transform>();
        foreach (var pos in group)
        {
            if (items[pos.x, pos.y] != null)
                transforms.Add(items[pos.x, pos.y].transform);
        }

        Vector3[] startScales = new Vector3[transforms.Count];
        for (int i = 0; i < transforms.Count; i++)
            startScales[i] = transforms[i].localScale;

        while (elapsed < BlastDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / BlastDuration;
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i] != null)
                    transforms[i].localScale = startScales[i] * (1f - t);
            }
            yield return null;
        }
    }

    private IEnumerator AnimateCubesToPoint(List<Vector2Int> group, int tapX, int tapY)
    {
        Vector3 targetPos = GridToWorld(tapX, tapY);
        float elapsed = 0f;

        List<Transform> transforms = new List<Transform>();
        List<Vector3> startPositions = new List<Vector3>();

        foreach (var pos in group)
        {
            if (items[pos.x, pos.y] != null)
            {
                transforms.Add(items[pos.x, pos.y].transform);
                startPositions.Add(items[pos.x, pos.y].transform.position);
            }
        }

        while (elapsed < MergeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / MergeDuration;
            t = t * t; // ease in
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i] != null)
                {
                    transforms[i].position = Vector3.Lerp(startPositions[i], targetPos, t);
                    transforms[i].localScale = Vector3.one * (1f - t * 0.5f) * 0.9f;
                }
            }
            yield return null;
        }
    }

    // ==================== OBSTACLE DAMAGE ====================

    private void DamageAdjacentObstacles(Vector2Int blastPos, HashSet<Vector2Int> damagedVases)
    {
        foreach (Vector2Int dir in Directions)
        {
            Vector2Int neighbor = blastPos + dir;
            if (!InBounds(neighbor.x, neighbor.y)) continue;
            if (items[neighbor.x, neighbor.y] is ObstacleItem obs && !obs.IsDestroyed)
            {
                if (!obs.CanBeDamagedByBlast()) continue;

                // Vases take max 1 damage per blast group
                if (obs.Type == ObstacleType.Vase)
                {
                    if (damagedVases.Contains(neighbor)) continue;
                    damagedVases.Add(neighbor);
                }

                obs.TakeDamage();
            }
        }
    }

    private void DamageByRocket(int x, int y, HashSet<Vector2Int> explodedRockets)
    {
        if (!InBounds(x, y)) return;
        ItemBase item = items[x, y];
        if (item == null) return;

        if (item is CubeItem)
        {
            DestroyItem(x, y);
        }
        else if (item is ObstacleItem obs && !obs.IsDestroyed)
        {
            if (obs.CanBeDamagedByRocket())
                obs.TakeDamage();
        }
        else if (item is RocketItem)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (!explodedRockets.Contains(pos))
            {
                explodedRockets.Add(pos);
            }
        }
    }

    // ==================== ROCKET LOGIC ====================

    private IEnumerator ProcessRocketTap(int x, int y)
    {
        movesLeft--;
        OnMovesChanged?.Invoke(movesLeft);

        // Check for adjacent rockets for combo
        List<Vector2Int> adjacentRockets = FindAdjacentRockets(x, y);

        if (adjacentRockets.Count > 0)
        {
            yield return HandleRocketCombo(x, y, adjacentRockets);
        }
        else
        {
            HashSet<Vector2Int> explodedRockets = new HashSet<Vector2Int>();
            explodedRockets.Add(new Vector2Int(x, y));
            yield return ExplodeSingleRocket(x, y, explodedRockets);

            // Chain reaction: explode any rockets that were hit
            yield return ProcessChainReactions(explodedRockets);
        }

        RemoveDestroyedObstacles();
        yield return ApplyGravityAndFill();
        UpdateRocketIndicators();
        CheckWinLose();
        isProcessing = false;
    }

    private List<Vector2Int> FindAdjacentRockets(int x, int y)
    {
        List<Vector2Int> rockets = new List<Vector2Int>();
        foreach (Vector2Int dir in Directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            if (InBounds(nx, ny) && items[nx, ny] is RocketItem)
            {
                rockets.Add(new Vector2Int(nx, ny));
            }
        }
        return rockets;
    }

    private IEnumerator ExplodeSingleRocket(int x, int y, HashSet<Vector2Int> explodedRockets)
    {
        RocketItem rocket = items[x, y] as RocketItem;
        if (rocket == null) yield break;

        RocketDirection dir = rocket.Direction;
        DestroyItem(x, y);

        // Create two moving parts
        yield return AnimateRocketParts(x, y, dir, explodedRockets);
    }

    private IEnumerator AnimateRocketParts(int x, int y, RocketDirection dir, HashSet<Vector2Int> explodedRockets)
    {
        Vector3 origin = GridToWorld(x, y);

        // Create part 1 (left/bottom)
        GameObject part1 = CreateRocketPartVisual(dir, true);
        part1.transform.position = origin;

        // Create part 2 (right/top)
        GameObject part2 = CreateRocketPartVisual(dir, false);
        part2.transform.position = origin;

        Vector2Int dir1, dir2;
        if (dir == RocketDirection.Horizontal)
        {
            dir1 = Vector2Int.left;
            dir2 = Vector2Int.right;
        }
        else
        {
            dir1 = Vector2Int.down;
            dir2 = Vector2Int.up;
        }

        int maxDist = Mathf.Max(Width, Height);
        float speed = FallSpeed * 1.5f;

        // Animate parts moving outward
        float elapsed = 0f;
        float totalTime = maxDist * CellSize / speed;
        HashSet<Vector2Int> damaged = new HashSet<Vector2Int>();

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float dist = elapsed * speed;

            if (part1 != null)
            {
                Vector3 offset1 = new Vector3(dir1.x, dir1.y, 0) * dist;
                part1.transform.position = origin + offset1;
            }
            if (part2 != null)
            {
                Vector3 offset2 = new Vector3(dir2.x, dir2.y, 0) * dist;
                part2.transform.position = origin + offset2;
            }

            // Damage cells along the path
            int cellDist = Mathf.FloorToInt(dist / CellSize) + 1;
            for (int d = 1; d <= cellDist && d <= maxDist; d++)
            {
                Vector2Int pos1 = new Vector2Int(x + dir1.x * d, y + dir1.y * d);
                if (InBounds(pos1.x, pos1.y) && !damaged.Contains(pos1))
                {
                    damaged.Add(pos1);
                    DamageByRocket(pos1.x, pos1.y, explodedRockets);
                }

                Vector2Int pos2 = new Vector2Int(x + dir2.x * d, y + dir2.y * d);
                if (InBounds(pos2.x, pos2.y) && !damaged.Contains(pos2))
                {
                    damaged.Add(pos2);
                    DamageByRocket(pos2.x, pos2.y, explodedRockets);
                }
            }

            yield return null;
        }

        if (part1 != null) Destroy(part1);
        if (part2 != null) Destroy(part2);
    }

    private GameObject CreateRocketPartVisual(RocketDirection dir, bool isFirst)
    {
        GameObject go = new GameObject("RocketPart");
        go.transform.SetParent(transform);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteLoader.GetRocketPart(dir, isFirst);
        sr.sortingOrder = 5;

        if (sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            float scale = CellSize * 0.9f / Mathf.Max(spriteSize.x, spriteSize.y);
            go.transform.localScale = new Vector3(scale, scale, 1f);
        }

        return go;
    }

    private IEnumerator HandleRocketCombo(int x, int y, List<Vector2Int> adjacentRockets)
    {
        HashSet<Vector2Int> explodedRockets = new HashSet<Vector2Int>();
        explodedRockets.Add(new Vector2Int(x, y));

        // Destroy tapped rocket and adjacent rockets
        DestroyItem(x, y);
        foreach (var pos in adjacentRockets)
        {
            explodedRockets.Add(pos);
            DestroyItem(pos.x, pos.y);
        }

        // 3x3 cross explosion in both directions
        HashSet<Vector2Int> damaged = new HashSet<Vector2Int>();

        // Horizontal band: 3 rows wide, full width
        for (int row = y - 1; row <= y + 1; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                Vector2Int pos = new Vector2Int(col, row);
                if (!InBounds(col, row) || damaged.Contains(pos)) continue;
                if (col == x && row == y) continue;
                damaged.Add(pos);
                DamageByRocket(col, row, explodedRockets);
            }
        }

        // Vertical band: 3 columns wide, full height
        for (int col = x - 1; col <= x + 1; col++)
        {
            for (int row = 0; row < Height; row++)
            {
                Vector2Int pos = new Vector2Int(col, row);
                if (!InBounds(col, row) || damaged.Contains(pos)) continue;
                damaged.Add(pos);
                DamageByRocket(col, row, explodedRockets);
            }
        }

        // Brief visual effect
        yield return new WaitForSeconds(0.2f);

        // Process chain reactions
        yield return ProcessChainReactions(explodedRockets);
    }

    private IEnumerator ProcessChainReactions(HashSet<Vector2Int> explodedRockets)
    {
        bool chainContinues = true;
        while (chainContinues)
        {
            chainContinues = false;
            List<Vector2Int> toExplode = new List<Vector2Int>();

            // Find rockets marked for explosion that still exist
            foreach (var pos in explodedRockets)
            {
                if (items[pos.x, pos.y] is RocketItem)
                {
                    toExplode.Add(pos);
                }
            }

            foreach (var pos in toExplode)
            {
                if (items[pos.x, pos.y] is RocketItem)
                {
                    chainContinues = true;
                    yield return ExplodeSingleRocket(pos.x, pos.y, explodedRockets);
                }
            }
        }

        RemoveDestroyedObstacles();
    }

    // ==================== GRAVITY AND FILL ====================

    private IEnumerator ApplyGravityAndFill()
    {
        // Apply gravity
        List<ItemMovement> movements = CalculateGravity();

        // Apply movement to grid data
        foreach (var move in movements)
        {
            items[move.toX, move.toY] = move.item;
            items[move.fromX, move.fromY] = null;
            move.item.X = move.toX;
            move.item.Y = move.toY;
        }

        // Fill empty top cells
        List<ItemMovement> fills = FillEmptyCells();

        // Combine all movements for animation
        movements.AddRange(fills);

        if (movements.Count > 0)
        {
            yield return AnimateMovements(movements);
        }

        // Recursive: check if more gravity needed after fill
        List<ItemMovement> moreMovements = CalculateGravity();
        if (moreMovements.Count > 0)
        {
            yield return ApplyGravityAndFill();
        }
    }

    private List<ItemMovement> CalculateGravity()
    {
        List<ItemMovement> movements = new List<ItemMovement>();

        for (int x = 0; x < Width; x++)
        {
            // Find segments separated by non-falling items
            int segmentBottom = 0;

            for (int y = 0; y <= Height; y++)
            {
                bool isBoundary = (y == Height) ||
                    (items[x, y] != null && !items[x, y].CanFall);

                if (isBoundary)
                {
                    // Compact segment from segmentBottom to y-1
                    int writeY = segmentBottom;
                    for (int readY = segmentBottom; readY < y; readY++)
                    {
                        if (items[x, readY] != null)
                        {
                            if (readY != writeY)
                            {
                                movements.Add(new ItemMovement
                                {
                                    item = items[x, readY],
                                    fromX = x, fromY = readY,
                                    toX = x, toY = writeY,
                                    startPos = items[x, readY].transform.position,
                                    endPos = GridToWorld(x, writeY)
                                });
                            }
                            writeY++;
                        }
                    }
                    segmentBottom = y + 1;
                }
            }
        }

        return movements;
    }

    private List<ItemMovement> FillEmptyCells()
    {
        List<ItemMovement> fills = new List<ItemMovement>();

        for (int x = 0; x < Width; x++)
        {
            // Find the top of the highest non-falling blocker in this column
            int fillFrom = 0;
            for (int y = Height - 1; y >= 0; y--)
            {
                if (items[x, y] != null && !items[x, y].CanFall)
                {
                    fillFrom = y + 1;
                    break;
                }
            }

            // Count empty cells in the fillable region (fillFrom to Height-1)
            int emptyCount = 0;
            for (int y = fillFrom; y < Height; y++)
            {
                if (items[x, y] == null)
                    emptyCount++;
            }

            // Fill from top
            int spawnIndex = 0;
            for (int y = Height - 1; y >= fillFrom && emptyCount > 0; y--)
            {
                if (items[x, y] == null)
                {
                    CubeItem cube = CreateCube(x, y, CubeItem.RandomColor());
                    Vector3 spawnPos = GridToWorld(x, Height + spawnIndex);
                    cube.transform.position = spawnPos;

                    fills.Add(new ItemMovement
                    {
                        item = cube,
                        fromX = x, fromY = Height + spawnIndex,
                        toX = x, toY = y,
                        startPos = spawnPos,
                        endPos = GridToWorld(x, y)
                    });

                    spawnIndex++;
                    emptyCount--;
                }
            }
        }

        return fills;
    }

    private IEnumerator AnimateMovements(List<ItemMovement> movements)
    {
        if (movements.Count == 0) yield break;

        // Calculate max distance for duration
        float maxDist = 0f;
        foreach (var m in movements)
        {
            float dist = Vector3.Distance(m.startPos, m.endPos);
            if (dist > maxDist) maxDist = dist;
        }

        float duration = maxDist / FallSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease out
            float eased = 1f - (1f - t) * (1f - t);

            foreach (var m in movements)
            {
                if (m.item != null && m.item.transform != null)
                {
                    m.item.transform.position = Vector3.Lerp(m.startPos, m.endPos, eased);
                }
            }
            yield return null;
        }

        // Snap to final positions
        foreach (var m in movements)
        {
            if (m.item != null && m.item.transform != null)
            {
                m.item.transform.position = m.endPos;
            }
        }
    }

    // ==================== UTILITY ====================

    private void DestroyItem(int x, int y)
    {
        if (items[x, y] != null)
        {
            SpawnParticleEffect(items[x, y]);
            Destroy(items[x, y].gameObject);
            items[x, y] = null;
        }
    }

    private void SpawnParticleEffect(ItemBase item)
    {
        GameObject particleGo = new GameObject("Particle");
        particleGo.transform.position = item.transform.position;
        ParticleSystem ps = particleGo.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.duration = 0.2f;
        main.loop = false;
        main.maxParticles = 8;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 8)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        // Set color based on item type
        if (item is CubeItem cube)
        {
            main.startColor = GetCubeParticleColor(cube.Color);
        }
        else if (item is ObstacleItem)
        {
            main.startColor = new Color(0.7f, 0.5f, 0.3f);
        }
        else
        {
            main.startColor = Color.white;
        }

        var renderer = particleGo.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 10;

        Destroy(particleGo, 1f);
    }

    private Color GetCubeParticleColor(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red: return new Color(1f, 0.3f, 0.3f);
            case CubeColor.Green: return new Color(0.3f, 1f, 0.3f);
            case CubeColor.Blue: return new Color(0.3f, 0.3f, 1f);
            case CubeColor.Yellow: return new Color(1f, 1f, 0.3f);
            default: return Color.white;
        }
    }

    private void RemoveDestroyedObstacles()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (items[x, y] is ObstacleItem obs && obs.IsDestroyed)
                {
                    DestroyItem(x, y);
                }
            }
        }
    }

    private void UpdateRocketIndicators()
    {
        // Reset all indicators
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (items[x, y] is CubeItem c)
                    c.SetRocketIndicator(false);

        // Find all groups and mark 4+ groups
        List<List<Vector2Int>> groups = FindAllGroups();
        foreach (var group in groups)
        {
            if (group.Count >= 4)
            {
                foreach (var pos in group)
                {
                    if (items[pos.x, pos.y] is CubeItem cube)
                        cube.SetRocketIndicator(true);
                }
            }
        }
    }

    private int CountRemainingObstacles()
    {
        int count = 0;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (items[x, y] is ObstacleItem obs && !obs.IsDestroyed)
                    count++;
        return count;
    }

    private void CheckWinLose()
    {
        int remaining = CountRemainingObstacles();

        if (remaining == 0)
        {
            OnWin?.Invoke();
            return;
        }

        if (movesLeft <= 0)
        {
            OnLose?.Invoke();
        }
    }

    public int GetRemainingObstacles()
    {
        return CountRemainingObstacles();
    }

    public int GetMovesLeft()
    {
        return movesLeft;
    }

    private struct ItemMovement
    {
        public ItemBase item;
        public int fromX, fromY, toX, toY;
        public Vector3 startPos, endPos;
    }
}
