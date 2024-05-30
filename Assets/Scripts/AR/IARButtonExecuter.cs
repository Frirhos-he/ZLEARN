using UnityEngine;

namespace VirtualClasses
{
    //classe virtuale usata per identificare l'azione da fare quando viene cliccato un pulsante
    public interface IARButtonExecuter
    {
        public void DoAction(GameObject obj);
    }
}
