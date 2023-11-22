using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TBoard : MonoBehaviourPunCallbacks
{
    public Transform[,] grid = new Transform[10, 22]; //�� ��ǥ�� Ÿ���� ������ �迭
    [SerializeField]
    private GameObject[] tetroSpawner; //��Ʈ�ι̳� �迭
    private GameObject[] ControllTetro = new GameObject[2]; //���� ��Ʈ�� ���� ��Ʈ�� �� ������ ������ ��Ʈ�ι̳�
    public TBoard EnemyBoard; //��� ����
    public GameObject GameOverPannel; //���� ���� ��� ui
    public Text PlayerPoint; //���� ���� ui
    private int PointSum = 0; // ���� ����
    public GameObject TSpawnPoint; //���� ��Ʈ�� ǥ�� ��ġ
    public bool isghost = false; //��Ʈ �����Ͻ� �� ��Ʈ�� ���� x

    public override void OnEnable() //�÷��̾ ���� ���� �ʿ�
    {
        if (PhotonNetwork.IsConnected)//�¶��� �÷��� ��
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
        {//�¶��� �÷��� �� ���� ���忡�� �۵�
            int point = 0;
            for (int i = 0; i < 20; i++) //��� �࿡ ���ؼ� �˻� �� �� �ı�
            {
                if (checkLineFull(i)) //�̰� �ϸ� ���� �����ع���
                {
                    point++;
                    DestroyLine(i--); //���� �����ع����� �˻����� ���ϴ� ���� ����Ƿ� --
                }
            }
            if (point == 2 && EnemyBoard.enabled)
            { //2�� �� �ı� �� ����̳� ����
                Debug.Log("�ΰ� ����");
                int temp = Random.Range(7, 9);
                photonView.RPC("ChangeTetro", RpcTarget.Others, temp);
            }
            if (point >= 3 && EnemyBoard.enabled)
            { //3�� �̻� �� �ı� �� ����̳� ����
                Debug.Log("���� ����");
                int temp = Random.Range(9, 11);
                photonView.RPC("ChangeTetro", RpcTarget.Others, temp);
            }
            
            PointSum += point * point * 10;
            PlayerPoint.text = "���� : " + PointSum.ToString();
            //���� ������Ʈ
            
            if (!ControllTetro[0].GetComponent<TetroBehav>().getMovable())
            {
                newTetro();
            }
        }
        else if(PhotonNetwork.IsConnected && GetComponentInParent<TPlayer>().PlayerType == 4)
        {//�¶��� �÷��� �� ��� ���忡�� �۵�
            int point = 0;
            for (int i = 0; i < 20; i++) //��� �࿡ ���ؼ� �˻� �� �� �ı�
            {
                if (checkLineFull(i)) //�̰� �ϸ� ���� �����ع���
                {
                    Debug.Log(i + " �� ����");
                    point++;
                    DestroyLine(i--); //���� �����ع����� �˻����� ���ϴ� ���� ����Ƿ� --
                }
            }
            PointSum += point * point * 10;
            PlayerPoint.text = "���� : " + PointSum.ToString();
        }
        else if (!isghost) //��Ʈ ����� �� �ı�, ��Ʈ�� ���� x
        {//���� �÷��� �� �۵�
            int point = 0;
            for (int i = 0; i < 20; i++) //��� �࿡ ���ؼ� �˻� �� �� �ı�
            {
                if (checkLineFull(i)) //�̰� �ϸ� ���� �����ع���
                {
                    point++;
                    DestroyLine(i--); //���� �����ع����� �˻����� ���ϴ� ���� ����Ƿ� --
                }
            }
            if (point == 2 && EnemyBoard.enabled)
            { //2�� �� �ı� �� ����̳� ����
                int temp = Random.Range(7, 9);
                GameObject[] EnemyTetros = EnemyBoard.getCurTetro();
                Destroy(EnemyTetros[1].gameObject);
                EnemyTetros[1] = Instantiate(tetroSpawner[temp]);
                EnemyTetros[1].GetComponent<TetroBehav>().setParentBoard(EnemyBoard.gameObject);
                EnemyTetros[1].GetComponent<TetroBehav>().setMovable(false);
                EnemyTetros[1].transform.position = EnemyBoard.TSpawnPoint.transform.position;
            }
            if (point >= 3 && EnemyBoard.enabled)
            { //3�� �̻� �� �ı� �� ����̳� ����
                int temp = Random.Range(9, 11);
                GameObject[] EnemyTetros = EnemyBoard.getCurTetro();
                Destroy(EnemyTetros[1].gameObject);
                EnemyTetros[1] = Instantiate(tetroSpawner[temp]);
                EnemyTetros[1].GetComponent<TetroBehav>().setParentBoard(EnemyBoard.gameObject);
                EnemyTetros[1].GetComponent<TetroBehav>().setMovable(false);
                EnemyTetros[1].transform.position = EnemyBoard.TSpawnPoint.transform.position;
            }

            PointSum += point * point * 10;
            PlayerPoint.text = "���� : " + PointSum.ToString();
            //���� ������Ʈ

            if (!ControllTetro[0].GetComponent<TetroBehav>().getMovable())
            {
                newTetro();
            }
        }
    }
    bool checkLineFull(int y) //�Է� ���� �� üũ
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
    void DestroyLine(int row) //���� ���� �ı�
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
    void RowDown(int row) //�ı� �� ��Ʈ�ε� ��ĭ ���߱�
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
    {//��� ��Ʈ�� ���� ���� �Լ�
        GameObject[] EnemyTetros = EnemyBoard.getCurTetro();
        PhotonNetwork.Destroy(EnemyTetros[1].gameObject);
        EnemyTetros[1] = PhotonNetwork.Instantiate(tetroSpawner[ind].name, EnemyBoard.TSpawnPoint.transform.position, new Quaternion(0, 0, 0, 0));
        EnemyTetros[1].GetComponent<TetroBehav>().setParentBoard(EnemyBoard.gameObject);
    }
    [PunRPC]
    void OnlineGameOver()
    {//���� ���� ���� ���� �Լ�
        GameOverPannel.SetActive(true);
        enabled = false;
    }
}
