using UnityEngine;
using VirtualClasses;

namespace AR.Executer
{
    public class NextButtonExecuter : MonoBehaviour, IARButtonExecuter
    {

        public void DoAction(GameObject obj)
        {
            ARButtonManager.Instance.NextStep();
        }
    }
}
