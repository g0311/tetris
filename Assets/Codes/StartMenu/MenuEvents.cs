using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MenuEvents : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button _1P;
    [SerializeField]
    private Button _L2P;
    [SerializeField]
    private Button _AI;
    [SerializeField]
    private Button _O2P;
    [SerializeField]
    private Button _Exit;
    [SerializeField]
    private GameObject LoginUI; //Online 2P 서버 접속 전 사용할 플레이어 이름

    void Start()
    {
        _1P.onClick.AddListener(_1PListener);
        _L2P.onClick.AddListener(_L2PListener);
        _O2P.onClick.AddListener(_O2PListener);
        _AI.onClick.AddListener(_AIListener);
        _Exit.onClick.AddListener(_EXITListener);
        Button[] LoginBtns = LoginUI.GetComponentsInChildren<Button>();
        LoginBtns[0].onClick.AddListener(_LoginListener);
        LoginBtns[1].onClick.AddListener(_CancelListener);
    }
    //각 버튼은 플레이어 이름 및 모드를 설정하고 씬을 전환
    void _1PListener()
    {
        PlayerData.Instance.SetP1Name("1P");
        PlayerData.Instance.SetP2Name("");
        PlayerData.Instance.Set1P_Mode(1);
        PlayerData.Instance.Set2P_Mode(-1);
        SceneManager.LoadScene("OnGame");
    }
    void _L2PListener()
    {
        PlayerData.Instance.SetP1Name("1P");
        PlayerData.Instance.SetP2Name("2P");
        PlayerData.Instance.Set1P_Mode(1);
        PlayerData.Instance.Set2P_Mode(2);
        SceneManager.LoadScene("OnGame");
    }
    void _AIListener()
    {
        PlayerData.Instance.SetP1Name("1P");
        PlayerData.Instance.SetP2Name("AI");
        PlayerData.Instance.Set1P_Mode(1);
        PlayerData.Instance.Set2P_Mode(3);
        SceneManager.LoadScene("OnGame");
    }
    void _EXITListener()
    {
        Application.Quit();
    }
    void _O2PListener()
    {
        LoginUI.SetActive(true);
    }
    void _CancelListener() //로그인 취소 버튼 리스너
    {
        LoginUI.GetComponentInChildren<InputField>().text = "";
        LoginUI.SetActive(false);
    }
    void _LoginListener() //로그인 버튼 리스너
    {
        PlayerData.Instance.SetCurName(LoginUI.GetComponentInChildren<InputField>().text);
        PhotonNetwork.ConnectUsingSettings(); //서버 접속 시도
    }
    public override void OnConnectedToMaster()
    { //서버 접속 성공 시
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby(); //로비 접속 시도
    }
    public override void OnJoinedLobby()
    { //로비 접속 성공 시
        base.OnJoinedLobby();
        PhotonNetwork.LocalPlayer.NickName = PlayerData.Instance.GetCurName();
        PhotonNetwork.LoadLevel("OnlineRoom"); //OnlineRoom 씬 전환
    }
}
