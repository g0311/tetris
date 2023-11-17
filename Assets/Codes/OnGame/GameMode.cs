using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMode : MonoBehaviour
{
    // Start is called before the first frame update
    public TPlayer P1;
    public TPlayer P2;

    public Text StartCount;
    void Start()
    {
        P1.PlayerType = 1;
        P2.PlayerType = PlayerData.Instance.Get2P_Mode();
        StartCoroutine(GameWait());
    }

    private IEnumerator GameWait()
    {
        for (int i = 3; i >= 1; i--)
        {
            StartCount.text = i.ToString() + "ÃÊ Àü";
            yield return new WaitForSeconds(1);
        }
        P1.GetPlayerBD().enabled = true;
        P2.GetPlayerBD().enabled = true;
        StartCount.text = "Start";
        yield return new WaitForSeconds(1);

        yield return null;
    }

}
