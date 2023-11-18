using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviour
{
    [SerializeField]
    private Button ReadyBtn;
    [SerializeField]
    private Button ExitBtn;
    [SerializeField]
    private GameObject P1Pannel;
    [SerializeField]
    private GameObject P2Pannel;
    // Start is called before the first frame update
    private void Awake()
    {
        ReadyBtn.onClick.AddListener(ReadyListener);
        ExitBtn.onClick.AddListener(ExitListener);
    }
    void OnEnable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ReadyListener()
    {

    }
    private void ExitListener()
    {

    }
}
