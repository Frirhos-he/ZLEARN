using System;
using System.Collections.Generic;
using System.Linq;
using UI_Toolkit.Scripts.Classes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace UI_Toolkit.Scripts
{
    class AnswerObj
    {
        public Answer Answer { get; set; }
        public VisualElement AnswerUi { get; set; }
        public AnswerObj(Answer answer, VisualElement answerUi)
        {
            Answer = answer;
            AnswerUi = answerUi;
        }
    }

    enum AnswerStatus
    {
        Initial,
        Selected,
        Last
    }
    
    public class QuestionsViewPresenter : MonoBehaviour, IDataPersistence
    {
        private List<Question> _questions = new();
        private Label _pageTitle, _questionText, _scoreLabel, _answerResultLabel;
        private VisualElement _answerContainer, _homeView, _submitContainer;
        private VisualTreeAsset _answerTemplate;
        private Button _submitButton;
        private int _currentQuestionIndex = 0;
        private AnswerObj _currentAnswerClicked;
        private int _score = 0;
        private AnswerObj _correctAnswer;
        private AnswerStatus _answerStatus;
        private bool _answerChecked;
        private QuizSummary _quizSummary;
        private QuizSummaryViewPresenter _quizSummaryPresenter;
        public Topic CurrentTopic { get; set; }
        public bool IsReachedBySkippingExplanation { get; set; }

        [FormerlySerializedAs("_mainViewPresenter")]
        public MainViewPresenter MainViewPresenter;

        private int _numberOfQuestions = 4; //todo: trovare modo per imostare dei settaggi

        public void SetHomeView(VisualElement homeView)
        {
            _homeView = homeView;
        }

        public void SetParent(MainViewPresenter mainViewPresenter)
        {
            MainViewPresenter = mainViewPresenter;
        }

        public void Render()
        {
            //carica il template dagli assets
            _answerTemplate = Resources.Load<VisualTreeAsset>("AnswerCardTemplate");

            //----page setup----
            _pageTitle = _homeView.Q<Label>("PageTitle");
            _questionText = _homeView.Q<Label>("QuestionText");
            _answerContainer = _homeView.Q<VisualElement>("AnswersContainer");
            _scoreLabel = _homeView.Q<Label>("ScoreLbl");
            _submitButton = _homeView.Q<Button>("SubmitButton");
            _submitContainer = _homeView.Q<VisualElement>("SubmitContainer");
            _answerResultLabel = _homeView.Q<Label>("answerResultLbl");

            //----questions setup----
            _questions = CurrentTopic.questions;
            _numberOfQuestions = _questions.Count > _numberOfQuestions ? _numberOfQuestions : _questions.Count;
            _questions = new List<Question>(Shuffle(_questions).Take(_numberOfQuestions));
            //-----

            //----show questions in view----
            _currentQuestionIndex = 0;
            _quizSummary = new QuizSummary();

            MainViewPresenter.UpdatePageContent("Questions");
            UpdateScoreLabel(_score);
            if (_questions.Count > 0)
            {
                ShowQuestion();
            }
            _submitButton.RegisterCallback<ClickEvent>(OnSubmit);
        }

        void ShowQuestion()
        {
            //pulizia
            _answerContainer.Clear();
            _answerStatus = AnswerStatus.Initial;
            _answerChecked = false;
            _submitButton.SetEnabled(false);

            var currentQuestion = _questions[_currentQuestionIndex];
            //crea una lista di 4 risposte in cui compare la risposta giusta (nel caso in cui le risposte siano più di 4)
            var answers = currentQuestion.answers.OrderBy(a => a.isCorrect ? 0 : 1).Take(4);
            answers = Shuffle(answers);
            UpdatePageTitle(_currentQuestionIndex + 1);
            _questionText.text = currentQuestion.question;

            //show answers (limit to 4 answer only)
            var i = 0;
            foreach (var answer in answers)
            {
                
                var answerUi = _answerTemplate.Instantiate().Q<VisualElement>("AnswerCardContainer");
                var answerText = answerUi.Q<Label>("AnswerText");
                answerText.text = $"{ConvertToLetter(i)}. {answer.text}";
                answerUi.Q("AnswerCard").RegisterCallback<ClickEvent>((ClickEvent evt) => OnAnswerClicked(evt, answer));
                if (answer.isCorrect)
                {
                    _correctAnswer = new AnswerObj(answer, answerUi.Q("AnswerCard"));
                }
                _answerContainer.Add(answerUi);
                i++;
            }
        }

        private void OnAnswerClicked(ClickEvent evt, Answer currentAnswer)
        {
            //se è già stato fatto il check della risposa allora non si possono più selezionare
            if (_answerChecked)
            {
                return;
            }

            //deselect old answer
            _currentAnswerClicked?.AnswerUi.RemoveFromClassList("selected");
            
            var selectedAnswer = evt.currentTarget as VisualElement;
            _currentAnswerClicked = new AnswerObj(currentAnswer, selectedAnswer);

            //high light the selected answer and active the submit button
            selectedAnswer.AddToClassList("selected");
            _submitButton.RemoveFromClassList("button-secondary");
            _answerStatus = AnswerStatus.Selected;
            _submitButton.SetEnabled(true);
        }

        private void OnSubmit(ClickEvent evt)
        {
            //pulizia
            if (_questions.Count == 0)
            {
                ShowQuizSummary();
                return;
            }
            _submitContainer.style.backgroundColor = new StyleColor(new Color32(0, 0, 0, 0));
            _submitButton.style.backgroundColor = StyleKeyword.Null;
            _submitButton.style.color = StyleKeyword.Null;
            _answerResultLabel.text = "";
            _correctAnswer.AnswerUi.RemoveFromClassList("correct-answer");
            if (_currentAnswerClicked != null)
            {
                _currentAnswerClicked.AnswerUi.RemoveFromClassList("selected");
                _currentAnswerClicked.AnswerUi.RemoveFromClassList("wrong-answer");
            }

            if (_answerChecked && _answerStatus != AnswerStatus.Last)
            {
                //se si è cliccato submit si può andare avanti
                NextQuestion();
                _answerStatus = AnswerStatus.Initial;//rimettere su initial
                _submitButton.text = "CHECK";
            }
            else
            {

                switch (_answerStatus)
                {
                    case AnswerStatus.Initial:
                        //non si è selezionato nulla allora non si deve fare nulla
                        _submitButton.text = "CHECK";
                        break;
                    case AnswerStatus.Selected:
                        //se si è selezionata una risposta si può fare submit
                        ShowCheckQuestion();

                        //salva la domanda e la risposta corrente prima di andare avanti (usato poi per il riassunto dei quiz)
                        _quizSummary.Questions.Add(new QuestionSummary(
                            _questions[_currentQuestionIndex].question,
                            _currentAnswerClicked.Answer,
                            _correctAnswer.Answer
                            ));

                        //ho mostrato se è sbagliata o meno mostro il pulsante per andare avanti altrimenti
                        //se è l'ultima domanda mostro il pulsante per finire
                        _answerStatus = _currentQuestionIndex >= _questions.Count() - 1 ? AnswerStatus.Last : AnswerStatus.Initial;
                        _answerChecked = true;
                        _submitButton.text = _currentQuestionIndex >= _questions.Count() - 1 ? "FINISH" : "NEXT";
                        break;
                    case AnswerStatus.Last:
                        ShowQuizSummary();
                        break;
                }
            }
        }

        private void ShowCheckQuestion()
        {
            if (_currentAnswerClicked.Answer.isCorrect)
            {
                _score++;
            }
            UpdateScoreLabel(_score);
            _correctAnswer.AnswerUi.AddToClassList("correct-answer");
            if (_currentAnswerClicked.Answer.isCorrect)
            {
                _submitContainer.style.backgroundColor = new StyleColor(new Color32(35, 132, 52, 40));
                _submitButton.style.backgroundColor = new StyleColor(new Color32(35, 120, 52, 255));
                _submitButton.style.color = new StyleColor(new Color32(217, 217, 217, 255));
                _answerResultLabel.text = "Correct!";
            }
            else
            {
                _submitContainer.style.backgroundColor = new StyleColor(new Color32(255, 52, 52, 40));
                _submitButton.style.backgroundColor = new StyleColor(new Color32(255, 52, 52, 200));
                _submitButton.style.color = new StyleColor(new Color32(217, 217, 217, 255));
                _answerResultLabel.text = "Wrong!";
                _currentAnswerClicked.AnswerUi.AddToClassList("wrong-answer");
            }
        }

        private void NextQuestion()
        {
            _submitButton.AddToClassList("button-secondary");

            _currentAnswerClicked = null;
            _currentQuestionIndex++;
            ShowQuestion();
        }

        private void ShowQuizSummary()
        {
            var dataManager = DataPersistenceManager.GetInstance();

            // check for achievements here

            dataManager.SaveGame();
            dataManager.IsInQuizSummary = true;
            IsReachedBySkippingExplanation = false;
            MainViewPresenter.UpdatePageContent("Quiz Summary");
            _quizSummaryPresenter.SetHomeView(_homeView);
            _quizSummaryPresenter.SetParent(MainViewPresenter);
            _quizSummaryPresenter.Render(_quizSummary, _score);
        }

        void UpdatePageTitle(int currentQuestionNumber)
        {
            if (_pageTitle == null)
                return;
            _pageTitle.text = $"Questions: {currentQuestionNumber}/{_numberOfQuestions}";
        }

        public void RefreshPageTitle()
        {
            UpdatePageTitle(_currentQuestionIndex + 1);
        }

        void UpdateScoreLabel(int score)
        {
            if (_scoreLabel == null)
                return;
            _scoreLabel.text = $"Score: {score}";
        }

        IOrderedEnumerable<T> Shuffle<T>(List<T> list)
        {
            return list.OrderBy(item => Random.value);
        }

        IOrderedEnumerable<T> Shuffle<T>(IEnumerable<T> list)
        {
            return list.OrderBy(item => Random.value);
        }

        static char ConvertToLetter(int number)
        {
            // Mapping numbers 0 to 3 to letters A to D
            return number switch {
                0 => 'A',
                1 => 'B',
                2 => 'C',
                3 => 'D',
                _ => throw new ArgumentOutOfRangeException("Number should be in the range 0 to 3."),
            };
        }

        void Awake()
        {
            _quizSummaryPresenter = gameObject.AddComponent<QuizSummaryViewPresenter>();
        }

        public void LoadData(GameData data)
        {
        }

        public void SaveData(GameData data)
        {
            if (CurrentTopic != null)
            {
                var dataManager = DataPersistenceManager.GetInstance();
                var savedChapter = data.savedChapters.Find(c => c.number == dataManager.CurrentChapter?.number);
                if (savedChapter == null)
                {
                    // se è la prima volta che salviamo il capitolo, lo creiamo
                    savedChapter = SavedChapter.FromChapter(dataManager.CurrentChapter);
                    data.savedChapters.Add(savedChapter);
                }
                var topic = savedChapter.topics.Find(x => x.number == CurrentTopic.number);
                // only update the correct questions number if it is lower than the score
                // otherwise we run into the problem where if someone redoes a quiz and gets a lower score
                // the topic suddenly relocks.
                if (topic.correctQuestions < _score)
                    topic.correctQuestions = _score;
                if (Utils.Utils.IsQuizCompleted(topic.correctQuestions, topic.totalQuestions))
                {
                    // quiz was completed
                    // which means topic was completed
                    if (!data.topicsDoneToday.Any(x => x.name == CurrentTopic.name))
                    {
                        data.topicsDoneToday.Add(new TopicDoneToday { name = CurrentTopic.name, skippedExplanation = IsReachedBySkippingExplanation });
                        data.quizDoneTodayWithoutErrors.Value += (topic.correctQuestions == topic.totalQuestions) ? 1 : 0;
                    }
                    if (CurrentTopic.number == dataManager.CurrentChapter.topics.Count - 1) // last topic done, chapter was completed
                    {
                        if (!data.chaptersDoneToday.Contains(dataManager.CurrentChapter.name))
                        {
                            data.chaptersDoneToday.Add(dataManager.CurrentChapter.name);
                            bool withoutErrors = savedChapter.topics.Select(x => x.totalQuestions == x.correctQuestions).All(x => x == true);
                            if (withoutErrors)
                            {
                                data.chaptersDoneTodayWithoutErrors.Value += 1;
                            }
                        }
                    }
                }
            }
        }
    }
}
