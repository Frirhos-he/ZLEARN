using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class AchievementConditions
{
    [OptionalField]
    [DefaultValue(0)]
    public int completedTopics;

    [OptionalField]
    [DefaultValue(0)]
    public int completedChapters;

    [OptionalField]
    [DefaultValue(0)]
    public int quizWithoutErrors;

    [OptionalField]
    [DefaultValue(0)]
    public int chapterWithoutErrors;

    [OptionalField]
    [DefaultValue(false)]
    public bool reportFeedback;

    [OptionalField]
    [DefaultValue(null)]
    public string completeNamedTopic;

    [OptionalField]
    [DefaultValue(null)]
    public string completeNamedChapter;

    [OptionalField]
    [DefaultValue(false)]
    public bool skipExplanation;

    [OptionalField]
    [DefaultValue(false)]
    public bool interactWithArObject;

    [OptionalField]
    [DefaultValue(0)]
    public int reachLeaderboardPlace;

    public bool ConditionsMet(AchievementConditions other)
    {
        List<bool> conditionsMet = new();
        if (completedTopics != 0)
        {
            conditionsMet.Add(completedTopics <= other.completedTopics);
        }
        if (completedChapters != 0)
        {
            conditionsMet.Add(completedChapters <= other.completedChapters);
        }
        if (quizWithoutErrors != 0)
        {
            conditionsMet.Add(quizWithoutErrors <= other.quizWithoutErrors);
        }
        if (reportFeedback)
        {
            conditionsMet.Add(reportFeedback == other.reportFeedback);
        }
        if (!string.IsNullOrEmpty(completeNamedTopic))
        {
            conditionsMet.Add(completeNamedTopic ==  other.completeNamedTopic);
        }
        if (!string.IsNullOrEmpty(completeNamedChapter))
        {
            conditionsMet.Add(completeNamedChapter == other.completeNamedChapter);
        }
        if (skipExplanation)
        {
            conditionsMet.Add(skipExplanation ==  other.skipExplanation);
        } 
        if (interactWithArObject)
        {
            conditionsMet.Add(interactWithArObject == other.interactWithArObject);
        }
        if (reachLeaderboardPlace != 0)
        {
            conditionsMet.Add(reachLeaderboardPlace >= other.reachLeaderboardPlace);
        }
        if (conditionsMet.Count == 0)
            return false;
        return conditionsMet.All(x => x == true);
    }

}


[System.Serializable]
public class Achievement
{
    public string name;
    public string description;
    public int points;
    public AchievementConditions conditions;

    public bool IsClaimable(AchievementConditions otherConditions)
    {
        return conditions.ConditionsMet(otherConditions);
    }
}


[System.Serializable]
public class Achievements : IEnumerable<Achievement>
{
    public List<Achievement> achievements;

    public object Current => throw new System.NotImplementedException();

    public Achievement GetAchievement(int index)
    {
        return achievements[index];
    }
    public Achievement GetAchievementByName(string name)
    {
        return achievements.Find(x => x.name == name);
    }

    public IEnumerator<Achievement> GetEnumerator()
    {
        return achievements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}