using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;

public class ShouldPickHealth : ConditionTask
{
    public BBParameter<LevelBlock> healthBlock;
    public BBParameter<float> healthPickupDistance;
    public BBParameter<int> dangerHealth;
    public BBParameter<int> currentHealth;

    protected override bool OnCheck()
    {
        if(currentHealth.value < 100)
        {
            if (healthBlock != null)
            {
                if (currentHealth.value < dangerHealth.value)
                {
                    return true;
                }

                Vector3 healthDirection = (healthBlock.value.pos - ownerAgent.transform.position);
                if (healthDirection.magnitude < healthPickupDistance.value)
                {                    
                    RaycastHit[] hits = Physics.BoxCastAll(ownerAgent.transform.position, Vector3.one * 3, healthDirection.normalized);

                    int enemyInTheWay = 0;

                    for(int i = 0; i < hits.Length; i++)
                    {
                        CompleteProject.EnemyHealth enemyHealth = hits[i].transform.GetComponent<CompleteProject.EnemyHealth>();

                        if(enemyHealth)
                        {
                            enemyInTheWay++;
                        }
                    }

                    Debug.Log("Enemy In The Way:" + enemyInTheWay);

                    if (currentHealth.value > (enemyInTheWay + 1) * 10)
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
        else
        {
            return false;
        }
    }
}
