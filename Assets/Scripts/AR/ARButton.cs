using System;
using UnityEngine;
using UnityEngine.Serialization;
using VirtualClasses;

namespace AR
{
    /// <summary>
    /// Si mette in ascolto di ButtonClicked di ARButtonManager per identificare che il pulsante cliccato sia quello dell'oggetto che contiene lo script
    /// imposta il tag a ARButton e cerca all'interno del game object un component che erediti IARButtonExecuter il quale ha la funzione da eseguire quando si clicca un pulsante
    /// </summary>
    public class ARButton: MonoBehaviour
    {
        private IARButtonExecuter _executer;
        public Guid Uid { get; private set; }

        private void Start()
        {
            _executer = GetComponent<IARButtonExecuter>();
            tag = "ARButton";
            Uid = Guid.NewGuid();
        }
        
        private void OnEnable()
        {
            ARButtonManager.OnArButtonClicked += ButtonClicked;
        }
        
        private void OnDisable()
        {
            ARButtonManager.OnArButtonClicked -= ButtonClicked;
        }

        private void ButtonClicked(GameObject obj)
        {
            //controllo che il pulsante cliccato sia lo stesso di quello corrente
            if (obj.GetComponent<ARButton>()?.Uid != Uid)
                return;
            _executer?.DoAction(obj);
        }


    }
}
