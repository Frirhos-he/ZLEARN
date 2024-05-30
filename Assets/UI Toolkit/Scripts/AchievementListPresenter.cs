using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Unity.VisualScripting;

public class AchievementListPresenter : MonoBehaviour, IDataPersistence
{
    private const int NUM_ACHIEVEMENTS = 3;
    private VisualTreeAsset _achievementTemplate;
    private List<Achievement> _allAchievements = new();
    private List<Achievement> _selectedAchievements = new();
    private VisualElement _achievementView;
    private SavedAchievements _savedAchievements;
    private LeaderboardViewPresenter _leaderboardView;
    private GameData _gameData;

    public void SetAchievementView(VisualElement achievementView)
    {
        _achievementView = achievementView;
    }

    public void SetLeaderboardViewPresenter(LeaderboardViewPresenter leaderboardView)
    {
        _leaderboardView = leaderboardView;
    }

    public bool IsAchievementCompleted(Achievement achievement)
    {
        return _savedAchievements.completedAchievements.Contains(achievement.name);
    }

    public bool IsAchievementClaimable(Achievement achievement)
    {
        return achievement.IsClaimable(new AchievementConditions
        {
            completedTopics = _gameData.topicsDoneToday.Count,
            completedChapters = _gameData.chaptersDoneToday.Count,
            quizWithoutErrors = _gameData.quizDoneTodayWithoutErrors.Value,
            completeNamedChapter = _gameData.chaptersDoneToday.FirstOrDefault(x => x.Equals(achievement.conditions.completeNamedChapter)),
            completeNamedTopic = _gameData.topicsDoneToday.FirstOrDefault(x => x.name.Equals(achievement.conditions.completeNamedTopic)).name,
            skipExplanation = _gameData.topicsDoneToday.Any(x => x.skippedExplanation),
            reportFeedback = _gameData.sentFeedbackToday.Value,
            interactWithArObject = _gameData.interactedWithArToday.Value,
            reachLeaderboardPlace = _gameData.leaderboardPlacement.Value
        });
    }

    public void SetAchievementCompleted(Achievement achievement)
    {
        _savedAchievements.completedAchievements.Add(achievement.name);
        _leaderboardView.RenderRows(achievement.points);
    }

    public void LoadData(GameData data)
    {
        // Debug.Log("LOAD DATA ACHIEVEMENTLISTPRESENTER CALLED");
        _gameData = data;
        _savedAchievements = data.savedAchievements;
        DateTime currentDate = DateTime.Now;
        DateTime savedDate = DateTime.Now;
        bool res = _savedAchievements.date != null && DateTime.TryParse(_savedAchievements.date, out savedDate) && _savedAchievements.selectedAchievements.Count == NUM_ACHIEVEMENTS;
        if (!res)
        {
            // no date, we just load N achievements from the non-completed ones
            _savedAchievements.selectedAchievements = SelectRandomAchievements(NUM_ACHIEVEMENTS);
            _savedAchievements.date = currentDate.ToShortDateString();
            data.ResetAchievementTracking();
        }
        else if (currentDate.Date == savedDate.Date)
        {
            // same day, reload the old ones
            _selectedAchievements = _savedAchievements.selectedAchievements.Select(x => _allAchievements.Find(y => y.name == x.name)).ToList();
        }
        else
        {
            // different day, load new ones, filtering completed ones
            _savedAchievements.selectedAchievements = SelectRandomAchievements(NUM_ACHIEVEMENTS);
            _savedAchievements.date = currentDate.ToShortDateString();
            data.ResetAchievementTracking();
        }
        data.topicsDoneToday.Changed += (obj, ev) =>
        {
            RenderAchievements();
        };
        data.chaptersDoneToday.Changed += (obj, ev) =>
        {
            RenderAchievements();
        };
        data.quizDoneTodayWithoutErrors.Changed += (obj, ev) =>
        {
            RenderAchievements();
        };
        data.sentFeedbackToday.Changed += (obj, ev) =>
        {
            RenderAchievements();
        };
        data.chaptersDoneTodayWithoutErrors.Changed += (obj, ev) =>
        {
            RenderAchievements();
        };
        data.leaderboardPlacement.Changed += (obj, ev) =>
        {
            RenderAchievements();
        };
    }

