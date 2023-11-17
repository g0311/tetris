using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    // Start is called before the first frame update
    public TPlayer P1;
    public TPlayer P2;

    public Text P1Name;
    public Text P2Name;
    
    public GameObject GameOverUI;
    bool isGameRunning = false;
    bool winner = true;

    public Button ReStartBtn;
    public Button MenuBtn;

    public Text StartCount;
    void Awake()
    {
        P1.PlayerType = 1;
        P2.PlayerType = PlayerData.Instance.Get2P_Mode();
        P1Name.text = PlayerData.Instance.GetP1Name();
        P2Name.text = PlayerData.Instance.GetP2Name();
        ReStartBtn.onClick.AddListener(ReStart);
        MenuBtn.onClick.AddListener(GoMenu);
        StartCoroutine(GameWait());
    }

    private IEnumerator GameWait()
    {
        for (int i = 3; i >= 1; i--)
        {
            StartCount.text = i.ToString() + "초";
            yield return new WaitForSeconds(1);
        }
        P1.GetPlayerBD().enabled = true;
        if (P2.PlayerType != -1)
        {
            P2.GetPlayerBD().enabled = true;
        }
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
            if (!P1.GetPlayerBD().enabled && P2.GetPlayerBD().enabled)
            {
                winner = false;
            }

            if (!P1.GetPlayerBD().enabled && !P2.GetPlayerBD().enabled) //이긴 애 출력 근데 혼자 할떈?
            {
                if (P2.PlayerType != -1)
                {
                    if (winner)
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

}
