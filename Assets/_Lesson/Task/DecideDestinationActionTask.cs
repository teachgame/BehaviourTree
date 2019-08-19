using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("Movement/Pathfinding")]
[Name("Decide Destination")]
[Description("查找安全方格")]
public class DecideDestinationActionTask : ActionTask
{
    public BBParameter<LevelBlock> safeBlock;
    public BBParameter<LevelBlock> healthBlock;

    public BBParameter<LevelBlock> destinationBlock;

    private LevelScanner levelScanner;
    private CompleteProject.PlayerHealth playerHealth;

    protected override void OnExecute()
    {
        base.OnExecute();

        levelScanner = ownerAgent.GetComponent<LevelScanner>();
        playerHealth = ownerAgent.GetComponent<CompleteProject.PlayerHealth>();

        //EndAction(true);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        DecideDestination();
        //EndAction(true);
    }

    private void DecideDestination()
    {
        if(playerHealth.currentHealth < playerHealth.startingHealth * 0.8f)
        {
            destinationBlock.value = healthBlock.value;
        }
        else
        {
            destinationBlock.value = safeBlock.value;
        }
    }
}
