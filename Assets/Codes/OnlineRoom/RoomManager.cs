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
        Player[] players = PhotonNetwork.PlayerList; //���� �뿡 ������ �÷��̾��� �迭�� ��ȯ�ϴ� �޼���
        foreach (Player pl in players)
        {
            if ((bool)pl.CustomProperties["IsReadyChanged"]) //�÷��̾��� ready ������ �ٲ������ �Ǵ�
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "IsReadyChanged", false },
            };
                pl.SetCustomProperties(props); //�÷��̾��� ready changed �Ӽ� ��ü
                UpdatePanel(); //�г� ������Ʈ
            }
        }
    }
    private void ReadyListener()
    { //���� �÷��̾��� �غ� �Ӽ� ��ü �� �г� ������Ʈ
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
        PhotonNetwork.LeaveRoom();//�� ������
    }
    public override void OnLeftRoom()
    { //�� �������� ��, OnConnectedToMaster�� ȣ�� ��
        base.OnLeftRoom();
    }
    public override void OnConnectedToMaster()
    { //�κ� ����
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    { //�κ� ���� ���� �� UI �ʱ�ȭ
        base.OnJoinedLobby();
        Debug.Log(PhotonNetwork.InLobby);
        LobbyUI.SetActive(true);
        PhotonNetwork.AutomaticallySyncScene = false;
        gameObject.SetActive(false);
    }
    private void UpdatePanel()
    { //�÷��̾��� �Ӽ� �� �������� ���� UI ������Ʈ
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
    { //�÷��̾ �뿡 ���ӽ� UI������Ʈ
        base.OnPlayerEnteredRoom(newPlayer);
        UpdatePanel();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    { //�÷��̾ �濡 ���� �� UI������Ʈ
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePanel();
    }
    private void StartGame()
    {
        IsGameStarted = true;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.AutomaticallySyncScene = true; //�� �̵��� ������ Ŭ���̾�Ʈ(����)�� ����ȭ ��
        Player[] players = PhotonNetwork.PlayerList;
        int Pindex = -1;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsLocal)
            {
                Pindex = i;
            }
        }
        //���� Ŭ���̾�Ʈ �÷��̾��� ��ġ�� ���� ��� �ʱ�ȭ
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