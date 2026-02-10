using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clicker.Core
{
    public class SaveReset : MonoBehaviour
    {
        // Key used by SaveSystem
        private const string SaveKey = "CLICKER_SAVE_V1";

        public void ResetSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();

            if (GameManager.I != null)
            {
                GameManager.I.softCurrency.Set(0);
                GameManager.I.totalEarned = 0;
                if (GameManager.I.upgradeManager != null) GameManager.I.upgradeManager.ResetAll();
                GameManager.I.SaveGame();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
