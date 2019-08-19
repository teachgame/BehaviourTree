using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("Movement/Pathfinding")]
[Name("Find Health Position")]
[Description("查找药包方格")]
public class FindHealthPositionActionTask : ActionTask
{
    public BBParameter<LevelBlock> healthBlock;
    private LevelScanner levelScanner;

    protected override void OnExecute()
    {
        base.OnExecute();

        levelScanner = ownerAgent.GetComponent<LevelScanner>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        LevelBlock healthBlock = levelScanner.GetHealthBlock();
        this.healthBlock.value = healthBlock;

    }
}
