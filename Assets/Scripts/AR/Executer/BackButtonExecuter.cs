using UnityEngine;
using VirtualClasses;

namespace AR.Executer
{
    public class BackButtonExecuter : MonoBehaviour, IARButtonExecuter
    {

        public void DoAction(GameObject obj)
        {
            ARButtonManager.Instance.PreviousStep();
        }
    }
}
