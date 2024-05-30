using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// NOT USED, JUST PROVIDED AS AN EXAMPLE TO SHOW HOW TO SAVE DATA
/// </summary>
public class ChapterViewPresenter : MonoBehaviour, IDataPersistence
{
    public void LoadData(GameData data)
    {
        // _count = data.count;
    }

    public void SaveData(GameData data)
    {
        // data.count = _count;
    }
}
