using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager), typeof(ARPlaneManager))]
public class ARPrefabManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ARSceneExplanationPrefabs;

    [SerializeField]
    private GameObject DefaultExplanationPrefab;

    private ARTrackedImageManager _trackedImageManager;
    private GameObject _explanationPrefab;

    void Awake()
    {
        var dataManager = DataPersistenceManager.GetInstance();
        var topicName = dataManager.CurrentTopic.Name;
        var planeManager = GetComponent<ARPlaneManager>();
        _explanationPrefab = ARSceneExplanationPrefabs.Find(x => x.name == topicName);
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        _trackedImageManager.requestedMaxNumberOfMovingImages = 1;
        if (dataManager.ARVisualType == ARVisualType.PlaneDetection)
        {
            _trackedImageManager.enabled = false;
            planeManager.enabled = true;
            planeManager.planePrefab = _explanationPrefab != null ? _explanationPrefab : DefaultExplanationPrefab;
            _trackedImageManager.trackedImagePrefab = null;
        }
        else
        {
            _trackedImageManager.enabled = true;
            planeManager.enabled = false;
            planeManager.planePrefab = null;
            _trackedImageManager.trackedImagePrefab = _explanationPrefab != null ? _explanationPrefab : DefaultExplanationPrefab;
        }
    }


    // Il codice seguente, per qualche motivo, non funziona
    // il tracked image manager trova correttamente le immagini, e il prefab viene instanziato ma per qualche motivo non compare nell'AR
    // se invece uso come prefab un semplice prefab contenente un cubo e basta, allora si vede, onestamente non capisco quale sia il problema
    // e' palesemente legato al "tipo" di prefab ma non so come risolvere
    // nonostante la documentazione dica di non usare "trackedImagePrefab" come content, io lo uso lo stesso perche' almeno li funziona
    // non so cosa faccia di diverso se non un semplice "Instantiate" ma a quanto pare lo fa funzionare

    // private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new();
    // 
    // void OnEnable()
    // {
    //     _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    // }
    // 
    // void OnDisable()
    // {
    //     _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    // }
    // 
    // void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    // {
    //     foreach (var newImage in args.added)
    //     {
    //         if (!_instantiatedPrefabs.ContainsKey(newImage.referenceImage.name))
    //         {
    //             var newPrefab = Instantiate(_explanationPrefab != null ? _explanationPrefab : DefaultExplanationPrefab, newImage.transform);
    //             _instantiatedPrefabs[newImage.referenceImage.name] = newPrefab;
    //         }
    //     }
    // 
    //     foreach (var updatedImage in args.updated)
    //     {
    //         if (_instantiatedPrefabs.TryGetValue(updatedImage.referenceImage.name, out var gameObject))
    //         {
    //             gameObject.SetActive(updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking);
    //         }
    //     }
    // 
    //     foreach (var removedImage in args.removed)
    //     {
    //         Destroy(_instantiatedPrefabs[removedImage.referenceImage.name]);
    //         _instantiatedPrefabs.Remove(removedImage.referenceImage.name);
    //     }
    // }
}
