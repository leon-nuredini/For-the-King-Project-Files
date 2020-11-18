using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGRedirectLinks : MonoBehaviour
{
    public void OpenAGMoreGamesLink()
    {
        Application.ExternalEval("window.open(\"https://armor.ag/MoreGames\")");
    }
    
    public void OpenAGFBLink() { Application.ExternalEval("window.open(\"https://www.facebook.com/ArmorGames\")"); }
}
