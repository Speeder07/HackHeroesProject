using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanningUI : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    public HexFieldGo go;
    public int price;
    [SerializeField] TMP_Text priceText;
    [SerializeField] PlaningStage stage;
    public void Initialize(HexFieldGo fieldGo, PlaningStage stage)
    {
        canvas.worldCamera = Camera.main;
        this.stage = stage;
        this.go = fieldGo;
        price = 250;
        priceText.text = price.ToString()+"zl";
    }

    void Update()
    {
        Quaternion camrot =  CameraMenager.Instance.transform.rotation;
        this.transform.rotation = camrot;
    }

    public void OnButtonClick(int id)
    {
        stage.OnButtonClicked(go, id);
    }
}
