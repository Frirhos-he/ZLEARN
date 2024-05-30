using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainViewPresenter : MonoBehaviour, IDataPersistence
{
    private VisualElement _homeTab, _profileTab, _achievementsTab, _leaderboardTab, _infoTab;
    private VisualElement _homeView, _profileView, _achievementsView, _leaderboardView, _infoView, _questionsView, _quizSummaryView;
    private VisualElement _chapterContentView;
    private VisualElement _backStartButtonContainer, _backTopicsButtonContainer;
    private VisualElement _dialogContainer, _toTitleConfirmationDialog, _toTopicsConfirmationDialog;
    private MyButton _backStartButton, _backTopicsButton;
    private Button _homeButton, _profileButton, _achievementsButton, _leaderboardButton, _infoButton;
    private Button _lastClicked, _goTitleYesButton, _goTitleNoButton, _goTopicsYesButton, _goTopicsNoButton;
    private Label _pageTitle;
    private AchievementListPresenter _achievementListPresenter;
    private ChapterListPresenter _chapterListPresenter;
    private LeaderboardViewPresenter _leaderboardViewPresenter;
    private List<SavedChapter> _savedChapters;
    public bool IsInChapter { get; set; }

    void Awake()
    {
        _achievementListPresenter = gameObject.AddComponent<AchievementListPresenter>();
        _chapterListPresenter = gameObject.AddComponent<ChapterListPresenter>();
        _leaderboardViewPresenter = gameObject.AddComponent<LeaderboardViewPresenter>();
    }
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        var dataManager = DataPersistenceManager.GetInstance();

        IsInChapter = false;
        // Page Title Label reference
        _pageTitle = root.Q<Label>("PageTitle");
        _dialogContainer = root.Q<VisualElement>("DialogContainer");
        _toTitleConfirmationDialog = root.Q<VisualElement>("ToTitleConfirmationDialog");
        _toTopicsConfirmationDialog = root.Q<VisualElement>("ToTopicsConfirmationDialog");
       
        _backStartButtonContainer = root.Q<VisualElement>("BackStartButtonContainer");
        _backStartButton = root.Q<MyButton>("BackStartButton");
        _backStartButton.OnPressed = e => _backStartButtonContainer.AddToClassList("back-button-container-active");
        _backStartButton.OnReleased = e => _backStartButtonContainer.RemoveFromClassList("back-button-container-active");
        _backStartButton.clicked += () =>
        {
            // backStartButton only exists when viewing a topic
            // and goes back to the chapter
            if (!IsInChapter)
            {
                throw new Exception("BackStartButton got clicked while not in a chapter, this shouldn't happen");
            }
            IsInChapter = false;
            _chapterContentView.style.display = DisplayStyle.None;
            _chapterListPresenter.SelectedChapter = null;
            _homeView.style.display = DisplayStyle.Flex;
            _pageTitle.text = "Home";
            _backStartButtonContainer.style.display = DisplayStyle.None;
        };

        // Show back to topics button only in questions view or quiz summary view
        _backTopicsButtonContainer = root.Q<VisualElement>("BackTopicsButtonContainer");
        if (IsInQuizView()) 
        {
            _backTopicsButtonContainer.style.display = DisplayStyle.Flex;
        }
        else {
            _backTopicsButtonContainer.style.display = DisplayStyle.None;
        }
        _backTopicsButton = root.Q<MyButton>("BackTopicsButton");
        _backTopicsButton.OnPressed = e => _backTopicsButtonContainer.AddToClassList("back-button-container-active");
        _backTopicsButton.OnReleased = e => _backTopicsButtonContainer.RemoveFromClassList("back-button-container-active");
        _backTopicsButton.clicked += () => OpenDialog(_toTopicsConfirmationDialog);

        _goTopicsNoButton = root.Q<Button>("GoTopicsNoButton");
        _goTopicsNoButton.clicked += () => CloseDialog(_toTopicsConfirmationDialog);
        _goTopicsYesButton = root.Q<Button>("GoTopicsYesButton");
        _goTopicsYesButton.clicked += () =>
        {
            dataManager.CurrentTopic = null;
            SceneManager.LoadScene("MainScene");
        };

        root.Q<Button>("GoToARButton").clicked += () =>
        {
            DataPersistenceManager.GetInstance().SaveGame();
            SceneManager.LoadScene("ARScene");
        };

        // Page Title Label reference
        _pageTitle = root.Q<Label>("PageTitle");

        // Content Views references
        _homeView = root.Q<VisualElement>("HomeContentView");
        _profileView = root.Q<VisualElement>("ProfileContentView");
        _achievementsView = root.Q<VisualElement>("AchievementsContentView");
        _leaderboardView = root.Q<VisualElement>("LeaderboardContentView");
        _infoView = root.Q<VisualElement>("InfoContentView");
        _chapterContentView = root.Q<VisualElement>("ChapterContentView");
        _questionsView = root.Q<VisualElement>("QuestionsContentView");
        _quizSummaryView = root.Q<VisualElement>("QuizSummaryView");
        // Navbar Tabs references
        _homeTab = root.Q<VisualElement>("HomeTab");
        _profileTab = root.Q<VisualElement>("ProfileTab");
        _achievementsTab = root.Q<VisualElement>("AchievementsTab");
        _leaderboardTab = root.Q<VisualElement>("LeaderboardTab");
        _infoTab = root.Q<VisualElement>("InfoTab");

        // Navbar Buttons references
        _homeButton = root.Q<Button>("HomeButton");
        _profileButton = root.Q<Button>("ProfileButton");
        _achievementsButton = root.Q<Button>("AchievementsButton");
        _leaderboardButton = root.Q<Button>("LeaderboardButton");
        _infoButton = root.Q<Button>("InfoButton");

        // logout pressed on profile -> go to main menu
        var logoutButton = _profileView.Q<Button>("LogoutButton");
        logoutButton.clicked += () => OpenDialog(_toTitleConfirmationDialog);
        
        _goTitleNoButton = root.Q<Button>("GoTitleNoButton");
        _goTitleNoButton.clicked += () => CloseDialog(_toTitleConfirmationDialog);
        _goTitleYesButton = root.Q<Button>("GoTitleYesButton");
        _goTitleYesButton.clicked += () =>
        {
            dataManager.SaveGame();
            dataManager.CurrentChapter = null;
            dataManager.CurrentTopic = null;
            SceneManager.LoadScene("TitleScene");
        };

        _homeButton.RegisterCallback<ClickEvent>(OnNavbarButtonClicked);
        _profileButton.RegisterCallback<ClickEvent>(OnNavbarButtonClicked);
        _achievementsButton.RegisterCallback<ClickEvent>(OnNavbarButtonClicked);
        _leaderboardButton.RegisterCallback<ClickEvent>(OnNavbarButtonClicked);
        _infoButton.RegisterCallback<ClickEvent>(OnNavbarButtonClicked);

        _lastClicked = _homeButton;

        _achievementListPresenter.SetAchievementView(_achievementsView);
        _leaderboardViewPresenter.SetLeaderboardView(_leaderboardView);
        _achievementListPresenter.SetLeaderboardViewPresenter(_leaderboardViewPresenter);
        _leaderboardViewPresenter.SetRankingLabel(_profileView.Q<Label>("RankingLabel"));

        _chapterListPresenter.SetHomeView(_homeView);
        _chapterListPresenter.SetParent(this);
    }

    private void OnNavbarButtonClicked(ClickEvent evt)
    {
        Button clickedButton = evt.target as Button;

        if (_lastClicked != clickedButton)
        {
            // Remove style from last clicked button
            UpdateTabStyle(_lastClicked, false);

            // Add style to current clicked button
            _lastClicked = clickedButton;
            UpdateTabStyle(_lastClicked, true);

            if (IsInQuizView() && clickedButton.name == "HomeButton") 
            {
                _backTopicsButtonContainer.style.display = DisplayStyle.Flex;
            }
            else {
                _backTopicsButtonContainer.style.display = DisplayStyle.None;
            }
            
            // Update page content
            string pageName = clickedButton.name.Replace("Button", "");
            UpdatePageContent(pageName);
        }
    }

    private void UpdateTabStyle(Button button, bool isSelected = false)
    {
        VisualElement tab = null;

        switch (button.name)
        {
            case "HomeButton":
                tab = _homeTab;
                break;
            case "ProfileButton":
                tab = _profileTab;
                break;
            case "AchievementsButton":
                tab = _achievementsTab;
                break;
            case "LeaderboardButton":
                tab = _leaderboardTab;
                break;
            case "InfoButton":
                tab = _infoTab;
                break;
        }

        if (tab != null)
        {
            if (isSelected)
            {
                tab.AddToClassList("navbar-tab-active");
            }
            else
            {
                tab.RemoveFromClassList("navbar-tab-active");
            }
        }
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

    private bool IsInQuizView()
    {
        var dataManager = DataPersistenceManager.GetInstance();
        if (dataManager.CurrentTopic != null) {
            return true;
        }
        return false;
    }

    public void UpdatePageContent(string pageName)
    {
        _pageTitle.text = pageName;

        if (IsInChapter)
        {
            if (pageName == "Home")
            {
                _pageTitle.text = _chapterListPresenter.SelectedChapter.name;
                var dataManager = DataPersistenceManager.GetInstance();
                if (dataManager.CurrentTopic != null)
                {
                    pageName = dataManager.IsInQuizSummary ? "Quiz Summary" : "Questions";
                    if (!dataManager.IsInQuizSummary)
                    {
                        _chapterListPresenter.RefreshPageTitle();
                    } else
                    {
                        _pageTitle.text = "Quiz Summary";
                    }
                } else
                {
                    _backTopicsButtonContainer.style.display = DisplayStyle.None;
                }
            }
            _chapterContentView.style.display = pageName == "Home" ? DisplayStyle.Flex : DisplayStyle.None;
            _backStartButtonContainer.style.display = pageName == "Home" ? DisplayStyle.Flex : DisplayStyle.None;

        }
        else
        {
            _backStartButtonContainer.style.display = DisplayStyle.None;
            _homeView.style.display = pageName == "Home" ? DisplayStyle.Flex : DisplayStyle.None;
        }
        _profileView.style.display = pageName == "Profile" ? DisplayStyle.Flex : DisplayStyle.None;
        _achievementsView.style.display = pageName == "Achievements" ? DisplayStyle.Flex : DisplayStyle.None;
        _leaderboardView.style.display = pageName == "Leaderboard" ? DisplayStyle.Flex : DisplayStyle.None;
        _infoView.style.display = pageName == "Info" ? DisplayStyle.Flex : DisplayStyle.None;
        _questionsView.style.display = pageName == "Questions" ? DisplayStyle.Flex : DisplayStyle.None;
        _quizSummaryView.style.display = pageName == "Quiz Summary" ? DisplayStyle.Flex : DisplayStyle.None;
        if (_profileView.style.display != DisplayStyle.None)
        {
            UpdateProfileProgress();
        }
    }

    void UpdateProfileProgress()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        var chapters = DataPersistenceManager.GetInstance().GetChapters();

        var chapterProgress = root.Q<RadialProgress>("ChapterProgress");
        var topicProgress = root.Q<RadialProgress>("TopicProgress");
        var quizProgress = root.Q<RadialProgress>("QuizProgress");
        var courseProgress = root.Q<RadialProgress>("CourseProgress");
        var dataManager = DataPersistenceManager.GetInstance().GetChapters();
        var totalProgress = (float)_savedChapters.Select(x => x.CompletedTopicCount).Sum() / chapters.Select(x => x.topics.Count).Sum();
        chapterProgress.progress = (float)_savedChapters.Where(x => x.Completed).Count() / chapters.Count * 100f;
        topicProgress.progress = (float)_savedChapters.Select(x => x.CompletedTopicCount).Sum() / chapters.Select(x => x.topics.Count).Sum() * 100f;
        var correctQuestionsTot = (float)_savedChapters.Select(x => x.topics.Select(x => x.correctQuestions).Sum()).Sum();
        var totalQuestions = chapters.Select(x => x.topics.Select(t => t.questions.Count).Sum()).Sum();
        quizProgress.progress = correctQuestionsTot / totalQuestions * 100f;
        courseProgress.progress = (chapterProgress.progress + quizProgress.progress + topicProgress.progress) / 3;
    }

    public void ShowBackButton()
    {
        _backStartButtonContainer.style.display = DisplayStyle.Flex;
    }

    public void TriggerSingleChapterReRender()
    {
        _chapterListPresenter.TriggerSingleChapterReRender();
    }

    public void LoadData(GameData data)
    {
        _savedChapters = data.savedChapters;
    }

    public void SaveData(GameData data)
    {
        // nothing
    }
}
