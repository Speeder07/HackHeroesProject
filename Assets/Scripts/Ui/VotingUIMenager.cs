using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VotingUIMenager : MonoBehaviour
{
    [SerializeField] VotingUIButton like_button;
    [SerializeField] VotingUIButton dislike_button;
    Canvas canvas => GetComponent<Canvas>();
    [SerializeField] short votingState = 0;

    [SerializeField] bool enable = true;

    void Awake()
    {
        canvas.worldCamera = Camera.main;
        like_button.OnButtonClick += OnButtonClick;
        dislike_button.OnButtonClick += OnButtonClick;
    }

    void Update()
    {
        Quaternion camrot =  CameraMenager.Instance.transform.rotation;
        this.transform.rotation = camrot;
    }

    void OnButtonClick(bool value)
    {
        if (!this.enable) return;
        this.votingState = (short)((value)?1:-1);
        OnVotingResult?.Invoke(this.votingState);
        if (value)
        {
            StartCoroutine(VotingResultAnim(like_button, dislike_button));
        }
        else
        {
            StartCoroutine(VotingResultAnim(dislike_button, like_button));
        }
        enable = false;
    }

    public event Action<short> OnVotingResult;

    IEnumerator VotingResultAnim(VotingUIButton showButton, VotingUIButton hideButton)
    {
        float process = 1;
        while (true)
        {
            Color hideColor = hideButton.button.color;
            process-=0.2f;
            hideColor.a = process;
            hideButton.button.color = hideColor;
            if (process<=0) break;
            yield return new WaitForEndOfFrame();
        }
        float showStep = showButton.GetComponent<RectTransform>().localPosition.x / 10;
        float boxStep = (this.GetComponent<RectTransform>().sizeDelta.x - 1.25f) / 10;
        while (true)
        {
            showButton.GetComponent<RectTransform>().localPosition-=new Vector3(showStep,0,0);
            this.GetComponent<RectTransform>().sizeDelta-=new Vector2(boxStep,0);
            if (Math.Abs(showButton.GetComponent<RectTransform>().localPosition.x)<=0.05) break;
            yield return new WaitForEndOfFrame();
        }

        
    }

    
}
