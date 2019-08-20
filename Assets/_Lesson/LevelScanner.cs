using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelScanner : MonoBehaviour
{
    public Gradient threatColorGradient;

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

    Vector3 offset = new Vector3(-36, 0, -36);

    float evaluateTimer = 0;

    public GameObject healthPrefab;
    private GameObject healthInstance;

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

                Vector3 pos = new Vector3(t * 2, 0, i * 2) + offset;
                levelBlocks[idx] = new LevelBlock(i, t, pos);

                NavMeshHit hit;

                if(NavMesh.SamplePosition(pos, out hit, 0.2f, NavMesh.AllAreas))
                {
                    levelBlocks[idx].pos = hit.position;
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
                    levelBlocks[i].enemyCount = CheckEnemyCount(levelBlocks[i].pos, 1f);
                }

                evaluateTimer = 0;
            }
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            startBlock = GetPlayerBlock();

            for (int i = 0; i < levelBlocks.Length; i++)
            {
                levelBlocks[i].EvaluateThreat(factors);
                //levelBlocks[i].threatScore = EvaluateThreatScore(levelBlocks[i], 5);
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

    int EvaluateThreatScore(LevelBlock levelBlock, int radius)
    {
        int threatScore = 0;

        List<LevelBlock> blocksInRange = GetBlocksInRange(levelBlock.pos, radius);

        for(int i = 0; i < blocksInRange.Count; i++)
        {
            int relativePos = Mathf.Abs(levelBlock.row - blocksInRange[i].row) + Mathf.Abs(levelBlock.col - blocksInRange[i].col);

            int baseScore = (int)(blocksInRange[i].enemyCount * (1 + (float)blocksInRange[i].occulsion / 8) * 10 + blocksInRange[i].occulsion);
            float scoreFactor = 1f;

            if (relativePos == 0)
            {
                scoreFactor = 1f;
            }

            if (relativePos == 1)
            {
                scoreFactor = 0.8f;
            }

            if (relativePos == 2)
            {
                scoreFactor = 0.5f;
            }

            if (relativePos == 3)
            {
                scoreFactor = 0.4f;
            }

            if (relativePos > 3)
            {
                scoreFactor = 0;
            }

            threatScore += (int)(baseScore * scoreFactor);
        }

        //Debug.Log(levelBlock.row + "," + levelBlock.col + ":" + threatScore);
        return threatScore;
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

        startBlock = GetBlock(transform.position);

        pathBlocks = AStarPathfinding(startBlock, endBlock);

        return pathBlocks;
    }

    public LevelBlock[] GetSafePath(Vector3 center, int pathNodeCount, int radius, float agreesion)
    {
        ResetDebugHighlight();

        LevelBlock[] results = new LevelBlock[pathNodeCount];
        List<LevelBlock> blocksInRange = new List<LevelBlock>();

        for (int i = 0; i < levelBlocks.Length; i++)
        {
            levelBlocks[i].EvaluateThreat(factors);
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
        int myBlockX;
        int myBlockY;

        GetBlockPosByPosition(transform.position, out myBlockX, out myBlockY);

        for (int i = 0; i < levelBlocks.Length; i++)
        {
            levelBlocks[i].EvaluateThreat(factors);
            //levelBlocks[i].threatScore = EvaluateThreatScore(levelBlocks[i], 5);
        }

        LevelBlock levelBlock = GetSafeBlockInRange(myBlockY, myBlockX, radius);
        currentTargetBlockIdx = levelBlock.row * col + levelBlock.col;
        return levelBlock;
    }

    public LevelBlock GetSafeBlockInRange(int centerRow, int centerCol, int offset)
    {
        //ResetDebugHighlight();

        LevelBlock centerBlock = GetBlock(centerRow, centerCol);

        if(centerBlock != null)
        {
            List<LevelBlock> blocksInRange = GetBlocksInRange(centerBlock.pos, offset);

            int saftestBlockIdx = 0;

            for (int i = 0; i < blocksInRange.Count; i++)
            {
                if (!blocksInRange[i].isOnNavMesh) continue;

                if (blocksInRange[i].threatScore <= levelBlocks[saftestBlockIdx].threatScore)
                {
                    saftestBlockIdx = i;
                }
            }

            return blocksInRange[saftestBlockIdx];
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

        if(blockIdx == Mathf.Clamp(blockIdx, 0, levelBlocks.Length))
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
        blockPos_x = (int)(position.x - offset.x) / 2;
        blockPos_y = (int)(position.z - offset.z) / 2;

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
                if(levelBlocks[i].isOnNavMesh)
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

            if(blockWithHealth != null)
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
        }
    }
}
