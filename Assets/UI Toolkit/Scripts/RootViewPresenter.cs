using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RootViewPresenter : MonoBehaviour, IDataPersistence
{
    private VisualElement _settingsView;
    private VisualElement _settingsPopup;
    private Button _startButton, _openSettingsButton, _quitButton;
    private Button _closeSettingsButton;
    private Button _resetSaveDataButton;
    private SavedSettings _savedSettings;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        _settingsView = root.Q<VisualElement>("SettingsView");
        _settingsPopup = root.Q<VisualElement>("SettingsPopupContainer");

        _openSettingsButton = root.Q<Button>("SettingsButton");
        _openSettingsButton.RegisterCallback<ClickEvent>(OnOpenSettingsButtonClicked);

        _closeSettingsButton = root.Q<Button>("CloseSettingsButton");
        _closeSettingsButton.RegisterCallback<ClickEvent>(OnCloseSettingsButtonClicked);

        _quitButton = root.Q<Button>("QuitButton");
        _quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClicked);

        _startButton = root.Q<Button>("StartButton");
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);

        _resetSaveDataButton = root.Q<Button>("ResetSaveFileButton");
        _resetSaveDataButton.clicked += () =>
        {
            DataPersistenceManager.GetInstance().ResetGame();
        };
        var soundToggle = root.Q<Toggle>("SoundToggle");
        soundToggle.value = _savedSettings.soundEnabled;
        soundToggle.RegisterCallback<ChangeEvent<bool>>((ev) =>
        {
            _savedSettings.soundEnabled = ev.newValue;
        });

        var volumeSlider = root.Q<Slider>("VolumeSlider");
        volumeSlider.value = _savedSettings.musicVolume;
        volumeSlider.RegisterCallback<ChangeEvent<float>>((ev) =>
        {
            _savedSettings.musicVolume = ev.newValue;
        });

        var vibrationToggle = root.Q<Toggle>("Vibration");
        vibrationToggle.value = _savedSettings.vibrationEnabled;
        vibrationToggle.RegisterCallback<ChangeEvent<bool>>((ev) =>
        {
            _savedSettings.vibrationEnabled = ev.newValue;
        });

        var dataManager = DataPersistenceManager.GetInstance();
        var arPlanesToggle = root.Q<Toggle>("ARSettingsToggle");
        arPlanesToggle.value = _savedSettings.arPlanesEnabled;
        dataManager.ARVisualType = _savedSettings.arPlanesEnabled ? ARVisualType.PlaneDetection : ARVisualType.Static;
        arPlanesToggle.RegisterCallback<ChangeEvent<bool>>((ev) =>
        {
            _savedSettings.arPlanesEnabled = ev.newValue;
            dataManager.ARVisualType = _savedSettings.arPlanesEnabled ? ARVisualType.PlaneDetection : ARVisualType.Static;
        });
    }

    private void OnOpenSettingsButtonClicked(ClickEvent evt)
    {
        // Show the settings view
        _settingsView.style.display = DisplayStyle.Flex;

        // Trigger the fadein animation
        _settingsPopup.AddToClassList("settings-popup-fadein");
    }

    private void OnCloseSettingsButtonClicked(ClickEvent evt)
    {
        // Show the settings view
        _settingsView.style.display = DisplayStyle.None;

        // Trigger the fadein animation
        _settingsPopup.RemoveFromClassList("settings-popup-fadein");
    }

    private void OnQuitButtonClicked(ClickEvent evt)
    {
        Application.Quit();
    }

    private void OnStartButtonClicked(ClickEvent evt)
    {
        DataPersistenceManager.GetInstance().SaveGame();
        SceneManager.LoadScene("MainScene");
    }

    public void LoadData(GameData data)
    {
        _savedSettings = data.savedSettings;
    }

    public void SaveData(GameData data)
    {
        data.savedSettings = _savedSettings;
    }
}
