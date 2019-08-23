using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("GameObject")]
[Name("Find Enemies In Range")]
[Description("找到范围内的敌人")]
public class FindEnemiesInRangeActionTask : ActionTask
{
    public BBParameter<GameObject> target;
    public BBParameter<List<GameObject>> enemies;

    public BBParameter<int> enemyCountInBombRange;

    public BBParameter<float> radius;

    protected override void OnExecute()
    {
        base.OnExecute();

        FindEnemy();

        if (target.value != null)
        {
            CompleteProject.EnemyHealth enemyHealth = target.value.GetComponent<CompleteProject.EnemyHealth>();
            if(enemyHealth.currentHealth <= 0)
            {
                target.value = null;
            }
        }

        EndAction(true);
    }

    private void FindEnemy()
    {
        List<GameObject> enemies = new List<GameObject>();
        Collider[] cols = Physics.OverlapSphere(ownerAgent.transform.position, radius.value);
        Debug.DrawLine(ownerAgent.transform.position, ownerAgent.transform.position + ownerAgent.transform.forward * radius.value, Color.yellow);
        //Debug.Log(cols.Length);

        int nearestEnemyIdx = 0;
        float nearestSqrDistance = 10000;

        enemyCountInBombRange.value = 0;

        for(int i = 0; i < cols.Length; i++)
        {
            Vector3 ownerCenter = ownerAgent.GetComponent<Collider>().bounds.center;
            Vector3 targetCenter = cols[i].bounds.center;

            Vector3 direction = (targetCenter - ownerCenter).normalized;

            RaycastHit hit;

            if(Physics.Raycast(ownerCenter, direction, out hit))
            {
                if (hit.collider != cols[i]) continue;
            }

            CompleteProject.EnemyHealth enemyHealth = cols[i].GetComponent<CompleteProject.EnemyHealth>();
            //Debug.Log("Find:" + cols[i].gameObject.name);

            if (enemyHealth && enemyHealth.currentHealth > 0)
            {
                //Debug.Log("Enemy:" + cols[i].gameObject.name);
                if (enemies.Contains(enemyHealth.gameObject)) continue;

                float sqrDistance = Vector3.SqrMagnitude(ownerAgent.transform.position - enemyHealth.transform.position);

                if(sqrDistance < 6 * 6)
                {
                    enemyCountInBombRange.value++;
                }

                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestEnemyIdx = enemies.Count;
                }

                enemies.Add(enemyHealth.gameObject);
            }
        }

        if(enemies.Count > 0)
        {
            //Debug.Log("Find Enemy:" + enemies.Count);
            if(this.target.value == null)
            {
                this.enemies.value = enemies;
                this.target.value = this.enemies.value[nearestEnemyIdx];
            }
        }
        else
        {
            this.target.value = null;
        }
    }
}
