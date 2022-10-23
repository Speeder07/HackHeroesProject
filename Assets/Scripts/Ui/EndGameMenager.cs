using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameMenager : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text wynik;
    [SerializeField] List<Behaviour> behaviours;

    public void SetFinalResult(int result)
    {
        Debug.Log("Game end");
        foreach (Behaviour item in behaviours)
        {
            item.enabled = true;
        }
        wynik.text = $"Udało ci sie zdobyc {result} punktow! \n Zobacz jak poszlo innym graczom";
    }
}
