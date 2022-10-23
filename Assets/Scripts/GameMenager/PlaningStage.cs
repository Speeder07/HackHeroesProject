using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

[System.Serializable]
public class PlaningStage : GameMenager.GameStage
{
    [SerializeField] const int startMoney = 1000;
    [SerializeField] public int currentMoney = 0;
    public List<PlanStruct> selectedPlans= new List<PlanStruct>();

    [SerializeField] PlanningUI currentPlaningBuilding = null;
    [SerializeField] GameMenager parentMenager;
    public PlaningStage(GameMenager parentMenager)
    {
        this.parentMenager = parentMenager;
        this.currentMoney = startMoney;
        this.parentMenager.planningUIText.gameObject.SetActive(true);
        this.parentMenager.planningUIText.text= $"Budrzet obywatelski: {currentMoney}zl";
        GameMenager.Instance.planningPanel.SetDisplay(true);
        GameMenager.Instance.votingPanel.SetDisplay(false);
    }
    
    public override void OnTileClicked(HexFieldGo fieldGo)
    {
        
        if (currentPlaningBuilding!=null) MonoBehaviour.Destroy(currentPlaningBuilding.gameObject);
        if (IsTileContained(fieldGo)) return;
        this.currentPlaningBuilding = MonoBehaviour.Instantiate(this.parentMenager.planningUIPrefab, fieldGo.transform.position + new Vector3(0,4,0), Quaternion.identity);
        this.currentPlaningBuilding.Initialize(fieldGo, this);
    }

    
    bool IsTileContained(HexFieldGo fieldGo)
    {
        foreach (PlanStruct plans in this.selectedPlans)
        {
            if (plans.coordinate == fieldGo.field.Coordinates) return true;
        }
        return false;
    }

    public override void OnTilEnd()
    {
        this.parentMenager.planningUIText.gameObject.SetActive(false);
        parentMenager.PlaningTillEnd(this.selectedPlans.ToArray());
        if (currentPlaningBuilding!=null) MonoBehaviour.Destroy(currentPlaningBuilding.gameObject);
    }

    public void OnButtonClicked(HexFieldGo go, int buildingId)
    {
        if (this.currentMoney <= 0) return;
        
        this.currentMoney -= currentPlaningBuilding.price;
        this.parentMenager.planningUIText.text = $"Budrzet obywatelski: {currentMoney}zl";
        MonoBehaviour.Destroy(currentPlaningBuilding.gameObject);
        currentPlaningBuilding=null;
        int variant = parentMenager.hologramPrefabs[buildingId].randomId;
        HexFieldGo newGo = HexGrid.Instance.InstantiateHexField(go.field, parentMenager.hologramPrefabs[buildingId].GetPrefab(variant));
        MonoBehaviour.Destroy(go.gameObject);
        selectedPlans.Add(new PlanStruct(buildingId, go.field.Coordinates, variant));
        newGo.StartCoroutine(newGo.SelectTile(new Vector3(0,1,0)));
        if (this.currentMoney <= 0) parentMenager.StartCoroutine(OnPlanningEnd());

    }
    
    IEnumerator OnPlanningEnd()
    {
        yield return new WaitForSeconds(2f);
        CurtainMenager.Instance.ChangeScenes_ServerRpc(2);
    }

    [System.Serializable]
    public struct PlanStruct : INetworkSerializable
    {
        public int id;
        public int variant;
        public HexCoordinate coordinate;

        public PlanStruct(int id, HexCoordinate coordinate, int variant)
        {
            this.id = id;
            this.coordinate = coordinate;
            this.variant = variant;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref coordinate);
            serializer.SerializeValue(ref variant);
        }
    }
}