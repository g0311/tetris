using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPlayer : MonoBehaviour
{
    public int PlayerType;

    private TBoard bd;
    TetroBehav tController;
    // Start is called before the first frame update
    void Start()
    {
        bd = GetComponentInChildren<TBoard>();
        bd.enabled = false;
    }
    private void Update()
    {
        if (bd.getCurTetro() != null)
        {
            tController = bd.getCurTetro().GetComponent<TetroBehav>();
            SetInputByInfo();
        }
    }
    void SetInputByInfo()
    {
        switch (PlayerType)
        {
            case -1: //플레이어가 없음
                bd.enabled = false;
                break;

            case 1: //플레이어가 본인임
                bd.enabled = true;
                Handle1PInput();
                break;

            case 2: //2번째 플레이어 (로컬 멀티)
                bd.enabled = true;
                bd.GetComponent<TBoard>().enabled = true;
                Handle2PInput();
                break;

            case 3: //AI가 플레이
                bd.enabled = true;
                bd.GetComponent<TBoard>().enabled = true;
                HandleAiInput();
                break;

            case 4: //poton 멀티
                break;

        }
    }
    private void Handle1PInput()
    {
        if(bd.getCurTetro() != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                tController.TMove(0);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                tController.TMove(1);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                tController.TRotate();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                tController.setCurt(2);
            }
        }
    }
    private void Handle2PInput()
    {
        if (bd.getCurTetro() != null)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                tController.TMove(0);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                tController.TMove(1);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                tController.TRotate();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                tController.setCurt(2);
            }
        }
    }
    private void HandleAiInput()
    {
        //AI 작동 방식 및 판단 구현하면 되나?
    }
}
