using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class VotingStage : GameMenager.GameStage
{
    GameMenager parentMenager;
    [SerializeField] VotingUIMenager currentVotingUI;
    [SerializeField] HexFieldGo currentfieldGo;
    [SerializeField] List<VotingData> votingDatas = new List<VotingData>();
    List<VotingUIMenager> votingUIs = new List<VotingUIMenager>();
    public VotingStage(GameMenager menager)
    {
        parentMenager = menager;
        GameMenager.Instance.planningPanel.SetDisplay(false);
        GameMenager.Instance.votingPanel.SetDisplay(true);
    }

    public override void OnTileClicked(HexFieldGo fieldGo)
    {
        if (currentVotingUI!=null) MonoBehaviour.Destroy(currentVotingUI.gameObject);
        if (!IsTileContained(fieldGo.field.Coordinates)) return;
        this.currentVotingUI = MonoBehaviour.Instantiate(this.parentMenager.votingUIMenagerPrefab, fieldGo.transform.position + new Vector3(0,3.5f,0), Quaternion.identity);
        this.currentfieldGo = fieldGo;
        this.currentVotingUI.OnVotingResult += OnVotingResult;
    }

    void OnVotingResult(short value)
    {
        this.votingDatas.Add(new VotingData(this.currentfieldGo.field.Coordinates, value));
        this.votingUIs.Add(currentVotingUI);
        currentVotingUI = null;
        if (votingDatas.Count==4) parentMenager.VotingResult(this.votingDatas);
    }

    public override void OnTilEnd()
    {
        foreach (VotingUIMenager item in this.votingUIs)
        {
            MonoBehaviour.Destroy(item.transform.gameObject);
        }
    }

    bool IsTileContained(HexCoordinate coordinate)
    {
        bool result = false;
        foreach (PlaningStage.PlanStruct item in parentMenager.tempActiveTiles)
        {
            if (item.coordinate == coordinate)
            {
                result = true;
                break;
            }
        }

        if (result==false) return false;

        foreach (VotingData data in this.votingDatas)
        {
            if (data.Coordinate == coordinate) return false;
        }
        return true;
    }

    [System.Serializable]
    public struct VotingData : INetworkSerializable
    {
        public HexCoordinate Coordinate;
        public int votingStage;

        public void Add(int value)
        {
            this.votingStage += value;
        }

        public VotingData(HexCoordinate coordinate, int votingStage)
        {
            Coordinate = coordinate;
            this.votingStage = votingStage;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Coordinate);
            serializer.SerializeValue(ref votingStage);
        }
    }

    
}
