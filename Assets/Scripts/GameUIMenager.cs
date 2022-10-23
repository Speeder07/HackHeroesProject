using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIMenager : MonoBehaviour
{
    public static GameUIMenager Instance;
    void Awake()=>Instance = this;
    [SerializeField]public TMP_Text text1;
    [SerializeField]public TMP_Text text2;
    [SerializeField]public TMP_Text text3;
    [SerializeField]public TMP_Text text4;

    [SerializeField]public TMP_Text StageText;

}
