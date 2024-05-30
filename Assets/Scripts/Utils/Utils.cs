namespace Utils
{
    public class Utils
    {
        public static bool IsQuizCompleted(int score, int numberOfQuestios)
        {
            if (score == 0 && numberOfQuestios == 0) return true; // if the topic has no questions, just say that it's completed
            return (float)score / numberOfQuestios >= 0.6;
        }
    }
}
