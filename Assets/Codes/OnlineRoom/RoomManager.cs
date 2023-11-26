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

    private bool IsGameStarted = false;

    // Start is called before the first frame update
    private void Awake()
    {
        ReadyBtn.onClick.AddListener(ReadyListener);
        ExitBtn.onClick.AddListener(ExitListener);
    }
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        UpdatePanel();
    }
    private void Update()
    {
        Player[] players = PhotonNetwork.PlayerList; //현재 룸에 접속한 플레이어의 배열을 반환하는 메서드
        foreach (Player pl in players)
        {
            if ((bool)pl.CustomProperties["IsReadyChanged"]) //플레이어의 ready 변수가 바뀌었는지 판단
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "IsReadyChanged", false },
            };
                pl.SetCustomProperties(props); //플레이어의 ready changed 속성 교체
                UpdatePanel(); //패널 업데이트
            }
        }
    }
    private void ReadyListener()
    { //로컬 플레이어의 준비 속성 교체 및 패널 업데이트
        bool currentReadyState = (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "IsReadyChanged", true },
            { "IsReady", !currentReadyState }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        UpdatePanel();
    }
    private void ExitListener()
    {
        PhotonNetwork.LeaveRoom();//방 나가기
    }
    public override void OnLeftRoom()
    { //방 나가졌을 시, OnConnectedToMaster이 호출 됨
        base.OnLeftRoom();
    }
    public override void OnConnectedToMaster()
    { //로비 참가
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    { //로비 참가 성공 시 UI 초기화
        base.OnJoinedLobby();
        Debug.Log(PhotonNetwork.InLobby);
        LobbyUI.SetActive(true);
        PhotonNetwork.AutomaticallySyncScene = false;
        gameObject.SetActive(false);
    }
    private void UpdatePanel()
    { //플레이어의 속성 및 정보들을 통해 UI 업데이트
        Player[] players = PhotonNetwork.PlayerList;
        Debug.Log(players.Length);
        if (players.Length > 0)
        {
            P1NameText.text = players[0].NickName;
            P1ReadyText.text = (bool)players[0].CustomProperties["IsReady"] ? "Ready" : "Not Ready";
        }

        if (players.Length > 1)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            P2NameText.text = players[1].NickName;
            P2ReadyText.text = (bool)players[1].CustomProperties["IsReady"] ? "Ready" : "Not Ready";
        }
        else
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            P2NameText.text = "";
            P2ReadyText.text = "";
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
        if (allPlayersReady && players.Length == 2 && !IsGameStarted)
        {
            StartGame();
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    { //플레이어가 룸에 접속시 UI업데이트
        base.OnPlayerEnteredRoom(newPlayer);
        UpdatePanel();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    { //플레이어가 방에 나갈 시 UI업데이트
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePanel();
    }
    private void StartGame()
    {
        IsGameStarted = true;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.AutomaticallySyncScene = true; //씬 이동이 마스터 클라이언트(방장)에 동기화 됨
        Player[] players = PhotonNetwork.PlayerList;
        int Pindex = -1;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsLocal)
            {
                Pindex = i;
            }
        }
        //현재 클라이언트 플레이어의 위치에 따라 모드 초기화
        if (Pindex == 0)
        {
            PlayerData.Instance.SetP1Name(players[0].NickName);
            PlayerData.Instance.SetP2Name(players[1].NickName);
            PlayerData.Instance.Set1P_Mode(1);
            PlayerData.Instance.Set2P_Mode(4);
        }
        else
        {
            PlayerData.Instance.SetP1Name(players[0].NickName);
            PlayerData.Instance.SetP2Name(players[1].NickName);
            PlayerData.Instance.Set1P_Mode(4);
            PlayerData.Instance.Set2P_Mode(2);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("OnGame");
        }
    }
}