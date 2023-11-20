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
    //ai 제작 방법 > 시뮬레이션 > 메트릭 계산 > 가장 좋은 동작 선택
    //메트릭 >> 1. 높이 평균이 낮게 2. 빈 구멍이 적게
    {
        isAiHandling = true;
        bdcp.GetComponent<TBoard>().grid = (Transform[,])bd.grid.Clone();
        Transform[,] grid = bdcp.GetComponent<TBoard>().grid;
        //실제 보드를 기준으로 고스트 보드 업데이트

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
    public TBoard GetPlayerBD()
    {
        return bd;
    }

    private void HandlePhotonInput()
    {
        if (bd.getCurTetro() != null)
        {
            // PhotonNetwork.playerList 를 사용하여 다른 플레이어의 입력을 받아옵니다.
            // 이를 위해, 각 플레이어는 자신의 입력을 PhotonNetwork에 전송해야 합니다.
            // 예를 들어, 플레이어가 왼쪽 화살표 키를 누르면, 이 정보를 모든 플레이어에게 알립니다.
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player != PhotonNetwork.LocalPlayer) // 다른 플레이어의 입력만 처리합니다.
                {
                    // 플레이어의 입력을 받아와서 처리합니다.
                    // 이 예시에서는 간단하게 키 입력만을 받아오지만, 실제 게임에서는 플레이어의 전체 입력을 받아와야 합니다.
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
