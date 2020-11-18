using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurrentLevel : MonoBehaviour
{
    [SerializeField] private string[] _titles;

    private TextMeshPro _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();

        _text.text = _titles[SceneManager.GetActiveScene().buildIndex - 2];
    }
}
