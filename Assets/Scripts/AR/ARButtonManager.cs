using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;

public class ARButtonManager : MonoBehaviour, IDataPersistence
{
    public static event Action<GameObject> OnArButtonClicked;
    public static ARButtonManager Instance { get; private set; }
    
    private IARAnimation _animationHandler;
    private bool _interactedWithAR = false;
    
    /// <summary>
    /// Usato per navigare tra i panel di spiegazione. un pulsante next fa andare avanti tra i panelli uno di back fa andare indietro
    /// -CANVAS:
    /// Ogni canvas ha un nome del tipo "CanvasStep[n]" con [n] numero dello step
    /// andando avanti viene mostrato un nuvo canvas e nascosto quello vecchio, andando indietro viceversa.
    /// è importante che l'ultimo canvas abbia un pulsante di fine per non rompere tutto
    ///
    /// ANIMAZIONI: per gestire le animazioni il game object deve avere come componente uno script che erediti IARAnimation. Ogni cambio di step viene chiamata la funzione per eseguire l'animazione
    /// </summary>
    private int StepIndex { get; set; }
    
    private void Awake() 
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this;
            DataPersistenceManager.GetInstance().RegisterDataPersistenceObject(Instance);
        }
    }

    private void Start()
    {
        StepIndex = 0;
        _animationHandler = GetComponent<IARAnimation>();
        
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (var canvas in canvases)
        {
            //trova il canvas 0
            if (canvas.name == $"CanvasStep0")
            {
                canvas.gameObject.SetActive(true);
                continue;
            }
            
            if (canvas.name.Contains("CanvasStep"))
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))) return;
        var touch = Input.GetTouch(0);
        var pointerEventData = new PointerEventData(EventSystem.current){ position = touch.position};
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        if(raycastResults.Count > 0)
        {
            foreach(var result in raycastResults)
            {
                //creo un azione di button clicked
                if (result.gameObject.CompareTag("ARButton"))
                {
                    _interactedWithAR = true;
                    OnArButtonClicked?.Invoke(result.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Mostra il canvas successivo a "CanvasStep[n]"
    /// </summary>
    public void NextStep()
    {
        NavigateStep(StepIndex + 1);
    }
    
    /// <summary>
    /// Mostra il canvas precedente a "CanvasStep[n]"
    /// </summary>
    public void PreviousStep()
    {
        NavigateStep(StepIndex - 1);
    }

    private void NavigateStep(int newStepIndex)
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (var canvas in canvases)
        {
            //trova il canvas all'indice vecchio e lo nasconde
            if (canvas.name == $"CanvasStep{StepIndex}")
            {
                canvas.gameObject.SetActive(false);
                continue;
            }
            //trova il canvas all'indice nuovo e lo mostra
            if (canvas.name == $"CanvasStep{newStepIndex}")
            {
                canvas.gameObject.SetActive(true);
                continue;
            }
        }
        StepIndex = newStepIndex;
        _animationHandler?.DoAnimation(StepIndex);
    }

    public void LoadData(GameData data)
    {
        _interactedWithAR = data.interactedWithArToday.Value;
    }

    public void SaveData(GameData data)
    {
        data.interactedWithArToday.Value = _interactedWithAR;
    }
}
