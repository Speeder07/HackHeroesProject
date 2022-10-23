using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanningUIButton : MonoBehaviour
{
    [SerializeField] PlanningUI parent;
    [SerializeField] Button button;
    public int buildingId;

    public void Initialize()
    {

    }


    public void onClick()
    {
        if (parent.go.id == buildingId)
        {
            this.button.enabled = false;
            return;
        } 
        parent.OnButtonClick(this.buildingId);
    }

    
}
