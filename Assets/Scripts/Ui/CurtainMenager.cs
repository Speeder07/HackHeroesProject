using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.UI;

public class CurtainMenager : NetworkBehaviour
{
    [SerializeField] float start = 0;
    [SerializeField] float end = 10;
    [SerializeField] int steps = 100;
    public static CurtainMenager Instance;
    [SerializeField] List<Image> images;
    [SerializeField] NetworkVariable<int> curtainStage = new NetworkVariable<int>();
    NetworkVariable<int> sceneid = new NetworkVariable<int>(0);
    void Awake() => Instance = this;


    [ServerRpc(RequireOwnership=false)]
    public void ChangeScenes_ServerRpc(int scene)
    {
        sceneid.Value = scene;
        StartCoroutine(ChangeCurtainCoroutine(0,0,1,0.01f,SendRisingEpc));
    }


    void SendRisingEpc(float progress, float start, float end, float step)
    {
        ChangeCurtainState_ClientRpc(progress);
        if (progress>=end)
        {
            StartCoroutine(WaitCoroutine(progress, start, end, -step, SendLowerRpc));
            return;
        }
        StartCoroutine(ChangeCurtainCoroutine(progress, start, end, step, SendRisingEpc));
    }   

    void SendLowerRpc(float progress, float start, float end, float step)
    {
        ChangeCurtainState_ClientRpc(progress);
        if (progress<=start)
        {
            foreach (Image image in images)
            {
                image.enabled = false;
            }
            return;
        } 
        StartCoroutine(ChangeCurtainCoroutine(progress, start, end, step, SendLowerRpc));
    }

    IEnumerator WaitCoroutine(float progress, float start, float end, float step, Action<float,float,float,float> callback)
    {
        yield return new WaitForSeconds(2f);
        callback( progress, start, end, step);
        switch (this.sceneid.Value)
        {
            case 1:
                GameMenager.Instance.SetFirstStage_ServerRpc();
                break;
            case 2:
                GameMenager.Instance.SetVotingStage_ServerRpc();
                break;
            case 3:
                GameMenager.Instance.SetSpactatingStage_ServerRpc();
                break;
            
        }
        yield return null;
    }

    IEnumerator ChangeCurtainCoroutine(float progress, float start, float end, float step, Action<float,float,float,float> callback)
    {
        progress+=step;
        yield return new WaitForEndOfFrame();
        callback(progress, start, end, step);
        yield return null;
    }

    [ServerRpc]
    private void ChangeCurtainState_ServerRpc(float progress)
    {
        foreach (Image image in images)
        {
            image.enabled = true;
            Color color = image.color;
            color.a = progress/(end-start);
            image.color = color;
        }
    }


    [ClientRpc]
    private void ChangeCurtainState_ClientRpc(float progress)
    {
        foreach (Image image in images)
        {
            image.enabled = true;
            Color color = image.color;
            color.a = progress;
            image.color = color;
        }
    }
}
