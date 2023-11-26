using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TPlayer : MonoBehaviourPunCallbacks
{
    private int PlayerType;
    [SerializeField]
    private GameObject PrefabBoard;
    private TBoard bd;
    private TetroBehav tController;
    private GameObject bdcp;
    private bool isAiHandling = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerType == 3)
        { //ai 플레이어일 시 시뮬레이팅을 위한 카피 보드 생성
            bdcp = Instantiate(PrefabBoard);
            bdcp.GetComponent<TBoard>().setisGhost(true);
            bdcp.transform.parent = this.transform;
            bdcp.GetComponent<TBoard>().enabled = true;
            bdcp.transform.position = new Vector3(-40, -9, 0.2f);
        }
    }
    private void Awake()
    { //초기화 함수
        //플레이어의 보드를 가져옴
        bd = GetComponentInChildren<TBoard>();
    }
    private void Update()
    {
        if (bd.enabled != false && PlayerType != 4)
        { //온라인 플레이의 경우 상대 클라이언트엔 컨트롤 중인 테트로미노의 정보가 없음
            tController = bd.getCurTetro()[0].GetComponent<TetroBehav>();
            SetInputByInfo();
        }
    }
    void SetInputByInfo()
    { //각 모드에 따라 입력 처리
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
                    StartCoroutine(HandleAiInput()); //메서드가 끝날때까지 대기
                    return;
                }
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
    { //코루틴 (비동기 호출)
        isAiHandling = true; //update()를 통해 동시에 여러번 수행되지 않게 설정
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
        {//현재 테트로 회전
            for (int ctp = 0; ctp < 10; ctp++)
            {//현재 테트로 위치
                CurT.transform.rotation = Quaternion.Euler(0, 0, ctr * 90);
                CurT.transform.position = bdcp.transform.position + new Vector3(ctp, 17, -0.2f);
                if (!CurT.isValidMove())
                {
                    continue;
                }
                CurT.setMovable(true);
                CurT.MovetBottom();
                //현재 테트로 배치하기
                
                for (int ntr = 0; ntr < 4; ntr++)
                {//다음 테트로 회전
                    for (int ntp = 0; ntp < 10; ntp++)
                    {//다음 테트로 위치
                        NexT.transform.rotation = Quaternion.Euler(0, 0, ctr * 90);
                        NexT.transform.position = bdcp.transform.position + new Vector3(ntp, 17, -0.2f);
                        if (!NexT.isValidMove())
                        {
                            continue;
                        }
                        NexT.setMovable(true);
                        NexT.MovetBottom();
                        //다음 테트로 배치하기
                        
                        float curScore = CalculateBoardScore(grid); //보드 점수 계산 및 저장
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
                        NexT.DSave(); //grid에서 테트로미노 위치정보 제거
                    }
                }
                CurT.DSave(); //grid에서 테트로미노 위치정보 제거
            }
        }

        //시뮬레이션 값 대로 이동
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
        //ai 플레이어의 이동에 제약 (0.1초에 한번씩 낙하 가능)
        while (tController.getMovable())
        {
            yield return new WaitForSeconds(0.1f); //0.1초 후에 메서드 재게
            tController.TFall();
        }
        //복사된 테트로미노 제거
        Destroy(NexT.gameObject);
        Destroy(CurT.gameObject);
        yield return null; //다음 프레임에 메서드 재게
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
        // 각 지표를 계산하여 보드의 점수를 계산
        float sumHoles = CalculateHoles(Simgrid) * -1f; // 구멍 수
        float sumHeight = CalculateSumHeight(Simgrid) * -1f; // 열 높이의 합
        float rowFlips = CalculateRowFlips(Simgrid) * -1f; // 행 내에서 셀 상태가 바뀌는 횟수
        float columnFlips = CalculateColumnFlips(Simgrid) * -1f; // 열 내에서 셀 상태가 바뀌는 횟수

        return sumHoles + sumHeight + rowFlips + columnFlips;
    }

    private bool IsRowFull(Transform[,] grid, int y)
    { //행이 꽉 찼는지 파악하는 메서드
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
    { //행이 꽉 찼을 시, 제거를 가정하는 메서드 (참조 배열이기 때문에 제거 불가)
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
    { // 구멍 수 계산 로직
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
    { // 열 높이의 합 계산 로직
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
    { // 행 내에서 셀 상태가 바뀌는 횟수 계산 로직
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
    {   //열 내에서 셀 상태가 바뀌는 횟수 계산 로직
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
    public TBoard GetPlayerBD()
    {
        return bd;
    }
    public int getPlayerType()
    {
        return PlayerType;
    }
    public void setPlayerType(int type)
    {
        PlayerType = type;
    }
}
