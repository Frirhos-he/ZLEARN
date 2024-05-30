using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class SavedTopic
{
    public int number;
    public int totalQuestions;
    public int correctQuestions;

    public static SavedTopic FromTopic(Topic topic)
    {
        return new SavedTopic()
        {
            number = topic.number,
            totalQuestions = topic.questions.Count,
            correctQuestions = 0
        };
    }
    public bool Completed { get => Utils.Utils.IsQuizCompleted(correctQuestions, totalQuestions); }
}

[Serializable]
public class SavedChapter
{
    public int number;
    public List<SavedTopic> topics = new();

    public static SavedChapter FromChapter(Chapter chapter)
    {
        SavedChapter savedChapter = new()
        {
            number = chapter.number,
            topics = chapter.topics.Select(t => SavedTopic.FromTopic(t)).ToList(),
        };
        return savedChapter;
    }

    public int CompletedTopicCount { get => topics.Where(x => x.Completed).Count(); }
    public bool Completed { get => CompletedTopicCount == topics.Count; }
}


[Serializable]
public class SavedAchievement
{
    public string name;
}

[Serializable]
public class SavedAchievements
{
    public string date;
    public List<string> completedAchievements = new(); 
    public List<SavedAchievement> selectedAchievements = new();
}

[Serializable]
public struct SavedSettings
{
    public bool soundEnabled;
    public float musicVolume;
    public bool vibrationEnabled;
    public bool arPlanesEnabled;
}

[Serializable]
public struct TopicDoneToday
{
    public string name;
    public bool skippedExplanation;
}

[Serializable]
public class GameData
{
    public List<SavedChapter> savedChapters;
    public SavedAchievements savedAchievements;
    public SavedSettings savedSettings;
    public int userPoints;
    public Observable<int> chaptersDoneTodayWithoutErrors;
    public Observable<int> quizDoneTodayWithoutErrors;
    public ObservableList<TopicDoneToday> topicsDoneToday;
    public ObservableList<string> chaptersDoneToday;
    public Observable<bool> sentFeedbackToday;
    public Observable<bool> interactedWithArToday;
    public Observable<int> leaderboardPlacement;


    public GameData()
    {
        savedChapters = new();
        savedAchievements = new();
        savedSettings = new()
        {
            soundEnabled = false,
            musicVolume = 50,
            vibrationEnabled = false,
            arPlanesEnabled = true,
        };
        userPoints = 0;
        topicsDoneToday = new();
        chaptersDoneToday = new();
        quizDoneTodayWithoutErrors = 0;
        chaptersDoneTodayWithoutErrors = 0;
        sentFeedbackToday = false;
        leaderboardPlacement = 0;
    }

    public void ResetData()
    {
        savedChapters.Clear();
        savedAchievements.date = "";
        savedAchievements.completedAchievements.Clear();
        savedAchievements.selectedAchievements.Clear();
        userPoints = 0;
        ResetAchievementTracking();
    }

    public void ResetAchievementTracking()
    {
        topicsDoneToday.Clear();
        chaptersDoneToday.Clear();
        quizDoneTodayWithoutErrors = 0;
        chaptersDoneTodayWithoutErrors = 0;
        sentFeedbackToday = false;
        interactedWithArToday = false;
        leaderboardPlacement = 0;
    }
}
