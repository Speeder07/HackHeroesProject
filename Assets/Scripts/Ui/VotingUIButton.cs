using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class VotingUIButton : MonoBehaviour
{
    [SerializeField] Image _button;
    [SerializeField] bool value;
    public Image button{
        get{
            if (this._button == null) _button = GetComponent<Image>();
            return this._button;
        }
    }
    public event Action<bool> OnButtonClick;

    public void OnClick()
    {
        OnButtonClick(value);
    }
}
