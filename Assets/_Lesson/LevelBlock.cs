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

    public bool isOnNavMesh;

    public bool debugHighlight;

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
}
