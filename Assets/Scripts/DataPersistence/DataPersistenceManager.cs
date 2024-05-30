using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataAssetLoader
{
    public static T Load<T>(TextAsset asset)
    {
        return JsonUtility.FromJson<T>(asset.text);
    }
}

public enum ARVisualType
{
    PlaneDetection,
    Static
}

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    [Header("Chapters data")]
    [SerializeField] private TextAsset chaptersData;

    [Header("Achievements data")]
    [SerializeField] private TextAsset achievementsData;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    
    //chapters
    private Chapters chapters;
    public Chapters GetChapters() { return chapters; }
    //achievements
    private Achievements achievements;
    public Achievements GetAchievements() { return achievements; }
    //current selected chapter - this is here because it allows us to go from AR to the current chapter when clicking the home button
#nullable enable
    private Chapter? currentChapter = null;
    public Chapter? CurrentChapter { get => currentChapter; set => currentChapter = value; }
#nullable disable
    
    //Quando si esce dalla schermata di AR cliccando il pulsante di skip allora questo flag viene settato a true
    public bool IsSkipButtonpressed { get; set; }

    // Quando si esce dalla schermata di AR finendo in modo standard la spiegazione
    public bool IsFinishButtonPressed { get; set; }

    private static DataPersistenceManager Instance { get; set; }
    public Topic CurrentTopic { get; set; }
    public bool IsInQuizSummary { get; set; }

    public ARVisualType ARVisualType { get; set; } 

    public static DataPersistenceManager GetInstance() { return Instance; }


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than one Data Persistence Manager, destroying the newest one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        dataHandler = new(Application.persistentDataPath, fileName);
        chapters = DataAssetLoader.Load<Chapters>(chaptersData);
        achievements = DataAssetLoader.Load<Achievements>(achievementsData);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjects = FindAllDataPersistanceObjects();
        LoadGame();
    }
    public void RegisterDataPersistenceObject(IDataPersistence obj)
    {
        dataPersistenceObjects.Add(obj);
        obj.LoadData(gameData);
    }
    public void NewGame()
    {
        gameData = new();
    }
    public void LoadGame()
    {
        gameData = dataHandler.Load();
        if (gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults");
            NewGame();
        }
        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
        {
            dataPersistence.LoadData(gameData);
        }

    }
    public void SaveGame()
    {
        foreach (IDataPersistence dataPersistence in dataPersistenceObjects)
        {
            dataPersistence.SaveData(gameData);
        }
        dataHandler.Save(gameData);
    }

    public void ResetGame()
    {
        gameData.ResetData();
        SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGame();
        }
    }

    private List<IDataPersistence> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistanceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistanceObjects);
    }
}
