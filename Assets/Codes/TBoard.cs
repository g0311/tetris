using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TBoard : MonoBehaviour
{
    public Transform[,] grid = new Transform[10, 20]; //각 좌표마다 타일을 저장할 배열
    public GameObject[] tetroSpawner;
    private GameObject[] ControllTetro = new GameObject[2]; //현재 컨트롤 중인 테트로 및 다음에 생성될 테트로미노
    public GameObject GameOverPannel;
    private bool isGameOver;
    public bool isghost = false; //고스트 보드일시 새 테트로 생성 x
    // Start is called before the first frame update
    void Start() //플레이어에 따른 설정 필요
    {
        GameOverPannel.SetActive(false);
        if (GetComponentInParent<TPlayer>().PlayerType != -1 && !isghost)
        {
            int tetroC = Random.Range(0, 7);
            ControllTetro[0] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[0].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
            ControllTetro[0].transform.position = transform.position + new Vector3(4, 19, -0.2f);
            Debug.Log("1");
            tetroC = Random.Range(0, 7);
            ControllTetro[1] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
            ControllTetro[1].transform.position = transform.position + new Vector3(14, 19, -0.2f);
        }
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver) {
            int point = 0;
            for (int i = 0; i < 20; i++) //모든 행에 대해서 검사 후 행 파괴
            {
                if (checkLineFull(i))
                {
                    point++;
                    DestroyLine(i);
                }
            }

            if (!isghost && !ControllTetro[0].GetComponent<TetroBehav>().getMovable())
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
        for(int i = 0; i < 10; i++)
        {
            Destroy(grid[i, row].gameObject);
            grid[i, row] = null;
        }
        RowDown(row);
    }
    void RowDown(int row) //파괴 후 테트로들 한칸 낮추기
    {
        for (int y = row + 1; y < 20; y++)
        {
            for(int x = 0; x < 10; x++)
            {
                if(grid[x,y] != null)
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
        ControllTetro[0].GetComponent<TetroBehav>().setMovable(true);
        if (!ControllTetro[0].GetComponent<TetroBehav>().isValidMove())
        {
            GameOver();
        }
        int tetroC = Random.Range(0, 7);
        ControllTetro[1] = Instantiate(tetroSpawner[tetroC]);
        ControllTetro[1].GetComponent<TetroBehav>().setParentBoard(gameObject);
        ControllTetro[1].GetComponent<TetroBehav>().setMovable(false);
        ControllTetro[1].transform.position = transform.position + new Vector3(14, 19, -0.2f);
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
}
