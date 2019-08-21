using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelScanner : MonoBehaviour
{
    public Gradient threatColorGradient;

    [Range(0,1)]
    public float aggression = 0;

    public int row;
    public int col;

    public bool evaluateThreat;

    public bool showDebug;

    private Color[] blockColors = new Color[] { Color.green, Color.yellow, new Color(1, 0.5f, 0), Color.red, Color.magenta };

    private int currentTargetBlockIdx;

    LevelBlock[] levelBlocks;
    List<LevelBlock> blocksOnMesh = new List<LevelBlock>();

    LevelBlock startBlock;
    LevelBlock endBlock;
    LevelBlock[] pathBlocks;

    float evaluateTimer = 0;

    public GameObject healthPrefab;
    private GameObject healthInstance;
    private CompleteProject.PlayerHealth playerHealth;


    private LevelBlock blockWithHealth;

    public float[] factors;

    // Start is called before the first frame update
    void Awake()
    {
        levelBlocks = new LevelBlock[row * col];

        for(int i = 0; i < row; i++)
        {
            for(int t = 0; t < col; t++)
            {
                int idx = i * col + t;

                Vector3 pos = new Vector3(t * 2, 0, i * 2);
                levelBlocks[idx] = new LevelBlock(i, t, pos);

                NavMeshHit hit;

                if(NavMesh.SamplePosition(pos, out hit, 0.2f, NavMesh.AllAreas))
                {
                    //levelBlocks[idx].pos = hit.position;
                    levelBlocks[idx].isOnNavMesh = true;
                    blocksOnMesh.Add(levelBlocks[idx]);
                }
                else
                {
                    levelBlocks[idx].isOnNavMesh = false;
                }
            }
        }

        InitThreatScore();
        SpawnHealth();

        playerHealth = GetComponent<CompleteProject.PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        if(evaluateThreat)
        {
            evaluateTimer += Time.deltaTime;

            if(evaluateTimer > 0.25f)
            {
                for (int i = 0; i < levelBlocks.Length; i++)
                {
                    levelBlocks[i].enemyCount = CheckEnemyCount(levelBlocks[i].pos, 1.25f);
                }

                evaluateTimer = 0;
            }
        }

        aggression = EvaluateAggression(0, 0.5f);

        if(Input.GetKeyDown(KeyCode.T))
        {
            startBlock = GetPlayerBlock();

            for (int i = 0; i < levelBlocks.Length; i++)
            {
                List<LevelBlock> blocksInRange = GetBlocksInRange(levelBlocks[i].pos, 5);
                levelBlocks[i].EvaluateThreat(blocksInRange, factors);
            }

            pathBlocks = AStarPathfinding(startBlock, GetBlock(18,18));
        }
    }

    LevelBlock GetPlayerBlock()
    {
        int row;
        int col;

        GetBlockPosByPosition(transform.position, out col, out row);

        return GetBlock(row, col);
    }

    public void OnHealthPickup()
    {
        SpawnHealth();
    }

    void SpawnHealth()
    {
        if(healthInstance != null)
        {
            Destroy(healthInstance);
        }

        LevelBlock levelBlock = GetRandomBlockOnMesh();
        healthInstance = GameObject.Instantiate(healthPrefab, levelBlock.pos, Quaternion.identity);
        blockWithHealth = levelBlock;

        Invoke("SpawnHealth", 20);
    }

    LevelBlock GetRandomBlockOnMesh()
    {
        int idx = Random.Range(0, blocksOnMesh.Count);
        return blocksOnMesh[idx];
    }

    void InitThreatScore()
    {
        for (int i = 0; i < row; i++)
        {
            for (int t = 0; t < col; t++)
            {
                int idx = i * col + t;

                levelBlocks[idx].occulsion = EvaluateOcculsion(i, t);
            }
        }
    }

    int CheckEnemyCount(Vector3 position, float radius)
    {
        Collider[] cols = Physics.OverlapSphere(position, radius);

        List<CompleteProject.EnemyHealth> enemies = new List<CompleteProject.EnemyHealth>();

        for (int i = 0; i < cols.Length; i++)
        {
            CompleteProject.EnemyHealth enemyHealth = cols[i].GetComponent<CompleteProject.EnemyHealth>();

            if (enemyHealth && enemyHealth.currentHealth > 0 && !enemies.Contains(enemyHealth))
            {
                enemies.Add(enemyHealth);
            }
        }

        return enemies.Count;
    }

    float EvaluateAggression(float min, float max)
    {
        float hpRemains = (float)playerHealth.currentHealth / playerHealth.startingHealth;
        return Mathf.Clamp(hpRemains, min, max);
    }

    int EvaluateOcculsion(int row, int col)
    {
        if(row >= 0 && col >= 0)
        {
            int idx = row * this.col + col;

            if(idx < levelBlocks.Length)
            {
                int occulsion = 0;

                LevelBlock centerBlock = GetBlock(row, col);

                if(centerBlock != null)
                {
                    List<LevelBlock> blocksInRange = GetBlocksInRange(centerBlock.pos, 1);

                    occulsion = 9 - blocksInRange.Count;
                }

                return occulsion;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            return -1;
        }
    }

    public LevelBlock GetHealthBlock()
    {
        return blockWithHealth;
    }

    void TestSort()
    {
        List<LevelBlock> blockList = new List<LevelBlock>(levelBlocks);

        Debug.LogWarning("Before Sort");

        for (int i = 0; i < blockList.Count; i++)
        {
            Debug.Log(i + ":" + blockList[i].row + "," + blockList[i].col + "," + blockList[i].threatScore);
        }

        blockList.Sort();

        Debug.LogWarning("After Sort");

        for (int i = 0; i < blockList.Count; i++)
        {
            Debug.Log(i + ":" + blockList[i].row + "," + blockList[i].col + "," + blockList[i].threatScore);
        }
    }

    public LevelBlock GetSafeAreaCenter()
    {
        LevelBlock[] nearAreaCenters = GetNearAreaCenters();

        int safeAreaIdx = -1;
        int minAreaEnemyCount = int.MaxValue;

        Debug.Log("Start Count Area");

        for(int i = 0; i < nearAreaCenters.Length; i++)
        {
            if (nearAreaCenters[i] == null) continue;

            int areaEnemyCount = GetAreaEnemyCount(nearAreaCenters[i], 1);

            if(areaEnemyCount < minAreaEnemyCount)
            {
                minAreaEnemyCount = areaEnemyCount;
                safeAreaIdx = i;
            }
        }

        Debug.Log("End Count Area");

        if (safeAreaIdx == -1)
        {
            return GetPlayerBlock();
        }
        else
        {
            if (nearAreaCenters[safeAreaIdx] == null)
            {
                return GetPlayerBlock();
            }
            else
            {
                return nearAreaCenters[safeAreaIdx];
            }
        }

    }

    public LevelBlock[] GetNearAreaCenters()
    {
        List<LevelBlock> centers = new List<LevelBlock>();

        for(int i = -1; i <= 1; i++)
        {
            for(int t = -1; t <= 1; t++)
            {
                LevelBlock center = GetNearAreaCenter(i, t);

                // 所有方格均不能移动的区域，设为空值
                if(center != null && GetBlocksInRange(center.pos, 1).Count == 0)
                {
                    center = null;
                }

                centers.Add(center);
            }
        }

        return centers.ToArray();
    }

    public LevelBlock GetNearAreaCenter(int offset_x, int offset_y)
    {
        LevelBlock playerBlock = GetPlayerBlock();
        int center_row = playerBlock.row + offset_y * 3;
        int center_col = playerBlock.col + offset_x * 3;

        LevelBlock centerBlock = GetBlock(center_row, center_col);
        return centerBlock;
    }

    public int GetAreaEnemyCount(LevelBlock centerBlock, int radius)
    {
        List<LevelBlock> area = GetBlocksInRange(centerBlock.pos, radius);

        int totalEnemies = 0;

        for(int i = 0; i < area.Count; i++)
        {
            totalEnemies += area[i].enemyCount;
        }

        //Debug.Log("Area Center:" + centerBlock.row + "," + centerBlock.col + ", Enemy Count:" + totalEnemies);

        return totalEnemies;
    }

    public List<LevelBlock> GetBlocksInRange(Vector3 center, int radius, bool excludeCenterBlock = false)
    {
        int centerCol;
        int centerRow;

        GetBlockPosByPosition(center, out centerCol, out centerRow);

        List<LevelBlock> blocks = new List<LevelBlock>();

        for (int offset_col = -radius; offset_col <= radius; offset_col++)
        {
            for (int offset_row = -radius; offset_row <= radius; offset_row++)
            {
                if (excludeCenterBlock && offset_col == 0 && offset_row == 0) continue;

                int currentRow = centerRow + offset_row;
                int currentCol = centerCol + offset_col;

                bool rowInside = Mathf.Clamp(currentRow, 0, this.row - 1) == currentRow;
                bool colInside = Mathf.Clamp(currentCol, 0, this.col - 1) == currentCol;

                if (colInside && rowInside)
                {
                    int currentBlockIdx = currentRow * this.col + currentCol;
                    LevelBlock currentBlock = levelBlocks[currentBlockIdx];

                    if (currentBlock.isOnNavMesh)
                    {
                        blocks.Add(currentBlock);
                    }
                }
            }
        }

        return blocks;
    }

    public LevelBlock[] GetSafePath(Vector3 center)
    {
        return GetSafePath(center, 3, 1, 0.1f);
    }

    public LevelBlock[] GetSafePath(Vector3 center, int radius)
    {
        ResetDebugHighlight();

        endBlock = GetSafePosition(radius);

        startBlock = GetPlayerBlock();

        pathBlocks = GetSafePath(startBlock.pos, 1 + radius, radius, aggression);
        //pathBlocks = AStarPathfinding(startBlock, endBlock);

        return pathBlocks;
    }

    public LevelBlock[] GetSafePath(Vector3 center, int pathNodeCount, int radius, float agreesion)
    {
        ResetDebugHighlight();

        LevelBlock[] results = new LevelBlock[pathNodeCount];
        List<LevelBlock> blocksInRange = new List<LevelBlock>();

        for (int i = 0; i < levelBlocks.Length; i++)
        {
            blocksInRange = GetBlocksInRange(levelBlocks[i].pos, 5);
            levelBlocks[i].EvaluateThreat(blocksInRange, factors);
            //levelBlocks[i].threatScore = EvaluateThreatScore(levelBlocks[i], 5);
        }

        for (int i = 0; i < pathNodeCount; i++)
        {
            if (i == 0)
            {
                blocksInRange = GetBlocksInRange(center, radius);
                blocksInRange.Sort();
            }
            else
            {
                blocksInRange = GetBlocksInRange(results[i-1].pos, radius);
                blocksInRange.Sort();
            }

            results[i] = blocksInRange[0];
            results[i].debugHighlight = true;
        }

        return results;
    }

    public LevelBlock GetSafePosition(int radius = 3)
    {
        //int myBlockX;
        //int myBlockY;

        //GetBlockPosByPosition(transform.position, out myBlockX, out myBlockY);

        for (int i = 0; i < levelBlocks.Length; i++)
        {
            List<LevelBlock> blocksInRange = GetBlocksInRange(levelBlocks[i].pos, 5);
            levelBlocks[i].EvaluateThreat(blocksInRange, factors);
            //levelBlocks[i].threatScore = EvaluateThreatScore(levelBlocks[i], 5);
        }

        LevelBlock safeAreaCenter = GetSafeAreaCenter();
        //Debug.Log("Area Center:" + safeAreaCenter.row + "," + safeAreaCenter.col);
        LevelBlock levelBlock = GetSafeBlockInRange(safeAreaCenter.row, safeAreaCenter.col, radius, aggression);
        currentTargetBlockIdx = levelBlock.row * col + levelBlock.col;
        return levelBlock;
    }

    public LevelBlock GetSafeBlockInRange(int centerRow, int centerCol, int offset, float aggression = 0)
    {
        //ResetDebugHighlight();

        LevelBlock centerBlock = GetBlock(centerRow, centerCol);

        if(centerBlock != null)
        {
            List<LevelBlock> blocksInRange = GetBlocksInRange(centerBlock.pos, offset);

            if(blocksInRange.Count == 0)
            {
                Debug.Log("Null: Area Center:" + centerRow + "," + centerCol);
                return null;
            }

            blocksInRange.Sort();

            int lowestThreat = blocksInRange[0].threatScore;
            int highestThreat = blocksInRange[blocksInRange.Count - 1].threatScore;

            int desireThreat = (int)Mathf.Lerp(lowestThreat, highestThreat, aggression);

            Debug.Log("lowestThreat:" + lowestThreat);
            Debug.Log("highestThreat:" + highestThreat);
            Debug.Log("desireThreat:" + desireThreat);

            int minDelta = highestThreat;
            int desireBlockIdx = -1;

            for (int i = 0; i < blocksInRange.Count; i++)
            {
                int delta = Mathf.Abs(blocksInRange[i].threatScore - desireThreat);
                //Debug.Log(i + ":" + blocksInRange[i].threatScore + "," + delta + "," + blocksInRange[i].isOnNavMesh);

                if(delta < minDelta)
                {
                    minDelta = delta;
                    desireBlockIdx = i;
                }
            }

            Debug.Log("Desire Block : " + desireBlockIdx + ",score:" + blocksInRange[desireBlockIdx].threatScore + ", delta:" + minDelta + "," + blocksInRange[desireBlockIdx].isOnNavMesh);
            return blocksInRange[desireBlockIdx];
        }

        return null;
    }

    LevelBlock GetBlock(Vector3 position)
    {
        int row;
        int col;

        GetBlockPosByPosition(position, out col, out row);

        return GetBlock(row, col);
    }

    LevelBlock GetBlock(int row, int col)
    {
        int blockIdx = row * this.col + col;

        //Debug.Log("Block Idx:" + blockIdx + ", Clamp:" + Mathf.Clamp(blockIdx, 0, levelBlocks.Length - 1));

        if(blockIdx == Mathf.Clamp(blockIdx, 0, levelBlocks.Length - 1))
        {
            return levelBlocks[blockIdx];
        }
        else
        {
            return null;
        }
    }

    void GetBlockPosByPosition(Vector3 position, out int blockPos_x, out int blockPos_y)
    {
        blockPos_x = Mathf.RoundToInt(position.x) / 2;
        blockPos_y = Mathf.RoundToInt(position.z) / 2;

        //Debug.Log("x:" + blockPos_x + ",y:" + blockPos_y);
    }

    LevelBlock[] AStarPathfinding(LevelBlock startBlock, LevelBlock endBlock)
    {
        List<LevelBlock> openList = new List<LevelBlock>();     // 等待检查的节点
        List<LevelBlock> closedList = new List<LevelBlock>();   // 已经检查完毕的节点

        openList.Add(startBlock);   // 将起始节点添加到openList

        // 循环直至没有节点等待检查，或者发现有效路径
        while (openList.Count > 0)
        {
            // 找出等待检查的节点中移动成本 f 最低的一个
            LevelBlock currentBlock = openList[0];
            currentBlock.astar_g = CaculateMoveCost(startBlock, currentBlock);
            currentBlock.astar_h = CaculateMoveCost(endBlock, currentBlock);
            int fmin = currentBlock.astar_f;

            for (int i = 0; i < openList.Count; i++)
            {
                openList[i].astar_g = CaculateMoveCost(startBlock, openList[i]);
                openList[i].astar_h = CaculateMoveCost(endBlock, openList[i]);

                int f = openList[i].astar_f;

                if (f < fmin)
                {
                    currentBlock = openList[i];
                    fmin = f;
                }
            }

            // 找到移动成本最低的节点，设置为当前节点，将它移入检查完成的节点列表closedList
            // 如果当前节点就是目标节点，执行保存路径操作
            // 否则，将当前节点周边八格里面未检查的节点添加进openList，等待检查

            openList.Remove(currentBlock);
            closedList.Add(currentBlock);

            // 如果找到了目标节点，就进入保存路径操作
            if (currentBlock == endBlock)
            {
                // 从目标节点往回找，直至回到开始节点

                Debug.Log("找到路径");

                LevelBlock backSearchBlock = endBlock;

                List<LevelBlock> path = new List<LevelBlock>();

                path.Add(backSearchBlock);

                while(backSearchBlock != startBlock)
                {
                    backSearchBlock = backSearchBlock.lastBlock;
                    path.Add(backSearchBlock);
                }

                path.Reverse();

                return path.ToArray();
            }


            // 将周边八格里面未检查的节点添加进待检查列表openList
            List<LevelBlock> neighborBlocks = GetBlocksInRange(currentBlock.pos, 1, true);

            for (int i = 0; i < neighborBlocks.Count; i++)
            {
                // 不在导航网格上的节点，忽略
                if (!neighborBlocks[i].isOnNavMesh) continue;

                // 已经检查过的节点，忽略
                if (closedList.Contains(neighborBlocks[i])) continue;

                //neighborBlocks[i].astar_g = CaculateMoveCost(startBlock, neighborBlocks[i]);
                //neighborBlocks[i].astar_h = CaculateMoveCost(endBlock, neighborBlocks[i]);

                // 已经被放入openList的节点，不需重复放入
                if(openList.Contains(neighborBlocks[i]))
                {
                    continue;
                }

                // 记录本节点在路径上的前一个节点，最后回溯路径的时候，根据这个变量，一步步走回头路，回到起点
                neighborBlocks[i].lastBlock = currentBlock;

                // 添加进待检查列表openList
                openList.Add(neighborBlocks[i]);
            }
        }

        // 待检查列表openList被清空后依然没有找到目标位置，说明路径不存在
        return null;
    }

    int CaculateMoveCost(LevelBlock startBlock, LevelBlock endBlock, LevelBlock currentBlock)
    {
        int g = CaculateMoveCost(startBlock, currentBlock);
        int h = CaculateMoveCost(endBlock, currentBlock);

        int f = g + h;

        return f;
    }

    int CaculateMoveCost(LevelBlock startBlock, LevelBlock endBlock)
    {
        int distance_x = startBlock.col - endBlock.col;
        int distance_y = startBlock.row - endBlock.row;

        return distance_x * distance_x + distance_y * distance_y;
    }

    void ResetDebugHighlight()
    {
        if(levelBlocks != null)
        {
            for (int i = 0; i < levelBlocks.Length; i++)
            {
                levelBlocks[i].debugHighlight = false;
            }
        }
    }

    Color GetBlockColorByOcculsion(int occulsion)
    {
        return blockColors[occulsion / 2];
    }

    Color GetBlockColorByThreatScore(int threatScore)
    {
        float threatRate = (float)threatScore / 20;

        Color threatColor = threatColorGradient.Evaluate(threatRate);

        return threatColor;
    }


    void OnGUI()
    {
        return;

        if (!showDebug) return;

        for(int i = 0; i < levelBlocks.Length; i++)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(levelBlocks[i].pos) - Vector3.up * 10;

            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 160, 80), levelBlocks[i].threatScore.ToString());
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        if(levelBlocks != null)
        {
            for (int i = 0; i < levelBlocks.Length; i++)
            {
                if (levelBlocks[i].isOnNavMesh)
                {
                    if (levelBlocks[i].debugHighlight)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(levelBlocks[i].pos, 0.4f);
                    }
                    else
                    {
                        Gizmos.color = GetBlockColorByThreatScore(levelBlocks[i].threatScore);
                        Gizmos.DrawSphere(levelBlocks[i].pos, 0.2f);
                    }
                }
            }
            return;

            if (blockWithHealth != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(blockWithHealth.pos + Vector3.up * 5, new Vector3(0.1f, 10f, 0.1f));
            }

            if(startBlock != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(startBlock.pos + Vector3.up * 0.5f, new Vector3(0.1f, 1f, 0.1f));
            }

            if (endBlock != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(startBlock.pos + Vector3.up * 2.5f, new Vector3(0.3f, 5f, 0.3f));
            }

            if(pathBlocks != null)
            {
                for(int i = 0; i < pathBlocks.Length; i++)
                {
                    float height = Mathf.Lerp(1, 5, (float)i / pathBlocks.Length);
                    Gizmos.color = Color.Lerp(Color.cyan, Color.magenta, (float)i / pathBlocks.Length);
                    Gizmos.DrawCube(pathBlocks[i].pos + Vector3.up * height / 2, new Vector3(0.2f, height, 0.2f));
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(GetPlayerBlock().pos + Vector3.up * 0.5f, new Vector3(0.1f, 1f, 0.1f));

        }
    }
}
