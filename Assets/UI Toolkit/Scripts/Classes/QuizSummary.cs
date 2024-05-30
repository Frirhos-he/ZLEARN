using System.Collections.Generic;

namespace UI_Toolkit.Scripts.Classes
{
    public class QuizSummary
    {
        public List<QuestionSummary> Questions = new List<QuestionSummary>();
    }

    public class QuestionSummary
    {
        public string Text;
        public Answer GivenAnswer;
        public Answer CorrectAnswer;
        public QuestionSummary(string text, Answer givenAnswer, Answer correctAnswer)
        {
            Text = text;
            GivenAnswer = givenAnswer;
            CorrectAnswer = correctAnswer;
        }
    }
}
