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
    public TBoard EnemyBoard; //상대 보드
    public GameObject GameOverPannel; //게임 오버 출력 ui
    public Text PlayerPoint; //보드 점수 ui
    private int PointSum = 0; // 보드 점수
    public GameObject TSpawnPoint; //다음 테트로 표현 위치
    public bool isghost = false; //고스트 보드일시 새 테트로 생성 x

    public override void OnEnable() //플레이어에 따른 설정 필요
    {
        if (PhotonNetwork.IsConnected)//온라인 플레이 시
        {
            if (GetComponentInParent<TPlayer>().PlayerType != 4)
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
        else if (GetComponentInParent<TPlayer>().PlayerType != -1 && !isghost)
        {
            int tetroC = Random.Range(0, 7);
            ControllTetro[0] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[0].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
            ControllTetro[0].transform.position = transform.position + new Vector3(4, 19, -0.2f);
            //Debug.Log("1");
            tetroC = Random.Range(0, 7);
            ControllTetro[1] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[1].transform.position = TSpawnPoint.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && GetComponentInParent<TPlayer>().PlayerType != 4) 
        {//온라인 플레이 중 로컬 보드에만 작동
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i)) //이거 하면 줄을 삭제해버림
                {
                    point++;
                    DestroyLine(i--); //줄을 삭제해버리면 검사하지 못하는 행이 생기므로 --
                }
            }
            if (point == 2 && EnemyBoard.enabled)
            { //2개 행 파괴 시 펜토미노 생성
                Debug.Log("두개 삭제");
                int temp = Random.Range(7, 9);
                photonView.RPC("ChangeTetro", RpcTarget.Others, temp);
            }
            if (point >= 3 && EnemyBoard.enabled)
            { //3개 이상 행 파괴 시 펜토미노 생성
                Debug.Log("세개 삭제");
                int temp = Random.Range(9, 11);
                photonView.RPC("ChangeTetro", RpcTarget.Others, temp);
            }
            
            PointSum += point * point * 10;
            PlayerPoint.text = "점수 : " + PointSum.ToString();
            //점수 업데이트
            
            if (!ControllTetro[0].GetComponent<TetroBehav>().getMovable())
            {
                newTetro();
            }
        }
        else if(PhotonNetwork.IsConnected && GetComponentInParent<TPlayer>().PlayerType == 4)
        {//온라인 플레이 중 상대 보드에만 작동
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i)) //이거 하면 줄을 삭제해버림
                {
                    Debug.Log(i + " 행 꽉참");
                    point++;
                    DestroyLine(i--); //줄을 삭제해버리면 검사하지 못하는 행이 생기므로 --
                }
            }
            PointSum += point * point * 10;
            PlayerPoint.text = "점수 : " + PointSum.ToString();
        }
        else if (!isghost) //고스트 보드는 줄 파괴, 테트로 생성 x
        {//로컬 플레이 시 작동
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i)) //이거 하면 줄을 삭제해버림
                {
                    point++;
                    DestroyLine(i--); //줄을 삭제해버리면 검사하지 못하는 행이 생기므로 --
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
            }
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
        //Debug.Log("fulled");
        return true;
    }
    void DestroyLine(int row) //최하 라인 파괴
    {
        if (PhotonNetwork.IsConnected)
        {
            for (int i = 0; i < 10; i++)
            {
                grid[i, row].gameObject.SetActive(false);
                grid[i, row] = null;
            }
            RowDown(row);
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                Destroy(grid[i, row].gameObject);
                grid[i, row] = null;
            }
            RowDown(row);
        }
    }
    void RowDown(int row) //파괴 후 테트로들 한칸 낮추기
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
        ControllTetro[0] = ControllTetro[1];
        ControllTetro[0].transform.position = transform.position + new Vector3(4, 19, -0.2f);
        if (!ControllTetro[0].GetComponent<TetroBehav>().isValidMove())
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("OnlineGameOver", RpcTarget.Others);
            }
            GameOver();
        }
        ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
        int tetroC = Random.Range(0, 7);

        if (PhotonNetwork.IsConnected)
        {
            Vector3 pos = TSpawnPoint.transform.position;
            Quaternion rot = new Quaternion(0,0,0,0);
            ControllTetro[1] = PhotonNetwork.Instantiate(tetroSpawner[tetroC].name, pos, rot);
            ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
        }
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
