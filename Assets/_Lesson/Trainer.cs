using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class TrainParameters
{
    public float[] factors;

    public override string ToString()
    {
        return string.Format("{0},{1},{2},{3},{4},{5},{6}", factors[0], factors[1], factors[2], factors[3], factors[4], factors[5], factors[6]);
    }
}

public class Trainer : MonoBehaviour
{
    public TrainParameters trainParameters;
    public bool saveToDisk;

    [Range(1, 100)]
    public float trainSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        if(saveToDisk)
        {
            FileUtility.WriteToBinaryFile(trainParameters, "TrainParameters.dat");
        }
        else
        {
            trainParameters = (TrainParameters)FileUtility.ReadFromBinaryFile("TrainParameters.dat");
            GameObject.Find("Player").GetComponent<LevelScanner>().factors = trainParameters.factors;
            Debug.Log(trainParameters.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = trainSpeed;
    }
}
