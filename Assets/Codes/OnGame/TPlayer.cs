using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TPlayer : MonoBehaviourPunCallbacks
{
    public int PlayerType;

    private TBoard bd;
    TetroBehav tController;
    GameObject bdcp;
    bool isAiHandling = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerType == -1)
        {
            bd.enabled = false;
        }
        if (PlayerType == 3)
        {
            bdcp = Instantiate(bd.gameObject);
            bdcp.GetComponent<TBoard>().isghost = true;
            bdcp.transform.parent = this.transform;
            //Debug.Log(bdcp.GetComponent<TBoard>() == null);
            bdcp.GetComponent<TBoard>().enabled = true;
            bdcp.transform.position = new Vector3(-40, -9, 0.2f);
        }
    }
    private void Awake()
    {
        bd = GetComponentInChildren<TBoard>();
        //Debug.Log(bd == null);
    }
    private void Update()
    {
        if (bd.enabled != false)
        {
            tController = bd.getCurTetro()[0].GetComponent<TetroBehav>();
            SetInputByInfo();
        }
    }
    void SetInputByInfo()
    {
        switch (PlayerType)
        {
            case -1: //�÷��̾ ����
                break;

            case 1: //�÷��̾ ������
                Handle1PInput();
                break;

            case 2: //2��° �÷��̾� (���� ��Ƽ)
                Handle2PInput();
                break;

            case 3: //AI�� �÷���
                if (!isAiHandling)
                {
                    StartCoroutine(HandleAiInput()); //�Լ��� ���������� ���
                    return;
                }
                break;

            case 4: //poton ��Ƽ
                //HandlePhotonInput();
                break;

        }
    }
    private void Handle1PInput()
    {
        if (bd.getCurTetro() != null)
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
                tController.TFall();
            }
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                tController.MovetBottom();
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
                tController.TFall();
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                tController.MovetBottom();
            }
        }
    }
    private IEnumerator HandleAiInput()
    //ai ���� ��� > �ùķ��̼� > ��Ʈ�� ��� > ���� ���� ���� ����
    //��Ʈ�� >> 1. ���� ����� ���� 2. �� ������ ����
    {
        isAiHandling = true;
        bdcp.GetComponent<TBoard>().grid = (Transform[,])bd.grid.Clone();
        Transform[,] grid = bdcp.GetComponent<TBoard>().grid;
        //���� ���带 �������� ��Ʈ ���� ������Ʈ

        TetroBehav CurT = Instantiate(bd.getCurTetro()[0]).GetComponent<TetroBehav>();
        CurT.setParentBoard(bdcp);
        //���� ��Ʈ�ι̳� ��� ���纻 ����

        TetroBehav NexT = Instantiate(bd.getCurTetro()[1]).GetComponent<TetroBehav>();
        NexT.setParentBoard(bdcp);
        //���� ��Ʈ�ι̳� ��� ���纻 ����
        
        int CbestRotation = 0;
        int CbestPosition = 0;
        float bestscore = float.NegativeInfinity;
        for (int ctr = 0; ctr < 4; ctr++)
        {//��Ʈ�� ��Ʈ�� ȸ��
            for (int ctp = 0; ctp < 10; ctp++)
            {//��Ʈ�� ��Ʈ�� ��ġ
             //���� ��Ʈ�� ��ġ�ϱ�.bdcp
                CurT.transform.rotation = Quaternion.Euler(0, 0, ctr * 90);
                CurT.transform.position = bdcp.transform.position + new Vector3(ctp, 17, -0.2f);

                //Debug.Log(ctr + " " + ctp);

                if (!CurT.isValidMove())
                {
                    continue;
                }
                CurT.setMovable(true);
                CurT.MovetBottom();
                for (int ntr = 0; ntr < 4; ntr++)
                {//���� ��Ʈ�� ȸ��
                    for (int ntp = 0; ntp < 10; ntp++)
                    {//���� ��Ʈ�� ��ġ
                     //���� ��Ʈ�� ��ġ�ϱ�
                        NexT.transform.rotation = Quaternion.Euler(0, 0, ctr * 90);
                        NexT.transform.position = bdcp.transform.position + new Vector3(ntp, 17, -0.2f);

                        if (!NexT.isValidMove())
                        {
                            continue;
                        }
                        NexT.setMovable(true);
                        NexT.MovetBottom();

                        float curScore = CalculateBoardScore(grid);
                        if (bestscore < curScore)
                        {
                            bestscore = curScore;
                            CbestRotation = ctr;
                            CbestPosition = ctp;
                        }
                        else if (bestscore == curScore) //������ ���� �� �����ϰ� ����
                        {
                            int x = Random.Range(0, 2);
                            if (x == 1)
                            {
                                bestscore = curScore;
                                CbestRotation = ctr;
                                CbestPosition = ctp;
                            }
                        }
                        NexT.DSave();
                    }
                }
                CurT.DSave();
            }
        }
        //�ùķ��̼� �� ��� �̵�
        //Debug.Log("RESULt " + CbestRotation + " " + CbestPosition + " " + bestscore);
        tController.TFall(); tController.TFall();
        for (int i = 0; i < CbestRotation; i++)
        {
            tController.TRotate();
        }
        int pos = 4;
        while (pos != CbestPosition)
        {
            if (pos < CbestPosition)
            {
                tController.TMove(1);
                pos++;
            }
            else
            {
                tController.TMove(0);
                pos--;
            }
        }

        while (tController.getMovable())
        {
            yield return new WaitForSeconds(0.1f);
            tController.TFall();
        }
        Destroy(NexT.gameObject);
        Destroy(CurT.gameObject);
        yield return null;
        isAiHandling = false;
    }
    private float CalculateBoardScore(Transform[,] grid)
    {
        Transform[,] Simgrid = (Transform[,])grid.Clone();
        // ���� �迭 �߰�, �� �����迭�� ���ؼ� ������Ʈ
        for (int y = 0; y < 20; y++)
        {
            if (IsRowFull(Simgrid, y))
            {
                AssumeDeleteRow(Simgrid, y);
            }
        }
        // �� ��ǥ�� ����ϰ� ����ġ�� �����Ͽ� ������ ������ ����մϴ�.
        float sumHoles = CalculateHoles(Simgrid) * -1f; // ���� �� 3 0
        float sumHeight = CalculateSumHeight(Simgrid) * -1f; // �� ������ �� 10 4
        float rowFlips = CalculateRowFlips(Simgrid) * -1f; // �� ������ �� ���°� �ٲ�� Ƚ�� 2 0 
        float columnFlips = CalculateColumnFlips(Simgrid) * -1f; // �� ������ �� ���°� �ٲ�� Ƚ�� 6 1

        return sumHoles + sumHeight + rowFlips + columnFlips;
    }
    private bool IsRowFull(Transform[,] grid, int y)
    {
        for (int x = 0; x < 10; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }

    private void AssumeDeleteRow(Transform[,] grid, int y)
    {
        for (int i = y; i < 19; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                grid[j, i] = grid[j, i + 1];
            }
        }
        for (int j = 0; j < 10; j++)
        {
            grid[j, 19] = null;
        }
    }

    private float CalculateHoles(Transform[,] grid)
    { /* ���� �� ��� ���� */
        float sumHoles = 0;
        for (int x = 0; x < 10; x++)
        {
            bool startCounting = false;
            for (int y = 19; y >= 0; y--)
            {
                if (grid[x, y] == null)
                {
                    if (startCounting)
                    {
                        startCounting = false;
                        sumHoles++;
                    }
                }
                else
                {
                    startCounting = true;
                }
            }
        }
        return sumHoles;
    }
    private float CalculateSumHeight(Transform[,] grid)
    { /* �� ������ �� ��� ���� */
        float sumHeight = 0;

        for (int x = 0; x < 10; x++)
        {
            for (int y = 19; y >= 0; y--)
            {
                if (grid[x, y] != null)
                {
                    sumHeight += y;
                    break;
                }
            }
        }
        return sumHeight;
    }

    private float CalculateRowFlips(Transform[,] grid)
    { /* �� ������ �� ���°� �ٲ�� Ƚ�� ��� ���� */
        float rowFlips = 0;
        for (int y = 0; y < 20; y++)
        {
            bool prevCellFilled = true;  // ���� ����� ä���� �ִٰ� ����
            for (int x = -1; x <= 10; x++)
            {
                bool currentCellFilled;
                if (x < 0 || x >= 10)
                {
                    currentCellFilled = true;  // ���� ����� ä���� �ִٰ� ����
                }
                else
                {
                    currentCellFilled = grid[x, y] != null;
                }
                if (prevCellFilled != currentCellFilled)
                {
                    rowFlips++;
                }
                prevCellFilled = currentCellFilled;
            }
        }
        return rowFlips;
    }

    private float CalculateColumnFlips(Transform[,] grid)
    {   /* �� ������ �� ���°� �ٲ�� Ƚ�� ��� ���� */
        float columnFlips = 0;
        for (int x = 0; x < 10; x++)
        {
            bool prevCellFilled = true;  // ���� ����� ä���� �ִٰ� ����
            for (int y = -1; y <= 20; y++)
            {
                bool currentCellFilled;
                if (y < 0 || y >= 20)
                {
                    currentCellFilled = true;  // ���� ����� ä���� �ִٰ� ����
                }
                else
                {
                    currentCellFilled = grid[x, y] != null;
                }
                if (prevCellFilled != currentCellFilled)
                {
                    columnFlips++;
                }
                prevCellFilled = currentCellFilled;
            }
        }
        return columnFlips;
    }

    private float CalculatePieceHeight(Transform[,] grid)
    {   /* ���� �ֱٿ� ���� ����� ���� ��� ���� */
        float pieceHeight = 0;

        for (int x = 0; x < 10; x++)
        {
            for (int y = 19; y >= 0; y--)
            {
                if (grid[x, y] != null)
                {
                    if (pieceHeight < y)
                    {
                        pieceHeight = y;
                        break;
                    }
                }
            }
        }

        return pieceHeight;
    }
    public TBoard GetPlayerBD()
    {
        return bd;
    }

    private void HandlePhotonInput()
    {
        if (bd.getCurTetro() != null)
        {
            // PhotonNetwork.playerList �� ����Ͽ� �ٸ� �÷��̾��� �Է��� �޾ƿɴϴ�.
            // �̸� ����, �� �÷��̾�� �ڽ��� �Է��� PhotonNetwork�� �����ؾ� �մϴ�.
            // ���� ���, �÷��̾ ���� ȭ��ǥ Ű�� ������, �� ������ ��� �÷��̾�� �˸��ϴ�.
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player != PhotonNetwork.LocalPlayer) // �ٸ� �÷��̾��� �Է¸� ó���մϴ�.
                {
                    // �÷��̾��� �Է��� �޾ƿͼ� ó���մϴ�.
                    // �� ���ÿ����� �����ϰ� Ű �Է¸��� �޾ƿ�����, ���� ���ӿ����� �÷��̾��� ��ü �Է��� �޾ƿ;� �մϴ�.
                    string input = (string)player.CustomProperties["input"];
                    switch (input)
                    {
                        case "left":
                            tController.TMove(0);
                            break;
                        case "right":
                            tController.TMove(1);
                            break;
                        case "rotate":
                            tController.TRotate();
                            break;
                        case "down":
                            tController.TFall();
                            break;
                        case "drop":
                            tController.MovetBottom();
                            break;
                    }
                }
            }
        }
    }
}
