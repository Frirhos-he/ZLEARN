using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ChapterListPresenter : MonoBehaviour, IDataPersistence
{
    private List<SavedChapter> _savedChapters;
    private FilteringList<Chapter, string> _chaptersList;
    private VisualElement _homeView;
    private VisualElement _chapterContentView;
    private MainViewPresenter _mainViewPresenter;
    private SingleChapterPresenter _singleChapterPresenter;
    private Label _completionLabel;
    private Label _pageTitle;
    private Label _totPercentCompletedLabel;
    public Chapter SelectedChapter { get; set; }

    public void TriggerSingleChapterReRender()
    {
        _singleChapterPresenter.Render(SelectedChapter);
    }

    public void SetHomeView(VisualElement homeView)
    {
        _homeView = homeView;
    }
    public void SetParent(MainViewPresenter mainViewPresenter)
    {
        _mainViewPresenter = mainViewPresenter;
    }

    public void LoadData(GameData data)
    {
        _savedChapters = data.savedChapters;
    }

    public void SaveData(GameData data)
    {
    }

    void Awake()
    {
        _singleChapterPresenter = gameObject.AddComponent<SingleChapterPresenter>();
    }

    void Update()
    {
        var chapters = DataPersistenceManager.GetInstance().GetChapters();
        var totalProgress = (float)_savedChapters.Select(x => x.CompletedTopicCount).Sum() / chapters.Select(x => x.topics.Count).Sum();
        _totPercentCompletedLabel.text = $"{Mathf.RoundToInt(totalProgress * 100)}% Completed Overall";
    }

    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        var dataManager = DataPersistenceManager.GetInstance();
        var chapters = dataManager.GetChapters();
        var totalProgress = (float)_savedChapters.Select(x => x.CompletedTopicCount).Sum() / chapters.Select(x => x.topics.Count).Sum();
        _totPercentCompletedLabel = _homeView.Q<Label>("ChapterPercentageCompletedLabel");
        _totPercentCompletedLabel.text = $"{Mathf.RoundToInt(totalProgress * 100)}% Completed Overall";
        _chaptersList = new(chapters.chapters, null);
        var searchField = _homeView.Q<TextField>("ChapterSearchField");
        var chaptersListView = _homeView.Q<ListView>("ChaptersListView");
        _chapterContentView = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("ChapterContentView");
        _completionLabel = _chapterContentView.Q<Label>("SingleChapterCompletionLabel");
        _pageTitle = root.Q<Label>("PageTitle"); ;
        chaptersListView.showBoundCollectionSize = false;
        searchField.RegisterValueChangedCallback((e) =>
        {
            var searchTerm = e.newValue;
            if (searchTerm == null || searchTerm.Length == 0)
            {
                _chaptersList.Filter = null;
            }
            else
            {
                _chaptersList.Filter = searchTerm;
            }
            chaptersListView.Rebuild();
        });

        chaptersListView.itemsSource = _chaptersList;
        chaptersListView.bindItem = (e, i) =>
        {
            var ch = _chaptersList[i];

            var nameLabel = e.Q<Label>("ChapterNameLabel");
            nameLabel.text = ch.name;
            var numLabel = e.Q<Label>("ChapterNumberLabel");
            numLabel.text = $"Chapter {ch.number}: ";

            var completionBarLabel = e.Q<Label>("CompletionBarLabel");
            var completionProgressBar = e.Q<ProgressBar>("CompletionProgressBar");
            var savedChapter = _savedChapters.Find(x => x.number == ch.number);
            if (savedChapter == null)
            {
                completionBarLabel.text = "0% Completed";
                completionProgressBar.value = 0;
            }
            else
            {
                var completionPercent = Mathf.RoundToInt(savedChapter.CompletedTopicCount / (float)ch.topics.Count * 100f);
                completionBarLabel.text = $"{completionPercent}% Completed";
                completionProgressBar.value = completionPercent;
            }
        };
        chaptersListView.makeItem = () =>
        {
            var singleChapterViewContainer = new VisualElement();
            singleChapterViewContainer.AddToClassList("single-chapter-view-container");
            var singleChapterView = new Button();
            singleChapterView.AddToClassList("button-primary");
            singleChapterView.AddToClassList("single-chapter-view-item");

            var rightArrowIcon = new VisualElement();
            rightArrowIcon.AddToClassList("right-arrow-icon");
            singleChapterView.Add(rightArrowIcon);

            var chapterDesc = new VisualElement();
            chapterDesc.AddToClassList("chapter-description");

            var chapterNameLabel = new Label
            {
                name = "ChapterNameLabel"
            };
            var chapterNumberLabel = new Label
            {
                name = "ChapterNumberLabel"
            };
            chapterNameLabel.AddToClassList("chapter-name-label");
            chapterNumberLabel.AddToClassList("chapter-number-label");
            chapterDesc.Add(chapterNumberLabel);
            chapterDesc.Add(chapterNameLabel);
            singleChapterView.clicked += () =>
            {
                ShowChapter(chapters.GetChapterByName(chapterNameLabel.text));
            };

            var completionBar = new VisualElement();
            completionBar.AddToClassList("chapter-completion-bar");

            var completionBarLabel = new Label
            {
                name = "CompletionBarLabel"
            };
            completionBarLabel.AddToClassList("chapter-completion-bar-label");

            var completionProgressBar = new ProgressBar
            {
                name = "CompletionProgressBar"
            };
            completionProgressBar.AddToClassList("chapter-progress-bar");

            completionBar.Add(completionBarLabel);
            completionBar.Add(completionProgressBar);

            singleChapterView.Add(chapterDesc);
            singleChapterView.Add(completionBar);
            singleChapterViewContainer.Add(singleChapterView);
            return singleChapterViewContainer;
        };
        chaptersListView.Rebuild();
        _singleChapterPresenter.SetParent(_mainViewPresenter);
        if (dataManager.CurrentChapter != null)
        {
            ShowChapter(dataManager.CurrentChapter);
        }
    }
    private void ShowChapter(Chapter chapter)
    {
        _homeView.style.display = DisplayStyle.None;
        _chapterContentView.style.display = DisplayStyle.Flex;
        SelectedChapter = chapter;
        _singleChapterPresenter.Render(SelectedChapter);
        _pageTitle.text = SelectedChapter.name;
        _mainViewPresenter.IsInChapter = true;
        _mainViewPresenter.ShowBackButton();
        var savedChapter = _savedChapters.Find(x => x.number == chapter.number);
        if (savedChapter == null)
        {
            _completionLabel.text = $"0% Completed";
        }
        else
        {
            var completion = Mathf.RoundToInt(savedChapter.CompletedTopicCount / (float)chapter.topics.Count * 100f);
            _completionLabel.text = $"{completion}% Completed";
        }
    }

    public void RefreshPageTitle()
    {
        _singleChapterPresenter.RefreshPageTitle();
    }
}
