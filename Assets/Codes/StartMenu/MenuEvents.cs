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
    private GameObject LoginUI;

    void Start()
    {
        _1P.onClick.AddListener(_1PListener);
        _L2P.onClick.AddListener(_L2PListener);
        _O2P.onClick.AddListener(_O2PListener);
        _AI.onClick.AddListener(_AIListener);
        _Exit.onClick.AddListener(_EXITListener);
        LoginUI.GetComponentInChildren<Button>().onClick.AddListener(_LoginListener);
    }

    void _1PListener()
    {
        PlayerData.Instance.SetP2Name("");
        PlayerData.Instance.Set2P_Mode(-1);
        SceneManager.LoadScene("OnGame");
    }
    void _L2PListener()
    {
        PlayerData.Instance.SetP2Name("2P");
        PlayerData.Instance.Set2P_Mode(2);
        SceneManager.LoadScene("OnGame");
    }
    void _O2PListener()
    {
        LoginUI.SetActive(true);
    }
    void _AIListener()
    {
        PlayerData.Instance.SetP2Name("AI");
        PlayerData.Instance.Set2P_Mode(3);
        SceneManager.LoadScene("OnGame");
    }
    void _EXITListener()
    {
        Application.Quit();
    }
    void _LoginListener()
    {
        PlayerData.Instance.SetP1Name(LoginUI.GetComponentInChildren<InputField>().text);
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnected()
    {
        base.OnConnected();
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        PhotonNetwork.LoadLevel("OnlineRoom");
    }
}
