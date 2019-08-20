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

    public float occulsion_reverse { get { return (occulsion == 0 ?  0 : 1f / occulsion); } }

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

    public void EvaluateThreat(float[] factors)
    {
        this.threatScore =
            (int)(occulsion * factors[0] + enemyCount * factors[1] +
            occulsion_reverse * factors[2] + enemyCount_reverse * factors[3] +
            occulsion_mul_enemyCount * factors[4] + occulsion_div_enemyCount * factors[5] + enemyCount_div_occulsion * factors[6]);
    }
}
