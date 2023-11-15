using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPlayer : MonoBehaviour
{
    public int PlayerType;

    private TBoard bd;
    TetroBehav tController;
    GameObject bdcp;
    bool isAiHandling = false;
    // Start is called before the first frame update
    void Start()
    {
        bd = GetComponentInChildren<TBoard>();
        if (PlayerType == -1)
        {
            bd.enabled = false;
        }
        if (PlayerType == 3)
        {
            bdcp = Instantiate(bd.gameObject);
            bdcp.GetComponent<TBoard>().isghost = true;
            bdcp.transform.parent = this.transform;
            bdcp.transform.position = new Vector3(-40, -9, 0.2f);
        }
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
                }
                break;

            case 4: //poton ��Ƽ
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
        bdcp = Instantiate(bd.gameObject);
        bdcp.GetComponent<TBoard>().isghost = true;
        bdcp.transform.parent = this.transform;
        bdcp.transform.position = new Vector3(-40, -9, 0.2f);
        bdcp.GetComponent<TBoard>().grid = (Transform[,])bd.grid.Clone();
        Transform[,] grid = bdcp.GetComponent<TBoard>().grid;
        //���� ���带 �������� ��Ʈ ���� ����

        TetroBehav CurT = Instantiate(bd.getCurTetro()[0]).GetComponent<TetroBehav>();
        CurT.setParentBoard(bdcp);
        //���� ��Ʈ�ι̳� ��� ���纻 ����

        TetroBehav NexT = Instantiate(bd.getCurTetro()[1]).GetComponent<TetroBehav>();
        NexT.setParentBoard(bdcp);
        //���� ��Ʈ�ι̳� ��� ���纻 ����
        Debug.Log("AIHANDLING");

        int CbestRotation = 0;
        int CbestPosition = 0;
        float AHeight = 0;
        float EBlcok = 0;
        float bestscore = float.PositiveInfinity;
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
                        NexT.transform.Rotate(new Vector3(0, 0, 90));
                        NexT.transform.position = bdcp.transform.position + new Vector3(ntp, 17, -0.2f);

                        if (!NexT.isValidMove())
                        {
                            continue;
                        }
                        Debug.Log("�ù�");
                        CurT.setMovable(true);
                        NexT.MovetBottom();
                        AHeight = CalculateAverHeight(grid) * 3;
                        EBlcok = CalculateEmptyBlock(grid) * 5;
                        float CLineCompletion = CalculateLineCompletion(grid) * 10; // add this line
                        float DisconBlocks = CalculateDisconnectedBlocks(grid) * 5;
                        float curScore = AHeight + EBlcok + CLineCompletion;


                        if (bestscore > curScore) //���� ���� ����
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
        Debug.Log("RESULt " + AHeight + " " + EBlcok + " " + CbestRotation + " " + CbestPosition + " " + bestscore);
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
        tController.MovetBottom();
        Destroy(bdcp);
        Destroy(NexT.gameObject);
        Destroy(CurT.gameObject);
        yield return null;
        isAiHandling = false;
    }
    private float CalculateAverHeight(Transform[,] grid)
    {
        float SumHeight = 0;
        int nonEmptyColumns = 0;
        for (int i = 0; i < 10; i++)
        {
            bool isEmptyColumn = true;
            for (int j = 19; j >= 0; j--)
            {
                if (grid[i, j] != null)
                {
                    SumHeight += j + 1;
                    isEmptyColumn = false;
                    break;
                }
            }
            if (!isEmptyColumn)
            {
                nonEmptyColumns++;
            }
        }
        SumHeight /= nonEmptyColumns;
        return SumHeight;
    }

    private float CalculateEmptyBlock(Transform[,] grid)
    {
        float SumEBlock = 0;
        bool BlStartPoint = false;
        for (int i = 0; i < 10; i++)
        {
            BlStartPoint = false;
            for (int j = 19; j >= 0; j--)
            {
                if (grid[i, j] != null)
                {
                    BlStartPoint = true;
                }
                if (BlStartPoint && grid[i, j] == null)
                {
                    SumEBlock += j + 1; // ���̸� ����Ͽ� �� ����� ���赵�� ���
                }
            }
        }
        return SumEBlock;
    }
    private float CalculateLineCompletion(Transform[,] grid)
    {
        float totalCompletion = 0;
        for (int j = 0; j < 20; j++)
        {
            int lineCompletion = 0;
            for (int i = 0; i < 10; i++)
            {
                if (grid[i, j] != null)
                {
                    lineCompletion++;
                }
            }
            totalCompletion += lineCompletion;
        }
        return totalCompletion / (20 * 10); // normalize the value
    }
    private float CalculateDisconnectedBlocks(Transform[,] grid)
    {
        float disconnectedBlocks = 0;
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                if (grid[i, j] != null)
                {
                    // ���� ��� �˻�
                    if (i > 0 && grid[i - 1, j] == null)
                    {
                        disconnectedBlocks++;
                    }
                    // ������ ��� �˻�
                    if (i < 9 && grid[i + 1, j] == null)
                    {
                        disconnectedBlocks++;
                    }
                }
            }
        }
        return disconnectedBlocks;
    }
}
