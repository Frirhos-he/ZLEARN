using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Answer
{
    public string text;
    public bool isCorrect;
}
[System.Serializable]
public class Topic : IIdentifier<string>
{
    public string name;
    public int number;
    public List<Question> questions;
    public string Name => name;
    public bool Matches(string identifier)
    {
        return name.ToLower().Contains(identifier.ToLower());
    }
}

[System.Serializable]
public class Question
{
    public string question;
    public List<Answer> answers;
}

[System.Serializable]
public class Chapter : IIdentifier<string>
{
    public string name;
    public int number;
    public List<Topic> topics;

    public string Name => name;

    public bool Matches(string identifier)
    {
        var lowerIdentifier = identifier.ToLower();
        var topicNameMatches = topics.Where(x => x.name.ToLower().Contains(lowerIdentifier)).Any();
        return name.ToLower().Contains(lowerIdentifier) || topicNameMatches;
    }
    
    public Topic GetTopicByNumber(int number)
    {
        return topics.Find(x => x.number == number);
    }
    
}

[System.Serializable]
public class Chapters : IEnumerable<Chapter>
{
    public List<Chapter> chapters;

    public int Count => chapters.Count;
    public Chapter GetChapter(int index)
    {
        return chapters[index];
    }
    public Chapter GetChapterByName(string name)
    {
        return chapters.Find(x => x.name == name);
    }
    public Chapter GetChapterByNumber(int number)
    {
        return chapters.Find(x => x.number == number);
    }

    public IEnumerator<Chapter> GetEnumerator()
    {
        return chapters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}