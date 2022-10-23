using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;

public class GameMenager : NetworkBehaviour
{
    public static GameMenager Instance;
    void Awake() => Instance = this;
    HexGrid grid => HexGrid.Instance;

    [SerializeField] LobbyUI lobbyUI;
    public HexGrid gridPrefab;

    

    

    public NetworkVariable<GamePhase> gamePhase = new NetworkVariable<GamePhase>(GamePhase.Idle, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    [SerializeField] List<Player> playersList = new List<Player>();
    [SerializeField] NetworkVariable<int> currentPlayerId = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    [SerializeReference]public GameStage stage;
    public NetworkVariable<int> turn = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    const int Max_Turns = 24;
    public List<PlaningStage.PlanStruct> tempActiveTiles = new List<PlaningStage.PlanStruct>();
    
    [SerializeField]List<VotingStage.VotingData> recentVotingData = new List<VotingStage.VotingData>();

    [ClientRpc]
    public void GenerateTasks_ClientRpc()
    {
        List<int> tasks = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            int rand;
            while (true)
            {
                rand = UnityEngine.Random.Range(1,9); 
                if (!tasks.Contains(rand)) break;
            }
            tasks.Add(rand);
        }
        TaskMenager.Instance.SetTasks(tasks.ToArray());
    }

    [ClientRpc] void SetIdleStage_ClientRpc()
    { 
        if(this.stage != null) this.stage.OnTilEnd();
        this.stage = new IdleStage();
    }

    [ClientRpc] void SetPlanningStage_ClientRpc(ClientRpcParams clientRpcParams = default)
    {   
        this.stage.OnTilEnd();
        this.stage = new PlaningStage(this);
    }

    [ServerRpc(RequireOwnership = false)]public void SetVotingStage_ServerRpc()
    {
        VotingStageEnding_ClientRpc();
        SetVotingStage_ClientRpc(tempActiveTiles.ToArray());
    }

    [ClientRpc]public void SetVotingStage_ClientRpc(PlaningStage.PlanStruct[] activeTiles)
    {
        this.stage = new VotingStage(this);
        this.tempActiveTiles = activeTiles.ToList();
        HexGrid.Instance.InstantiateHexField_ClientRpc(activeTiles);
    }

    [ClientRpc]
    void VotingStageEnding_ClientRpc()
    {
        this.stage.OnTilEnd();
    }

    [ServerRpc(RequireOwnership = false)] void SynchPlaningData_ServerRpc(PlaningStage.PlanStruct[] activeTiles)
    {
        SetVotingStage_ClientRpc(activeTiles);
    }


    public void PlaningTillEnd(PlaningStage.PlanStruct[] activeTiles)
    {
        SynchPlaningData_ServerRpc(activeTiles);
    }

    [ServerRpc(RequireOwnership = false)]public void SetSpactatingStage_ServerRpc()
    {
        this.stage.OnTilEnd();
        List<VotingResultStruct> votingResults = new List<VotingResultStruct>();
        for (int i = 0; i < this.recentVotingData.Count; i++)
        {
            bool resultBoot = false;
            foreach (VotingResultStruct result in votingResults)
            {
                if (result.coordinate == recentVotingData[i].Coordinate)
                {
                    recentVotingData[i].Add(result.votingResults);
                    resultBoot = true;
                    break;
                }
            }
            if (resultBoot == false)
            {
                votingResults.Add(new VotingResultStruct(recentVotingData[i].Coordinate, recentVotingData[i].votingStage));
            }
        }
        this.recentVotingData = new List<VotingStage.VotingData>();
        this.tempActiveTiles = new List<PlaningStage.PlanStruct>();

        ResetData_ClientRpc();
        SetSpecttingStage_ClientRpc(votingResults.ToArray());
        StartCoroutine(ResetTurn());
    }

    [ClientRpc]
    void ResetData_ClientRpc()
    {
        this.recentVotingData = new List<VotingStage.VotingData>();
        this.tempActiveTiles = new List<PlaningStage.PlanStruct>();
    }


    public struct VotingResultStruct : INetworkSerializable
    {
        public HexCoordinate coordinate;
        public int votingResults;

