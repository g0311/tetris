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
    }

    private void ReadyListener()
    {    
        // 준비 상태를 토글하고 패널을 업데이트합니다.
        bool currentReadyState = (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        PhotonNetwork.LocalPlayer.CustomProperties["IsReady"] = !currentReadyState;
        UpdatePanel();
    }
    private void ExitListener()
    {
        PhotonNetwork.LeaveRoom();
        gameObject.SetActive(false);
        LobbyUI.SetActive(true);
    }
    public override void OnLeftRoom()
    {
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
        UpdatePanel();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 플레이어가 방을 나가면 패널을 업데이트합니다.
        UpdatePanel();
    }

    private void StartGame()
    {
        PhotonNetwork.LoadLevel("OnGame");
    }
}
