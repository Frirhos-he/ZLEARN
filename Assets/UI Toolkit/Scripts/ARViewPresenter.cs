using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ARViewPresenter : MonoBehaviour, IDataPersistence
{
    private VisualElement _backHomeButtonContainer, _feedbackButtonContainer, _skipButtonContainer, _dialogContainer;
    private VisualElement _feedbackFormDialog, _feedbackConfirmationDialog, _goHomeConfirmationDialog, _skipDemoConfirmationDialog;
    private MyButton _backHomeButton, _feedbackButton, _skipButton;
    private Button _closeFeedbackButton, _sendFeedbackButton, _closeFeedbackConfirmDialogButton;
    private Button _goHomeYesButton, _goHomeNoButton, _skipDemoYesButton, _skipDemoNoButton;
    
    private Toggle _t1, _t2, _t3;
    private TextField _commentField;

    private GameData _gameData;


    void Start()
    {
        var dataManager = DataPersistenceManager.GetInstance();
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        _backHomeButtonContainer = root.Q<VisualElement>("BackHomeButtonContainer");
        _feedbackButtonContainer = root.Q<VisualElement>("FeedbackButtonContainer");
        _skipButtonContainer = root.Q<VisualElement>("SkipButtonContainer");

        _dialogContainer = root.Q<VisualElement>("DialogContainer");
        _feedbackFormDialog = root.Q<VisualElement>("FeedbackFormDialog");
        _feedbackConfirmationDialog = root.Q<VisualElement>("FeedbackConfirmationDialog");
        _goHomeConfirmationDialog = root.Q<VisualElement>("GoHomeConfirmationDialog");
        _skipDemoConfirmationDialog = root.Q<VisualElement>("SkipDemoConfirmationDialog");

        // Feedback form input fields
        _t1 = root.Q<Toggle>("QuestionErrorToggle");
        _t2 = root.Q<Toggle>("ConceptToggle");
        _t3 = root.Q<Toggle>("OtherToggle");
        _commentField = root.Q<TextField>("CommentTextField");
        _commentField.SetPlaceholderText("Add a comment...");

        // Add OnChange events to the toggles and text field
        _t1.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
        _t2.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
        _t3.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
        _commentField.RegisterCallback<ChangeEvent<string>>(OnTextFieldChanged);

        // X button in the feedback dialog
        _closeFeedbackButton = root.Q<Button>("CloseFeedbackButton");
        _closeFeedbackButton.clicked += () => 
        {
            CloseDialog(_feedbackFormDialog);
            ResetFeedbackForm();
        };

        // Submit button for sending feedback
        _sendFeedbackButton = root.Q<Button>("SendFeedbackButton");
        _sendFeedbackButton.SetEnabled(false);
        _sendFeedbackButton.style.backgroundColor = new StyleColor(new Color32(249, 170, 51, 255));
        _sendFeedbackButton.clicked += () => 
        {
            _feedbackFormDialog.style.display = DisplayStyle.None;
            _feedbackConfirmationDialog.style.display = DisplayStyle.Flex;
            _gameData.sentFeedbackToday.Value = true;
        };

        // Close button after sending feedback
        _closeFeedbackConfirmDialogButton = root.Q<Button>("CloseFeedbackConfirmDialogButton");
        _closeFeedbackConfirmDialogButton.clicked += () => 
        {
            CloseDialog(_feedbackConfirmationDialog);
            ResetFeedbackForm();
        };

        // Home button in the AR view
        _backHomeButton = root.Q<MyButton>("BackHomeButton");
        SetButtonAnimation(_backHomeButton, _backHomeButtonContainer);
        _backHomeButton.clicked += () => OpenDialog(_goHomeConfirmationDialog);

        // Go Home yes/no buttons
        _goHomeNoButton = root.Q<Button>("GoHomeNoButton");
        _goHomeNoButton.clicked += () => CloseDialog(_goHomeConfirmationDialog);

        _goHomeYesButton = root.Q<Button>("GoHomeYesButton");
        _goHomeYesButton.clicked += () =>
        {
            dataManager.SaveGame();
            dataManager.CurrentTopic = null;
            SceneManager.LoadScene("MainScene");
        };
        
        root.Q<Label>("ARChapterNameLabel").text = $"Ch. {dataManager.CurrentChapter?.number}: {dataManager.CurrentChapter?.name}";
        root.Q<Label>("ARChapterTopicNameLabel").text = dataManager.CurrentTopic.name;

        // Feedback button in the AR view
        _feedbackButton = root.Q<MyButton>("FeedbackButton");
        SetButtonAnimation(_feedbackButton, _feedbackButtonContainer);
        _feedbackButton.clicked += () =>
        {
            _sendFeedbackButton.SetEnabled(false);
            OpenDialog(_feedbackFormDialog);
        };

        // Skip button in the AR view
        _skipButtonContainer = root.Q<VisualElement>("SkipButtonContainer");
        _skipButton = root.Q<MyButton>("SkipButton");
        SetButtonAnimation(_skipButton, _skipButtonContainer);
        _skipButton.clicked += () => OpenDialog(_skipDemoConfirmationDialog);

        // Skip demo yes/no button
        _skipDemoNoButton = root.Q<Button>("SkipDemoNoButton");
        _skipDemoNoButton.clicked += () => CloseDialog(_skipDemoConfirmationDialog);

        _skipDemoYesButton = root.Q<Button>("SkipDemoYesButton");
        _skipDemoYesButton.clicked += () => 
        {
            dataManager.SaveGame();
            dataManager.IsSkipButtonpressed = true;
            SceneManager.LoadScene("MainScene");
        };
    }

    private void SetButtonAnimation(MyButton button, VisualElement buttonContainer)
    {
        button.OnPressed = e => buttonContainer.AddToClassList("back-button-container-active");
        button.OnReleased = e => buttonContainer.RemoveFromClassList("back-button-container-active");
    }

    private void OpenDialog(VisualElement dialog)
    {
        _dialogContainer.style.display = DisplayStyle.Flex;
        dialog.style.display = DisplayStyle.Flex;
        _dialogContainer.AddToClassList("dialog-container-fadein");
    }
    
    private void CloseDialog(VisualElement dialog)
    {
        _dialogContainer.RemoveFromClassList("dialog-container-fadein");
        _dialogContainer.style.display = DisplayStyle.None;
        dialog.style.display = DisplayStyle.None;
    }

    private void OnToggleChanged(ChangeEvent<bool> evt)
    {
        bool t1Value = _t1.value;
        bool t2Value = _t2.value;
        bool t3Value = _t3.value;

        bool isButtonEnabled = t1Value || t2Value || t3Value;
        _sendFeedbackButton.SetEnabled(isButtonEnabled);
    }

    private void OnTextFieldChanged(ChangeEvent<string> evt)
    {
        string tfValue = _commentField.value;
        bool isToggleActive = _t1.value || _t2.value || _t3.value;

        bool isButtonEnabled = !string.IsNullOrEmpty(tfValue) || isToggleActive;
        _sendFeedbackButton.SetEnabled(isButtonEnabled);
    }

    private void ResetFeedbackForm()
    {
        _t1.value = false;
        _t2.value = false;
        _t3.value = false;
        _commentField.value = "";
        _commentField.SetPlaceholderText("Add a comment...");
        _sendFeedbackButton.SetEnabled(false);
    }

    public void LoadData(GameData data)
    {
        _gameData = data;
    }

    public void SaveData(GameData data)
    {
        
    }
}
