using System.Linq;
using UI_Toolkit.Scripts.Classes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UI_Toolkit.Scripts
{
    public class QuizSummaryViewPresenter : MonoBehaviour
    {
        private ScrollView _quizSummaryList;

        private VisualElement _homeView;
        
        [FormerlySerializedAs("_mainViewPresenter")]
        public MainViewPresenter MainViewPresenter;

        public void SetParent(MainViewPresenter mainViewPresenter)
        {
            MainViewPresenter = mainViewPresenter;
        }
        
        public void SetHomeView(VisualElement homeView)
        {
            _homeView = homeView;
        }

        public void Render(QuizSummary summary, int score)
        {
            _quizSummaryList = _homeView.Q<ScrollView>("QuizSummaryScrollView");
            var summaryItemTemplate = Resources.Load<VisualTreeAsset>("QuestionSummaryTemplate");
            var resultLabel = _homeView.Q<Label>("ResultLbl");
            var nextTopicButton = _homeView.Q<Button>("NextTopicButton");
            var retryButton = _homeView.Q<Button>("RetryButton");
            var scoreLabel = _homeView.Q<Label>("QuizSummaryScoreLbl");
            
            retryButton.RegisterCallback<ClickEvent>(Retry);
            nextTopicButton.RegisterCallback<ClickEvent>(NextTopic);
            
            //pulizia
            retryButton.RemoveFromClassList("button-secondary");
            
            scoreLabel.text = $"Score: {score} out of {summary.Questions.Count}";
            var isQuizCompleted = Utils.Utils.IsQuizCompleted(score, summary.Questions.Count) || summary.Questions.Count == 0;
            //se è completato allora può cliccare su avanti
            nextTopicButton.SetEnabled(isQuizCompleted);
            
            if (isQuizCompleted)
            {
                resultLabel.text = "More than 60% of the answers are correct. You have completed this topic!";
                //rendi meno importatne il bottone di retry
                retryButton.AddToClassList("button-secondary");
            }
            else
            {
                resultLabel.text = "Less than 60% of the answers are correct. Try the quiz again.";
            }
            
            //----lista risposte----
            _quizSummaryList.mode = ScrollViewMode.Vertical;
            _quizSummaryList.Clear();
            var i = 0;
            foreach (var question in summary.Questions)
            {
                var questionUI = summaryItemTemplate.Instantiate().Q<VisualElement>("Container");
                questionUI.Q<Label>("QuestionText").text = question.Text;
                var questionResultLabel = questionUI.Q<Label>("QuestionResultLbl");
                var correctAnswerText = questionUI.Q<Label>("CorrectAnswerText");
                var givenAnswerText = questionUI.Q<Label>("GivenAnswerText");
                var wrongIcon = questionUI.Q<VisualElement>("WrongIcon");
                var correctIcon = questionUI.Q<VisualElement>("CorrectIcon");
                var correctAnswerContainer = questionUI.Q<VisualElement>("CorrectAnswerContainer");
                var questionNumber = questionUI.Q<Label>("QuestionNumber");
                
                var isCorrect = question.GivenAnswer.isCorrect;
                questionResultLabel.text = isCorrect ? "CORRECT" : "WRONG";
                questionResultLabel.AddToClassList(isCorrect ? "color-green" : "color-red");

                givenAnswerText.text = question.GivenAnswer.text;
                correctAnswerText.text = question.CorrectAnswer.text;
                questionNumber.text = (i + 1).ToString();

                //Se quella data è sbagliata viene mostrata quella corretta
                if (isCorrect)
                {
                    wrongIcon.style.display = StyleKeyword.None;
                    correctIcon.style.display = StyleKeyword.Initial;
                    correctAnswerContainer.style.display = StyleKeyword.None;
                }
                else
                {
                    wrongIcon.style.display = StyleKeyword.Initial;
                    correctIcon.style.display = StyleKeyword.None;
                    correctAnswerContainer.style.display = StyleKeyword.Initial;
                }
                _quizSummaryList.Add(questionUI);
                i++;
            }
            //--- --- --- ---
        }

        void NextTopic(ClickEvent evt)
        {
            DataPersistenceManager.GetInstance().SaveGame();
            DataPersistenceManager.GetInstance().CurrentTopic = null;
            DataPersistenceManager.GetInstance().IsInQuizSummary = false;
            //ritorna alla pagina di selezione del topic (equivalente a cliccare il pulsante home)
            MainViewPresenter.TriggerSingleChapterReRender();
            MainViewPresenter.UpdatePageContent("Home");
        }
        
        void Retry(ClickEvent evt)
        {
            //ritorna alla schermata di VR
            DataPersistenceManager.GetInstance().SaveGame();
            DataPersistenceManager.GetInstance().IsInQuizSummary = false;
            SceneManager.LoadScene("ARScene");
        }
    }
}
