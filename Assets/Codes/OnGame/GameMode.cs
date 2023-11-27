using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameMode : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private TPlayer P1;
    [SerializeField]
    private TPlayer P2;

    [SerializeField]
    private Text P1Name;
    [SerializeField]
    private Text P2Name;

    [SerializeField]
    private GameObject GameOverUI;
    private bool isGameRunning = false;
    TPlayer winner;

    [SerializeField]
    private Button ReStartBtn;
    [SerializeField]
    private Button MenuBtn;

    [SerializeField]
    private Text StartCount;
    [SerializeField]
    private Button exitBtn;
    void Awake()
    { //각 플레이어 초기화 및 버튼 리스너 부착
        P1.setPlayerType(PlayerData.Instance.Get1P_Mode());
        P2.setPlayerType(PlayerData.Instance.Get2P_Mode());
        P1Name.text = PlayerData.Instance.GetP1Name();
        P2Name.text = PlayerData.Instance.GetP2Name();
        ReStartBtn.onClick.AddListener(ReStart);
        MenuBtn.onClick.AddListener(GoMenu);
        exitBtn.onClick.AddListener(EXIT);
        StartCoroutine(GameWait());
    }

    private IEnumerator GameWait()
    { //비동기로 3초 대기후 게임 시작 설정
        for (int i = 3; i >= 1; i--)
        {
            StartCount.text = i.ToString() + "초";
            yield return new WaitForSeconds(1);
        }
        P1.GetPlayerBD().enabled = true;
        if (P2.getPlayerType() != -1)
        {
            P2.GetPlayerBD().enabled = true;
        }
        //보드 스크립트를 활성화 시킴으로 게임 시작
        StartCount.text = "Start";
        yield return new WaitForSeconds(1);
        StartCount.text = "";
        yield return null;
        isGameRunning = true;
    }

    private void Update()
    {
        if (isGameRunning)
        {
            //P1이 먼저 탈락
            if (!P1.GetPlayerBD().enabled && P2.GetPlayerBD().enabled)
            {
                winner = P2;
                if(PlayerData.Instance.Get2P_Mode() == 3)
                { //상대가 ai일시 바로 게임 종료
                    GameOverUI.transform.GetChild(0).GetComponent<Text>().text = "Winner\n" + P2Name.text;
                    P2.GetPlayerBD().enabled = false;
                    GameOverUI.SetActive(true);
                    isGameRunning = false;
                }
            }
            // P2가 먼저 탈락
            if (P1.GetPlayerBD().enabled && !P2.GetPlayerBD().enabled)
            {
                winner = P1;
            }

            if (!P1.GetPlayerBD().enabled && !P2.GetPlayerBD().enabled) //둘 다 종료 시
            {
                if (P2.getPlayerType() != -1) //1인 플레이가 아닐 시에 승자 출력
                {
                    if (winner == P1)
                    {
                        GameOverUI.transform.GetChild(0).GetComponent<Text>().text = "Winner\n" + P1Name.text;
                    }
                    else
                    {
                        GameOverUI.transform.GetChild(0).GetComponent<Text>().text = "Winner\n" + P2Name.text;
                    }
                }
                GameOverUI.SetActive(true);
                isGameRunning = false;
                if (PhotonNetwork.IsConnected)
                { //온라인 플레이일시 5초후 로비로 돌아감
                    ReStartBtn.gameObject.SetActive(false);
                    MenuBtn.gameObject.SetActive(false);
                    StartCoroutine(OnlineGameOver());
                }
            }
        }
    }

    private void GoMenu() 
    {
        SceneManager.LoadScene("StartMenu");
    }
    private void ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private IEnumerator OnlineGameOver()
    {
        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player player in players)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                {"IsReady", false}
            };
            player.SetCustomProperties(props);
        }
        for (int i = 5; i >= 1; i--)
        {
            string winnername = (winner == P1 ? P1Name.text : P2Name.text);
            GameOverUI.transform.GetChild(0).GetComponent<Text>().text = "Winner\n" + winnername + "\n" + i.ToString() + "초 후 로비";
            yield return new WaitForSeconds(1);
        }
        SceneManager.LoadScene("OnlineRoom");
    }
    private void EXIT()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        GoMenu();
    }
}
