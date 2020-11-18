using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnlockAllLevels : MonoBehaviour
{
    public void UnlockAll()
    {
        ES3.Save<int>("Level", 10); // 10 is the last level index

        if (SceneManager.GetActiveScene().buildIndex == 1)
            Menu.Instance.UpdateButtonsAfterUnlockAllLevels();
    }
}
