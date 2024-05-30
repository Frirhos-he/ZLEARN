using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualClasses;

namespace AR.Executer
{
    public class FinishExecuter : MonoBehaviour, IARButtonExecuter
    {

        public void DoAction(GameObject obj)
        {
            var dataManager = DataPersistenceManager.GetInstance();
            dataManager.SaveGame();
            dataManager.IsFinishButtonPressed = true;
            SceneManager.LoadScene("MainScene");
        }
    }
}
