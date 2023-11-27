using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TBoard : MonoBehaviourPunCallbacks
{
    public Transform[,] grid = new Transform[10, 22]; //각 좌표의 타일을 저장할 배열
    [SerializeField]
    private GameObject[] tetroSpawner; //테트로미노 배열
    private GameObject[] ControllTetro = new GameObject[2]; //현재 컨트롤 중인 테트로 및 다음에 생성될 테트로미노
    [SerializeField]
    private TBoard EnemyBoard; //상대 보드
    [SerializeField]
    private GameObject GameOverPannel; //게임 오버 출력 ui
    [SerializeField]
    private Text PlayerPoint; //보드 점수 ui
    private int PointSum = 0; // 보드 점수
    [SerializeField]
    private GameObject TSpawnPoint; //다음 테트로 표현 위치
    private bool isghost = false; //고스트 보드일시 새 테트로 생성 x

    public override void OnEnable() //컴포넌트(TBaord.cs) 활성화 시 호출
    { //초기 테트로미노 생성
        if (PhotonNetwork.IsConnected)
        { //온라인 플레이 시 서버 동기화 생성
            if (GetComponentInParent<TPlayer>().getPlayerType() != 4)
            {
                Vector3 pos = transform.position + new Vector3(4, 19, -0.2f);
                Quaternion rot = new Quaternion(0, 0, 0, 0);

                int tetroC = Random.Range(0, 7);
                ControllTetro[0] = PhotonNetwork.Instantiate(tetroSpawner[tetroC].name, pos, rot);
                ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
                ControllTetro[0].GetComponent<TetroBehav>().setParentBoard(gameObject);

                tetroC = Random.Range(0, 7);
                pos = TSpawnPoint.transform.position;
                ControllTetro[1] = PhotonNetwork.Instantiate(tetroSpawner[tetroC].name, pos, rot);
                ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
            }
        }
        else if (GetComponentInParent<TPlayer>().getPlayerType() != -1 && !isghost)
        { //로컬 플레이 시
            int tetroC = Random.Range(0, 7);
            ControllTetro[0] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[0].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
            ControllTetro[0].transform.position = transform.position + new Vector3(4, 19, -0.2f);
            tetroC = Random.Range(0, 7);
            ControllTetro[1] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[1].transform.position = TSpawnPoint.transform.position;
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && GetComponentInParent<TPlayer>().getPlayerType() != 4)
        {//온라인 플레이 중 로컬 플레이어
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i))
                {
                    point++;
                    DestroyLine(i--);
                }
            }
            if (point == 2 && EnemyBoard.enabled)
            { //2개 행 파괴 시 상대 플레이어에 펜토미노 생성
                int temp = Random.Range(7, 9);
                photonView.RPC("ChangeTetro", RpcTarget.Others, temp);
            }
            if (point >= 3 && EnemyBoard.enabled)
            { //3개 이상 행 파괴 시 상대 플레이어에 펜토미노 생성
                int temp = Random.Range(9, 11);
                photonView.RPC("ChangeTetro", RpcTarget.Others, temp);
            }

            PointSum += point * point * 10;
            PlayerPoint.text = "점수 : " + PointSum.ToString();
            //점수 업데이트

            if (!ControllTetro[0].GetComponent<TetroBehav>().getMovable())
            {
                newTetro();
            } //현재 테트로미노가 이동이 불가능하다면 새로운 테트로미노 생성
        }
        else if (PhotonNetwork.IsConnected && GetComponentInParent<TPlayer>().getPlayerType() == 4)
        {//온라인 플레이 중 상대 플레이어
         //온라인 상대 플레이어 보드는 새로운 테트로미노 생성 없이 파괴만 수행
            if (PhotonNetwork.PlayerList.Length != 2)
            {//플레이어가 나갔을 시 작동
                GameOver();
            }
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i))
                {
                    point++;
                    DestroyLine(i--);
                }
            }
            PointSum += point * point * 10; //한번에 제거한 수의 제곱 * 10점 추가
            PlayerPoint.text = "점수 : " + PointSum.ToString();
        }
        else if (!isghost) //고스트 보드는 줄 파괴, 테트로 생성 x
        {//로컬 플레이
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i))
                {
                    point++;
                    DestroyLine(i--);
                }
            }
            if (point == 2 && EnemyBoard.enabled)
            { //2개 행 파괴 시 펜토미노 생성
                int temp = Random.Range(7, 9);
                GameObject[] EnemyTetros = EnemyBoard.getCurTetro();
                Destroy(EnemyTetros[1].gameObject);
                EnemyTetros[1] = Instantiate(tetroSpawner[temp]);
                EnemyTetros[1].GetComponent<TetroBehav>().setParentBoard(EnemyBoard.gameObject);
                EnemyTetros[1].GetComponent<TetroBehav>().setMovable(false);
                EnemyTetros[1].transform.position = EnemyBoard.TSpawnPoint.transform.position;
            }
            if (point >= 3 && EnemyBoard.enabled)
            { //3개 이상 행 파괴 시 펜토미노 생성
                int temp = Random.Range(9, 11);
                GameObject[] EnemyTetros = EnemyBoard.getCurTetro();
                Destroy(EnemyTetros[1].gameObject);
                EnemyTetros[1] = Instantiate(tetroSpawner[temp]);
                EnemyTetros[1].GetComponent<TetroBehav>().setParentBoard(EnemyBoard.gameObject);
                EnemyTetros[1].GetComponent<TetroBehav>().setMovable(false);
                EnemyTetros[1].transform.position = EnemyBoard.TSpawnPoint.transform.position;
            }

            PointSum += point * point * 10;
            PlayerPoint.text = "점수 : " + PointSum.ToString();
            //점수 업데이트

            if (!ControllTetro[0].GetComponent<TetroBehav>().getMovable())
            {
                newTetro();
            }//현재 테트로미노가 이동이 불가능하다면 새로운 테트로미노 생성
        }
    }
    bool checkLineFull(int y) //입력 받은 행 체크
    {
         for(int x = 0; x < 10; x++)
         {
            if(grid[x,y] == null)
            {
                return false;
            }
         }
        return true;
    }
    void DestroyLine(int row) //입력 받은 행 비활성화 및 행 내리기
    {
        for (int i = 0; i < 10; i++)
        {
            grid[i, row].gameObject.SetActive(false);
            grid[i, row] = null;
        }
        RowDown(row);
    }
    void RowDown(int row) //입력 받은 행 상위 행들 한칸 낮추기
    {
        for (int y = row + 1; y < 20; y++)
        {
            for(int x = 0; x < 10; x++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y - 1].position += new Vector3(0, -1, 0);
                }
                grid[x, y] = null;
            }
        }
    }

    private void newTetro()
    {
        ControllTetro[0] = ControllTetro[1]; //다음 테트로가 현재 테트로가 됨
        ControllTetro[0].transform.position = transform.position + new Vector3(4, 19, -0.2f); //위치 설정
        if (!ControllTetro[0].GetComponent<TetroBehav>().isValidMove())
        { //테트로 스폰 위치 이동 후 이동 불가 판정 시 게임 오버
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("OnlineGameOver", RpcTarget.Others);
            } //온라인 상태일시 상대 클라이언트에 게임오버 정보 전달
            GameOver();
        }
        ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
        //현재 테트로미노 이동 시작
        int tetroC = Random.Range(0, 7);
        // 난수를 통해 랜덤 테트로 생성
        if (PhotonNetwork.IsConnected)
        {
            Vector3 pos = TSpawnPoint.transform.position;
            Quaternion rot = new Quaternion(0,0,0,0);
            ControllTetro[1] = PhotonNetwork.Instantiate(tetroSpawner[tetroC].name, pos, rot);
            ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
        } //온라인 상태일시 서버 동기화 생성
        else
        {
            ControllTetro[1] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[1].transform.position = TSpawnPoint.transform.position;
        }
    }

    public GameObject[] getCurTetro()
    {
        return ControllTetro;
    }
    public void setisGhost(bool isg)
    {
        isghost = isg;
    }

    public void GameOver()
    {
        GameOverPannel.SetActive(true);
        enabled = false;
    }

    [PunRPC]
    void ChangeTetro(int ind)
    {//상대 테트로 변경 전달 함수
        GameObject[] EnemyTetros = EnemyBoard.getCurTetro();
        PhotonNetwork.Destroy(EnemyTetros[1].gameObject);
        EnemyTetros[1] = PhotonNetwork.Instantiate(tetroSpawner[ind].name, EnemyBoard.TSpawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
        EnemyTetros[1].GetComponent<TetroBehav>().setParentBoard(EnemyBoard.gameObject);
    }

    [PunRPC]
    void OnlineGameOver()
    {//게임 종료 상태 전달 함수
        GameOverPannel.SetActive(true);
        enabled = false;
    }
}
