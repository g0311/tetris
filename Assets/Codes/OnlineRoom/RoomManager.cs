using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject LobbyUI;
    [SerializeField]
    private Button ReadyBtn;
    [SerializeField]
    private Button ExitBtn;

    [SerializeField]
    private Text P1NameText;
    [SerializeField]
    private Text P1ReadyText;
    [SerializeField]
    private Text P2NameText;
    [SerializeField]
    private Text P2ReadyText;

    // Start is called before the first frame update
    private void Awake()
    {
        ReadyBtn.onClick.AddListener(ReadyListener);
        ExitBtn.onClick.AddListener(ExitListener);
    }
    public override void OnEnable()
    {
        UpdatePanel();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Update()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if ((bool)players[i].CustomProperties["IsReadyChanged"])
            {
                players[i].CustomProperties["IsReadyChanged"] = false;
                UpdatePanel();
            }
        }
    }

    private void ReadyListener()
    {    
        bool currentReadyState = (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        PhotonNetwork.LocalPlayer.CustomProperties["IsReadyChanged"] = true;
        PhotonNetwork.LocalPlayer.CustomProperties["IsReady"] = !currentReadyState;
    }
    private void ExitListener()
    {
        PhotonNetwork.LeaveRoom();
        gameObject.SetActive(false);
        LobbyUI.SetActive(true);
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        base.OnLeftRoom();
    }

    private void UpdatePanel()
    {
        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length > 0)
        {
            P1NameText.text = players[0].NickName;
            P1ReadyText.text = (bool)players[0].CustomProperties["IsReady"] ? "Ready" : "Not Ready";
        }
        if (players.Length > 1)
        {
            P2NameText.text = players[1].NickName;
            P2ReadyText.text = (bool)players[1].CustomProperties["IsReady"] ? "Ready" : "Not Ready";
        }

        bool allPlayersReady = true;
        foreach (Player player in players)
        {
            if (!(bool)player.CustomProperties["IsReady"])
            {
                allPlayersReady = false;
                break;
            }
        }
        if (allPlayersReady && players.Length == 2)
        {
            StartGame();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 플레이어가 방에 들어오면 패널을 업데이트합니다.
        Debug.Log("나감");
        UpdatePanel();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 플레이어가 방을 나가면 패널을 업데이트합니다.
        UpdatePanel();
    }

    private void StartGame()
    {
        Player[] players = PhotonNetwork.PlayerList;
        int Pindex = -1;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsLocal)
            {
                Pindex = i;
            }
        }
        if (Pindex == 0)
        {
            PlayerData.Instance.SetP1Name(players[0].NickName);
            PlayerData.Instance.SetP2Name(players[1].NickName);
            PlayerData.Instance.Set1P_Mode(1);
            PlayerData.Instance.Set2P_Mode(4);
        }
        else
        {
            PlayerData.Instance.SetP1Name(players[1].NickName);
            PlayerData.Instance.SetP2Name(players[0].NickName);
            PlayerData.Instance.Set1P_Mode(4);
            PlayerData.Instance.Set2P_Mode(1);
        }
        PhotonNetwork.LoadLevel("OnGame");
    }
}
