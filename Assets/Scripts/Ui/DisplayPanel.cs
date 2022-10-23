using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayPanel : MonoBehaviour
{
    [SerializeField] List<Behaviour> displayBehaviours;

    void Awake()
    {
        SetDisplay(false);
    }

    public void SetDisplay(bool isEnabled)
    {
        foreach (Behaviour item in this.displayBehaviours)
        {
            item.enabled = isEnabled;
        }
    }
}
