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

    Vector3 offset = new Vector3(-36, 0, -36);

    float evaluateTimer = 0;

    public GameObject healthPrefab;
    private GameObject healthInstance;

    private LevelBlock blockWithHealth;

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
            TestSort();
        }
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

        for (int offset_col = -radius; offset_col <= radius; offset_col++)
        {
            for (int offset_row = -radius; offset_row <= radius; offset_row++)
            {
                int neighborRow = levelBlock.row + offset_row;
                int neighborCol = levelBlock.col + offset_col;

                bool neighborRowInside = Mathf.Clamp(neighborRow, 0, this.row - 1) == neighborRow;
                bool neighborColInside = Mathf.Clamp(neighborCol, 0, this.col - 1) == neighborCol;

                if (neighborColInside && neighborRowInside)
                {
                    int currentBlockIdx = neighborRow * this.col + neighborCol;
                    LevelBlock neighborBlock = levelBlocks[currentBlockIdx];

                    if (neighborBlock.isOnNavMesh)
                    {
                        int relativePos = Mathf.Abs(offset_row) + Mathf.Abs(offset_col);

                        int baseScore = (int)(neighborBlock.enemyCount * (1 + (float)neighborBlock.occulsion / 8) * 100 + neighborBlock.occulsion);
                        float scoreFactor = 1f;

                        if (relativePos == 0)
                        {
                            scoreFactor = 1f;
                        }

                        if(relativePos == 1)
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
                }
            }
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

                //for(int offset_col = -1; offset_col < 2; offset_col++)
                //{
                //    for (int offset_row = -1; offset_row < 2; offset_row++)
                //    {
                //        int neighborRow = row + offset_row;
                //        int neighborCol = col + offset_col;

                //        bool neighborRowInside = Mathf.Clamp(neighborRow, 0, this.row - 1) == neighborRow;
                //        bool neighborColInside = Mathf.Clamp(neighborCol, 0, this.col - 1) == neighborCol;

                //        if(neighborColInside && neighborRowInside)
                //        {
                //            LevelBlock neighborBlock = levelBlocks[neighborRow * this.col + neighborCol];
                //            if(!neighborBlock.isOnNavMesh)
                //            {
                //                occulsion += 1;
                //            }
                //        }
                //        else
                //        {
                //            occulsion += 1;
                //        }
                //    }
                //}

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

    public List<LevelBlock> GetBlocksInRange(Vector3 center, int radius)
    {
        int centerCol;
        int centerRow;

        GetBlockPosByPosition(center, out centerCol, out centerRow);

        List<LevelBlock> blocks = new List<LevelBlock>();

        for (int offset_col = -radius; offset_col <= radius; offset_col++)
        {
            for (int offset_row = -radius; offset_row <= radius; offset_row++)
            {
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

    public LevelBlock[] GetSafePath(Vector3 center, int pathNodeCount, int radius, float agreesion)
    {
        ResetDebugHighlight();

        LevelBlock[] results = new LevelBlock[pathNodeCount];
        List<LevelBlock> blocksInRange = new List<LevelBlock>();

        for (int i = 0; i < levelBlocks.Length; i++)
        {
            levelBlocks[i].threatScore = EvaluateThreatScore(levelBlocks[i], 5);
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

    public LevelBlock GetSafePosition()
    {
        int myBlockX;
        int myBlockY;

        GetBlockPosByPosition(transform.position, out myBlockX, out myBlockY);

        for (int i = 0; i < levelBlocks.Length; i++)
        {
            levelBlocks[i].threatScore = EvaluateThreatScore(levelBlocks[i], 5);
        }

        LevelBlock levelBlock = GetSafeBlockInRange(myBlockY, myBlockX, 3);
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
                if (blocksInRange[i].threatScore <= levelBlocks[saftestBlockIdx].threatScore)
                {
                    saftestBlockIdx = i;
                }
            }

            return blocksInRange[saftestBlockIdx];
        }

        return null;

        //int saftestBlockIdx = centerRow * this.col + centerCol;

        //for (int offset_col = -offset; offset_col <= offset; offset_col++)
        //{
        //    for (int offset_row = -offset; offset_row <= offset; offset_row++)
        //    {
        //        int neighborRow = centerRow + offset_row;
        //        int neighborCol = centerCol + offset_col;

        //        bool neighborRowInside = Mathf.Clamp(neighborRow, 0, this.row - 1) == neighborRow;
        //        bool neighborColInside = Mathf.Clamp(neighborCol, 0, this.col - 1) == neighborCol;

        //        if (neighborColInside && neighborRowInside)
        //        {
        //            int currentBlockIdx = neighborRow * this.col + neighborCol;
        //            LevelBlock neighborBlock = levelBlocks[currentBlockIdx];

        //            if (neighborBlock.isOnNavMesh)
        //            {
        //                neighborBlock.debugHighlight = true;

        //                if(neighborBlock.threatScore <= levelBlocks[saftestBlockIdx].threatScore)
        //                {
        //                    saftestBlockIdx = currentBlockIdx;
        //                }
        //            }
        //        }
        //    }
        //}

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
        }
    }
}
