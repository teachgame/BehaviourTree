using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;
#endif

namespace NodeCanvas.Tasks.Actions
{

    [Category("Movement/Pathfinding")]
    [Description("Move along level blocks path taken from the list provided")]
    public class MoveAlongPathActionTask : ActionTask<NavMeshAgent>
    {
        [RequiredField]
        public BBParameter<List<LevelBlock>> targetList;
        public BBParameter<float> speed = 4;
        public BBParameter<float> keepDistance = 0.1f;

        private int index = -1;
        private Vector3? lastRequest;

        protected override string info
        {
            get { return string.Format("{0} Move Along Path", targetList); }
        }

        protected override void OnExecute()
        {
            if (targetList.value.Count == 0)
            {
                EndAction(false);
                return;
            }

            if (targetList.value.Count == 1)
            {

                index = 0;

            }
            else
            {
                index = (int)Mathf.Min(index + 1, targetList.value.Count - 1);
            }

            var targetGo = targetList.value[index];
            if (targetGo == null)
            {
                Debug.LogWarning("List's game object is null on MoveToFromList Action");
                EndAction(false);
                return;
            }

            var targetPos = targetGo.pos;

            agent.speed = speed.value;
            if ((agent.transform.position - targetPos).magnitude < agent.stoppingDistance + keepDistance.value)
            {
                EndAction(true);
                return;
            }
        }

        protected override void OnUpdate()
        {
            index = Mathf.Clamp(index, 0, targetList.value.Count - 1);
            var targetPos = targetList.value[index].pos;
            if (lastRequest != targetPos)
            {
                if (!agent.SetDestination(targetPos))
                {
                    EndAction(false);
                    return;
                }
            }

            lastRequest = targetPos;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance.value)
            {
                EndAction(true);
            }
        }

        protected override void OnPause() { OnStop(); }
        protected override void OnStop()
        {
            if (lastRequest != null && agent.gameObject.activeSelf)
            {
                agent.ResetPath();
            }
            lastRequest = null;
        }

        public override void OnDrawGizmosSelected()
        {
            if (agent && targetList.value != null)
            {
                foreach (var go in targetList.value)
                {
                    if (go != null)
                    {
                        Gizmos.DrawSphere(go.pos, 0.1f);
                    }
                }
            }
        }
    }
}