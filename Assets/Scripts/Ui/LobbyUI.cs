using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyUI : MonoBehaviour
{
    
    public TMPro.TMP_Text playerCountText;
    public GameObject startGame;
    const byte MAX_PLAYERS = 8;
    

    void Start()
    {
        //NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnServerStarted += OnHostEnterServerRpc;
    }

    /*void OnClientConnected(ulong value) {

        Debug.Log($"Player joined {value}");
        playerCount.Value++;
        GameMenager.Instance.AddPlayer_ClientRpc(value);
        //playerCountText.text = playerCount.Value.ToString();
        OnValueChangedClientRpc(playerCount.Value);
    }*/

    void OnHostEnterServerRpc()
    {
        startGame.SetActive(true);
        Button startGameButton = this.startGame.GetComponent<Button>();
        startGameButton.onClick.AddListener(()=>{
            OnStartGame();
        });
    }

    public void OnValueChanged(int value)
    {
        playerCountText.text = $"{value.ToString()}/{MAX_PLAYERS}";
    }

    void OnStartGame()
    {
        GameMenager.Instance.OnGameStart_ServerRpc();
        GameMenager.Instance.OnStartGame_ClientRpc();
    }

}