        public VotingResultStruct(HexCoordinate coordinate, int votingResults)
        {
            this.coordinate = coordinate;
            this.votingResults = votingResults;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref coordinate);
            serializer.SerializeValue(ref votingResults);
        }
    }

    


    [ClientRpc]public void SetSpecttingStage_ClientRpc(VotingResultStruct[] array)
    {
        this.stage.OnTilEnd();
        this.stage = new SpectatingStage();

        for (int i = 0; i < array.Length; i++)
        {
            HexFieldGo go = HexGrid.Instance.Cell(array[i].coordinate).Instance;
            if (array[i].votingResults<=0)
            {
                
                HexGrid.Instance.InstantiateHexField(
                    go.field, this.emptyTile
                );
                Destroy(go.gameObject);
            }
            if (array[i].votingResults>0)
            {
                HexGrid.Instance.InstantiateHexField(
                    go.field, this.buildingPrefabs[go.id].GetPrefab(go.variant)
                );
                Destroy(go.gameObject);
            }
        }

    }

    IEnumerator ResetTurn()
    {
        yield return new WaitForSeconds(5f);
        ResetTurn_ServerRpc();
        yield return null;

    }

    [ServerRpc]
    void ResetTurn_ServerRpc()
    {
        CurtainMenager.Instance.ChangeScenes_ServerRpc(1);
    }




    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerJoined;
        //NetworkManager.Singleton.OnServerStarted += OnHostEnterServerRpc;
        this.stage = new IdleStage();
    }

    void PlayerJoined(ulong id)
    {
        Debug.Log($"Player {id} joined");
        playerCount.Value++;
        this.lobbyUI.OnValueChanged(playerCount.Value);
        playersList.Add(new Player(id));
        OnPlayerJoined_ClientRpc(playerCount.Value, this.playersList.ToArray());
    }

    [ClientRpc]
    void OnPlayerJoined_ClientRpc(int value, Player[] playersList)
    {
        this.lobbyUI.OnValueChanged(value);
        this.playersList = Enumerable.ToList(playersList);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnGameStart_ServerRpc()
    {
        HexGrid grid = Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        grid.GetComponent<NetworkObject>().Spawn();

        currentPlayerId.Value = 0;
        Player currentPlayer = this.playersList[this.currentPlayerId.Value];

        SetIdleStage_ClientRpc();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{currentPlayer.id}
            }
        };
        GenerateTasks_ClientRpc();

        SetPlanningStage_ClientRpc(clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetFirstStage_ServerRpc()
    {
        turn.Value++;
        if (turn.Value>=Max_Turns)
        {
            GameEnd_ClientRpc();
            return;
        }
        this.currentPlayerId.Value++;
        if (this.currentPlayerId.Value>=this.playerCount.Value)
        {
            this.currentPlayerId.Value = 0;
        }
        Player currentPlayer = this.playersList[this.currentPlayerId.Value];

        SetIdleStage_ClientRpc();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{currentPlayer.id}
            }
        };

        SetPlanningStage_ClientRpc(clientRpcParams);
    }

    [ClientRpc]
    void GameEnd_ClientRpc()
    {
        TaskMenager.Instance.CollectPoints();
    }

    [ClientRpc]
    public void OnStartGame_ClientRpc()
    {
        lobbyUI.gameObject.SetActive(false);
    }

    

    [ClientRpc]
    public void AddPlayer_ClientRpc(ulong id)
    {
        this.playersList.Add(new Player(id));
        Debug.Log($"Player added {id}");
    }

    public void VotingResult(List<VotingStage.VotingData> datas)
    {
        
        AddVoting_ServerRpc(datas.ToArray());
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddVoting_ServerRpc(VotingStage.VotingData[] datas)
    {
        //this.recentVotingData = this.recentVotingData.Concat(datas).ToList();
        //debug.text = this.recentVotingData.Count.ToString();
        this.recentVotingData = this.recentVotingData.Concat(datas).ToList();
        AddVoting_ClientRpc(this.recentVotingData.ToArray());
        debug.text = $"{this.recentVotingData.Count/4}/{this.playerCount.Value}";
        if (this.recentVotingData.Count/4==this.playerCount.Value)
        {
            OnVotingEnd_ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void OnVotingEnd_ServerRpc()
    {
        //debug.text = "d";
        CurtainMenager.Instance.ChangeScenes_ServerRpc(3);
    }


    [ClientRpc]
    public void AddVoting_ClientRpc(VotingStage.VotingData[] datas)
    {
        this.recentVotingData = datas.ToList();
        debug.text = this.recentVotingData.Count.ToString();
    }
    
    
    


    [Header("HexTilesPrefab")]
    // 0 => park, 1 => house, 2 => sky scraper, 3 => shop 
    public HexFieldPrefabContainer[] hologramPrefabs = new HexFieldPrefabContainer[4];
    public HexFieldPrefabContainer[] buildingPrefabs = new HexFieldPrefabContainer[4];

    public DisplayPanel planningPanel;
    public DisplayPanel votingPanel;
    


    public enum GamePhase
    {
        Idle,
        Planing,
        Voting,
        Resulting
    }

    [System.Serializable]
    public struct Player : INetworkSerializable
    {
        public ulong id;

        public Player(ulong id)
        {
            this.id = id;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
        }
    }

    [Serializable]
    public struct HexFieldPrefabContainer
    {
        [SerializeField] List<HexFieldGo> prefabs;
        int Length => prefabs.Count();
        public int randomId => (Length>1)?UnityEngine.Random.Range(0,Length):0;

        public HexFieldGo GetPrefab(int id) => this.prefabs[id];

        public HexFieldPrefabContainer(List<HexFieldGo> prefabs)
        {
            this.prefabs = prefabs;
        }
    }
    
    [System.Serializable]
    public abstract class GameStage{
        public abstract void OnTileClicked(HexFieldGo fieldGo);
        public abstract void OnTilEnd();
    }
    [System.Serializable]
    public class IdleStage : GameStage
    {
        public IdleStage()
        {
            IdleStagePanel.Instance.setActive(true);
            GameMenager.Instance.planningPanel.SetDisplay(false);
            GameMenager.Instance.votingPanel.SetDisplay(false);
        }

        public override void OnTileClicked(HexFieldGo fieldGo)
        {

        }

        public override void OnTilEnd()
        {
            IdleStagePanel.Instance.setActive(false);
        }
    }

    [System.Serializable]
    public class SpectatingStage : GameStage
    {
        public SpectatingStage()
        {
            GameMenager.Instance.planningPanel.SetDisplay(false);
            GameMenager.Instance.votingPanel.SetDisplay(false);
        }

        public override void OnTileClicked(HexFieldGo fieldGo)
        {

        }

        public override void OnTilEnd()
        {
        }
    }

    [Space]
    [SerializeField]public PlanningUI planningUIPrefab;
    [SerializeField]public TMPro.TMP_Text planningUIText;

    public VotingUIMenager votingUIMenagerPrefab;
    public TMPro.TMP_Text debug;
    [SerializeField] HexFieldGo emptyTile;
}
