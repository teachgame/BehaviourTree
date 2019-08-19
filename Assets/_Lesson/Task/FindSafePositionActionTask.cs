using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("Movement/Pathfinding")]
[Name("Find Safe Position")]
[Description("查找安全方格")]
public class FindSafePositionActionTask : ActionTask
{
    public BBParameter<LevelBlock> safeBlock;
    public BBParameter<List<LevelBlock>> safePath;
    private LevelScanner levelScanner;

    protected override void OnExecute()
    {
        base.OnExecute();

        levelScanner = ownerAgent.GetComponent<LevelScanner>();

        //EndAction(true);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        FindSafePosition();
        //EndAction(true);
    }

    private void FindSafePosition()
    {
        LevelBlock[] safePathNodes = levelScanner.GetSafePath(ownerAgent.transform.position);

        if (safePathNodes.Length > 0)
        {
            safeBlock.value = safePathNodes[safePathNodes.Length - 1];

            safePath.value.Clear();

            for (int i = 0; i < safePathNodes.Length; i++)
            {
                safePath.value.Add(safePathNodes[i]);
            }
        }

        //safeBlock.value = levelScanner.GetSafePosition();
    }
}
