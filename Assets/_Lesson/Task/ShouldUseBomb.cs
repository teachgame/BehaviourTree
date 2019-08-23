using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;

public class ShouldUseBomb : ConditionTask
{
    public BBParameter<int> energyConsume;
    public BBParameter<int> currentEnergy;
    public BBParameter<int> enemyCountToTriggerBomb;
    public BBParameter<int> enemyCount;
    public BBParameter<int> dangerHealth;
    public BBParameter<int> currentHealth;

    protected override bool OnCheck()
    {
        if(currentEnergy.value >= energyConsume.value)
        {
            if (currentHealth.value <= dangerHealth.value)
            {
                if(enemyCount.value > 0)
                {
                    return true;
                }
            }
            else
            {
                if (enemyCount.value >= enemyCountToTriggerBomb.value)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            return false;
        }
    }
}