    public void RenderAchievements()
    {
        var achievementListView = _achievementView.Q<VisualElement>("AchievementsListView"); //container

        achievementListView.Clear();
        //load the template
        _achievementTemplate = Resources.Load<VisualTreeAsset>("AchievementCardTemplate");


        for (int i = 0; i < _selectedAchievements?.Count && i < 3; i++)
        {
            var achievementUi = _achievementTemplate.Instantiate(); // Instantiate the achievement template

            if (achievementUi != null)
            {
                // Find elements within the instantiated template
                var achievementContainer = achievementUi.Q<VisualElement>("Container");

                var achievement = achievementContainer.Q<VisualElement>("Achievement");
                var titleLabel = achievement.Q<Label>("Title");
                var descriptionLabel = achievement.Q<Label>("Description");

                var points = achievementContainer.Q<VisualElement>("Points");
                var valueLabel = points.Q<Label>("Value");

                var claimPointsButton = achievementContainer.Q<Button>("ClaimPointsButton");

                // Set text content if elements are found
                if (titleLabel != null && descriptionLabel != null && valueLabel != null && claimPointsButton != null)
                {
                    var currentAchievement = _selectedAchievements[i];
                    titleLabel.text = currentAchievement.name;
                    descriptionLabel.text = currentAchievement.description;
                    valueLabel.text = currentAchievement.points.ToString();
                    if (IsAchievementCompleted(currentAchievement))
                    {
                        claimPointsButton.SetEnabled(false);
                        claimPointsButton.style.backgroundColor = new StyleColor(new Color32(35, 120, 52, 255));
                        claimPointsButton.style.color = new StyleColor(new Color32(217, 217, 217, 255));
                        claimPointsButton.text = "CLAIMED";
                    }
                    else if (!IsAchievementClaimable(currentAchievement))
                    {
                        claimPointsButton.SetEnabled(false);
                        claimPointsButton.text = "REWARD";
                    }
                    claimPointsButton.clicked += () =>
                    {
                        claimPointsButton.SetEnabled(false);
                        claimPointsButton.style.backgroundColor = new StyleColor(new Color32(35, 120, 52, 255));
                        claimPointsButton.style.color = new StyleColor(new Color32(217, 217, 217, 255));
                        claimPointsButton.text = "CLAIMED";
                        SetAchievementCompleted(currentAchievement);
                    };

                    if (achievementListView != null)
                    {
                        achievementListView.Add(achievementUi);
                    }
                    else
                    {
                        Debug.LogError("achievementListView is null!");
                    }
                }
                else
                {
                    Debug.LogError("Title or Description or value label not found in achievementUi!");
                }
            }
            else
            {
                Debug.LogError("Failed to instantiate achievementUi!");
            }
        }
    }

    public void SaveData(GameData data)
    {
        // Implement saving of data here
    }

    void Awake()
    {
        var achievementsData = DataPersistenceManager.GetInstance().GetAchievements(); //list of stored achievements
        _allAchievements.Clear();
        _allAchievements.AddRange(achievementsData);
    }

    void Start()
    {
        RenderAchievements();   
    }

    private List<SavedAchievement> SelectRandomAchievements(int count)
    {
        System.Random rnd = new();
        _selectedAchievements = _allAchievements
            .Where(x => !_savedAchievements.completedAchievements.Contains(x.name))
            .OrderBy(x => rnd.Next())
            .Take(count)
            .ToList();
        return _selectedAchievements.Select(x => new SavedAchievement { name = x.name }).ToList();
    }

    void Update()
    {
        // Implement update logic if needed
    }
}
