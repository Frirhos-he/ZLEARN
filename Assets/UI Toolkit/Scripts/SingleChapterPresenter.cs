using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SingleChapterPresenter : MonoBehaviour, IDataPersistence
{
    public Chapter CurrentChapter { get; private set; }
    public QuestionsViewPresenter _questionsViewPresenter;
    public MainViewPresenter _mainViewPresenter;
    public List<SavedChapter> _savedChapters;
    public void LoadData(GameData data)
    {
        _savedChapters = data.savedChapters;
    }

    public void SaveData(GameData data)
    {
    }

    public void SetParent(MainViewPresenter mainViewPresenter)
    {
        _mainViewPresenter = mainViewPresenter;
    }

    public void Render(Chapter chapter)
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var singleChapterTopicView = root.Q<ScrollView>("SingleChapterTopicView");
        singleChapterTopicView.Clear();
        CurrentChapter = chapter;
        var savedChapter = _savedChapters.Find(x => x.number == CurrentChapter.number);
        // each chapter card is 10%h * 20%w
        // placed vertically at 5%, 50%, 25%, 80%, ... repeating
        // placed horizontally at 5%, 40%, 60%, 75%, ... repeating + canvas width
        const int cardsPerScreen = 4;
        var vertOffset = new StyleLength[cardsPerScreen] { Length.Percent(12), Length.Percent(45), Length.Percent(25), Length.Percent(55) };
        foreach (var (topic, index) in CurrentChapter.topics.Select((v, i) => (v, i)))
        {
            var topicCard = new VisualElement();
            var lockImage = new VisualElement();
            var topicCardButton = new MyButton();
            var offsetIndex = index % cardsPerScreen;
            topicCard.AddToClassList("topic-card");
            topicCardButton.AddToClassList("topic-card-button");
            bool cardIsEnabled = false;
            if (savedChapter == null)
            {
                // chapter has never been saved before, first time opening it, only the first card is enabled
                cardIsEnabled = index == 0;
            }
            else
            {
                // card is enabled either if index == 0 (first card is always enabled)
                // or if the card before it was completed.
                if (index > 0)
                {
                    var savedTopic = savedChapter.topics.Find(x => x.number == index - 1);
                    cardIsEnabled = savedTopic != null && savedTopic.Completed;
                }
                else
                {
                    cardIsEnabled = true;
                }
            }
            if (!cardIsEnabled)
            {
                topicCardButton.SetEnabled(false);
                lockImage.AddToClassList("topic-locked");
            }
            else
            {
                lockImage.AddToClassList("topic-unlocked");
                topicCardButton.clicked += () =>
                {
                    var dataManager = DataPersistenceManager.GetInstance();
                    dataManager.CurrentChapter = CurrentChapter;
                    dataManager.CurrentTopic = topic;
                    dataManager.SaveGame();
                    SceneManager.LoadScene("ARScene");

                };
                topicCardButton.OnPressed += (e) =>
                {
                    lockImage.style.translate = new Translate(10, 10);
                };
                topicCardButton.OnReleased += (e) =>
                {
                    lockImage.style.translate = new Translate();
                };
            }
            topicCardButton.text = topic.name;
            if (index == 0)
            {
                topicCard.AddToClassList("topic-card-first");
            }
            topicCard.style.top = vertOffset[offsetIndex];
            topicCard.Add(topicCardButton);
            topicCard.Add(lockImage);
            var path = new VisualElement();
            path.AddToClassList("topic-path-generic");
            if (index == CurrentChapter.topics.Count - 1)
            {
                // use special path for last one
                path.AddToClassList($"topic-path-last");
            }
            else
            {
                path.AddToClassList($"topic-path-{offsetIndex + 1}");
            }
            singleChapterTopicView.Add(topicCard);
            singleChapterTopicView.Add(path);
        }
        var endCard = new VisualElement();
        var endCardButton = new Button();
        endCard.AddToClassList("end-card");
        endCardButton.AddToClassList("end-card-button");
        endCardButton.SetEnabled(false);
        endCardButton.text = "Chapter finished";
        endCard.Add(endCardButton);
        singleChapterTopicView.Add(endCard);
    }

    void Awake()
    {
        _questionsViewPresenter = gameObject.AddComponent<QuestionsViewPresenter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _questionsViewPresenter.SetHomeView(root);
        _questionsViewPresenter.SetParent(_mainViewPresenter);

        //usato solo quando viene cliccato il pulsante di skip dall'AR view
        var dataManager = DataPersistenceManager.GetInstance();
        if (dataManager.IsSkipButtonpressed)
        {
            dataManager.IsSkipButtonpressed = false;
            _questionsViewPresenter.CurrentTopic = dataManager.CurrentTopic;
            _questionsViewPresenter.IsReachedBySkippingExplanation = true;
            //passa alla pagina del quiz
            _questionsViewPresenter.Render();
        } else if (dataManager.IsFinishButtonPressed)
        {
            dataManager.IsFinishButtonPressed = false;
            _questionsViewPresenter.CurrentTopic = dataManager.CurrentTopic;
            _questionsViewPresenter.IsReachedBySkippingExplanation = false;
            _questionsViewPresenter.Render();
        }
    }

    public void RefreshPageTitle()
    {
        _questionsViewPresenter.RefreshPageTitle();
    }
}
