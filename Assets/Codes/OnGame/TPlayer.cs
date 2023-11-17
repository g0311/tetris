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
    private void Awake()
    {
        bd = GetComponentInChildren<TBoard>();
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
            case -1: //플레이어가 없음
                break;

            case 1: //플레이어가 본인임
                Handle1PInput();
                break;

            case 2: //2번째 플레이어 (로컬 멀티)
                Handle2PInput();
                break;

            case 3: //AI가 플레이
                if (!isAiHandling)
                {
                    StartCoroutine(HandleAiInput()); //함수가 끝날때까지 대기
                    return;
                }
                break;

            case 4: //poton 멀티
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
    //ai 제작 방법 > 시뮬레이션 > 메트릭 계산 > 가장 좋은 동작 선택
    //메트릭 >> 1. 높이 평균이 낮게 2. 빈 구멍이 적게
    {
        isAiHandling = true;
        bdcp = Instantiate(bd.gameObject);
        bdcp.GetComponent<TBoard>().isghost = true;
        bdcp.transform.parent = this.transform;
        bdcp.transform.position = new Vector3(-40, -9, 0.2f);
        bdcp.GetComponent<TBoard>().grid = (Transform[,])bd.grid.Clone();
        Transform[,] grid = bdcp.GetComponent<TBoard>().grid;
        //실제 보드를 기준으로 고스트 보드 생성

        TetroBehav CurT = Instantiate(bd.getCurTetro()[0]).GetComponent<TetroBehav>();
        CurT.setParentBoard(bdcp);
        //현재 테트로미노 블록 복사본 생성

        TetroBehav NexT = Instantiate(bd.getCurTetro()[1]).GetComponent<TetroBehav>();
        NexT.setParentBoard(bdcp);
        //다음 테트로미노 블록 복사본 생성
        
        int CbestRotation = 0;
        int CbestPosition = 0;
        float bestscore = float.NegativeInfinity;
        for (int ctr = 0; ctr < 4; ctr++)
        {//컨트롤 테트로 회전
            for (int ctp = 0; ctp < 10; ctp++)
            {//컨트롤 테트로 위치
             //지금 테트로 배치하기.bdcp
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
                {//다음 테트로 회전
                    for (int ntp = 0; ntp < 10; ntp++)
                    {//다음 테트로 위치
                     //다음 테트로 배치하기
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
                        else if (bestscore == curScore) //점수가 같을 시 랜덤하게 선택
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
        //시뮬레이션 값 대로 이동
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

        Destroy(bdcp);
        Destroy(NexT.gameObject);
        Destroy(CurT.gameObject);
        yield return null;
        isAiHandling = false;
    }
    private float CalculateBoardScore(Transform[,] grid)
    {
        Transform[,] Simgrid = (Transform[,])grid.Clone();
        // 복제 배열 추가, 그 복제배열에 대해서 업데이트
        for (int y = 0; y < 20; y++)
        {
            if (IsRowFull(Simgrid, y))
            {
                AssumeDeleteRow(Simgrid, y);
            }
        }
        // 각 지표를 계산하고 가중치를 적용하여 보드의 점수를 계산합니다.
        float sumHoles = CalculateHoles(Simgrid) * -1f; // 구멍 수 3 0
        float sumHeight = CalculateSumHeight(Simgrid) * -1f; // 열 높이의 합 10 4
        float rowFlips = CalculateRowFlips(Simgrid) * -1f; // 행 내에서 셀 상태가 바뀌는 횟수 2 0 
        float columnFlips = CalculateColumnFlips(Simgrid) * -1f; // 열 내에서 셀 상태가 바뀌는 횟수 6 1
        float pieceHeight = CalculatePieceHeight(Simgrid) * -1f; // 최고 블록의 높이 0 0
        float sumWell = CalculateSumWell(Simgrid) * -1f; // 우물의 크기 -3 -2


        return sumHoles + sumHeight + rowFlips + columnFlips; // + pieceHeight; // + sumWell;
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
    { /* 구멍 수 계산 로직 */
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
    { /* 열 높이의 합 계산 로직 */
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
    { /* 행 내에서 셀 상태가 바뀌는 횟수 계산 로직 */
        float rowFlips = 0;
        for (int y = 0; y < 20; y++)
        {
            bool prevCellFilled = true;  // 벽을 블록이 채워져 있다고 간주
            for (int x = -1; x <= 10; x++)
            {
                bool currentCellFilled;
                if (x < 0 || x >= 10)
                {
                    currentCellFilled = true;  // 벽을 블록이 채워져 있다고 간주
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
    {   /* 열 내에서 셀 상태가 바뀌는 횟수 계산 로직 */
        float columnFlips = 0;
        for (int x = 0; x < 10; x++)
        {
            bool prevCellFilled = true;  // 벽을 블록이 채워져 있다고 간주
            for (int y = -1; y <= 20; y++)
            {
                bool currentCellFilled;
                if (y < 0 || y >= 20)
                {
                    currentCellFilled = true;  // 벽을 블록이 채워져 있다고 간주
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
    {   /* 가장 최근에 놓인 블록의 높이 계산 로직 */
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
    private float CalculateSumWell(Transform[,] grid)
    {
        float sumWell = 0; // 우물의 총 깊이를 저장할 변수

        // 모든 열에 대해 반복
        for (int x = 0; x < 10; x++)
        {
            int wellDepth = 0; // 현재 열의 우물 깊이를 저장할 변수

            // 한 열의 맨 위부터 맨 아래까지 반복
            for (int y = 19; y >= 0; y--)
            {
                // 현재 위치에 블록이 없는 경우
                if (grid[x, y] == null)
                {
                    // 맨 왼쪽 끝이거나, 왼쪽에 블록이 있는 경우
                    // 또는 맨 오른쪽 끝이거나, 오른쪽에 블록이 있는 경우
                    // 즉, 현재 위치가 우물의 일부분이라면
                    if ((x == 0 || grid[x - 1, y] != null) &&
                        (x == 9 || grid[x + 1, y] != null))
                    {
                        wellDepth++; // 우물 깊이를 증가
                    }
                }
                else // 현재 위치에 블록이 있는 경우 
                {
                    // 우물이 끝난 경우
                    if (wellDepth > 0)
                    {
                        // 총 우물 깊이에 현재 우물 깊이의 제곱을 더함
                        // 제곱을 하는 이유는 깊이가 깊은 우물에 더 큰 페널티를 부과하기 위함
                        sumWell += wellDepth * wellDepth;
                        wellDepth = 0; // 우물 깊이 초기화
                        break;
                    }
                }
            }

            // 한 열이 끝났을 때 아직 계산되지 않은 우물이 남아있는 경우
            if (wellDepth > 0)
            {
                sumWell += wellDepth * wellDepth; // 남은 우물 깊이를 총 우물 깊이에 더함
            }
        }

        return sumWell; // 총 우물 깊이 반환
    }

    public TBoard GetPlayerBD()
    {
        return bd;
    }
}
