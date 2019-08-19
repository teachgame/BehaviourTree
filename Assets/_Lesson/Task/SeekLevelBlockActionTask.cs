using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;
#endif

namespace NodeCanvas.Tasks.Actions
{

    [Name("Seek (LevelBlock)")]
    [Category("Movement/Pathfinding")]
    public class SeekLevelBlockActionTask : ActionTask<NavMeshAgent>
    {

        public BBParameter<LevelBlock> targetBlock;
        public BBParameter<float> speed = 4;
        public BBParameter<float> keepDistance = 0.1f;

        private Vector3? lastRequest;

        protected override string info
        {
            get { return "Seek " + targetBlock; }
        }

        protected override void OnExecute()
        {
            agent.speed = speed.value;
            if (Vector3.Distance(agent.transform.position, targetBlock.value.pos) < agent.stoppingDistance + keepDistance.value)
            {
                EndAction(true);
                return;
            }
        }

        protected override void OnUpdate()
        {
            if (lastRequest != targetBlock.value.pos)
            {
                if (!agent.SetDestination(targetBlock.value.pos))
                {
                    EndAction(false);
                    return;
                }
            }

            lastRequest = targetBlock.value.pos;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance.value)
            {
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            if (lastRequest != null && agent.gameObject.activeSelf)
            {
                agent.ResetPath();
            }
            lastRequest = null;
        }

        protected override void OnPause()
        {
            OnStop();
        }
    }
}