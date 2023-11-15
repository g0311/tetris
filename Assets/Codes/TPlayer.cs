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
                    StartCoroutine(HandleAiInput());
                }
                break;

            case 4: //poton ��Ƽ
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


        TetroBehav CurT = Instantiate(bd.getCurTetro()[0]).GetComponent<TetroBehav>();
        CurT.setParentBoard(bdcp);

        TetroBehav NexT = Instantiate(bd.getCurTetro()[1]).GetComponent<TetroBehav>();
        NexT.setParentBoard(bdcp);

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
                    for(int ntp = 0; ntp < 10; ntp++)
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
                        AHeight = CalculateAverHeight(grid);
                        //EBlcok = CalculateEmptyBlock(grid);
                        float curScore = AHeight + EBlcok;

                        if(bestscore > curScore) //���� ���� ����
                        {
                            bestscore = curScore;
                            CbestRotation = ctr;
                            CbestPosition = ctp;
                        }
                        else if( bestscore == curScore) //������ ���� �� �����ϰ� ����
                        {
                            int x = Random.Range(0, 2);
                            if(x == 1)
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
            if(pos < CbestPosition)
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
        for(int i = 0; i < 10; i++)
        {
            for (int j = 19; j >= 0; j--)
            {
                if(grid[i,j] != null)
                {
                    SumHeight += j + 1;
                    break;
                }
            }
        }
        SumHeight /= 10;
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
                if (BlStartPoint && grid[i,j] == null)
                {
                    SumEBlock++;
                }
            }
        }
        return SumEBlock;
    }
}
