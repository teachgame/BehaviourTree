using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelBlock : IComparable<LevelBlock>
{
    public int row;
    public int col;

    public Vector3 pos;
    public int threatScore;
    public int occulsion;

    public int enemyCount;

    public int occulsion_mul_enemyCount { get { return occulsion * enemyCount; } }

    public float occulsion_square { get { return (occulsion * occulsion); } }

    public float enemyCount_reverse { get { return (enemyCount == 0 ? 0 : 1f / enemyCount); } }

    public float occulsion_div_enemyCount { get { return (enemyCount == 0 ? 0 : (float)occulsion / enemyCount); } }

    public float enemyCount_div_occulsion { get { return (occulsion == 0 ? 0 : (float)enemyCount / occulsion); } }

    public bool isOnNavMesh;

    public bool debugHighlight;

    public bool isAstarChecked;

    public int astar_f { get { return astar_g + astar_h; } }
    public int astar_g;
    public int astar_h;

    public LevelBlock lastBlock;

    public LevelBlock(int row, int col,Vector3 pos)
    {
        this.row = row;
        this.col = col;
        this.pos = pos;
    }

    // CompareTo方法的返回值： 小于：-1， 大于：+1，等于：0 
    public int CompareTo(LevelBlock other)
    {
        int result = this.threatScore.CompareTo(other.threatScore);
        //Debug.Log(this.threatScore + ":" + other.threatScore + ",result:" + result);
        return result;
    }

    public void EvaluateThreat(List<LevelBlock> blocksInRange, float[] factors)
    {
        if (blocksInRange == null || blocksInRange.Count == 0) return;

        this.threatScore = 0;

        for (int i = 0; i < blocksInRange.Count; i++)
        {
            int relativePos = Mathf.Abs(this.row - blocksInRange[i].row) + Mathf.Abs(this.col - blocksInRange[i].col);

            int baseScore = (int)(blocksInRange[i].occulsion * factors[0] + blocksInRange[i].enemyCount * factors[1] +
            blocksInRange[i].occulsion_square * factors[2] + blocksInRange[i].enemyCount_reverse * factors[3] +
            blocksInRange[i].occulsion_mul_enemyCount * factors[4] + blocksInRange[i].occulsion_div_enemyCount * factors[5] + blocksInRange[i].enemyCount_div_occulsion * factors[6]);

            float scoreFactor = 1f / (1 + relativePos * relativePos);

            //if (relativePos == 0)
            //{
            //    scoreFactor = 1f;
            //}

            //if (relativePos == 1)
            //{
            //    scoreFactor = 1f;
            //}

            //if (relativePos == 2)
            //{
            //    scoreFactor = 0.8f;
            //}

            //if (relativePos == 3)
            //{
            //    scoreFactor = 0.8f;
            //}

            //if (relativePos > 3)
            //{
            //    scoreFactor = 0;
            //}

            this.threatScore += (int)(baseScore * scoreFactor);
        }
    }
}
