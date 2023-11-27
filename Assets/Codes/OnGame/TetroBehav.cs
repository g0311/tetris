using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TetroBehav : MonoBehaviour, IPunObservable
{
    private GameObject parentBoard; //�θ� ����
    private Transform[,] grid; //�θ� ������ ����
    private bool movable = false; //�̵� ���� �Ǵ� ����
    private float curt;
    PhotonView photonView; //����ȭ�� �ʿ��� ����

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        curt = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (movable)    
        {
            curt += Time.deltaTime;
            if (curt > 1)
            {
                TFall();
            }
        }
        else
        {
            bool isDestroyed = true;
            foreach (Transform children in transform)
            {
                isDestroyed = isDestroyed && !children.gameObject.activeSelf;
            } //��Ʈ�ι̳��� ��� ����� ��Ȱ��ȭ�� �������� Ȯ��
            if (isDestroyed)
            {
                Destroy(gameObject);
            }
        }
    }
    public void MovetBottom()
    {
        while (movable)
        {
            TFall();
        }
    }
    //�浹 ���� ���� /�������� ���� üũ? ȸ����?
    public bool isValidMove()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.position.x - parentBoard.transform.position.x); // �� Ÿ���� x��ǥ
            int y = Mathf.RoundToInt(children.position.y - parentBoard.transform.position.y); // �� Ÿ���� y��ǥ
            if (x < 0 || x >= 10 || y < 0 || y >= 22)
            {
                return false;
            }
            if (grid[x, y] != null)
            {
                return false;
            }
        }
        return true;
    } //true�Ͻ� �̵�, false�Ͻ� �̵� �ǵ�����
    public void TFall()
    {
        curt = 0;
        transform.position += new Vector3(0, -1, 0);

        if (!isValidMove())
        {
            movable = false;
            transform.position -= new Vector3(0, -1, 0);
            if (PhotonNetwork.IsConnected)
            {
                PhotonSave();
            }
            else
            {
                save();
            }
        }
    }
    private void save()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.FloorToInt(children.position.x - parentBoard.transform.position.x);
            int y = Mathf.FloorToInt(children.position.y - parentBoard.transform.position.y);
            grid[x, y] = children;
        }
    }
    public void TRotate() //ȸ�� ����
    {
        transform.Rotate(new Vector3(0, 0, 90));
        if (!isValidMove())
        {
            transform.Rotate(new Vector3(0, 0, -90));
        }
    }
    public void TMove(int way) //�̵� ����
    {
        if (way == 1)
        {
            transform.position += new Vector3(1, 0, 0);
            if (!isValidMove())
            {
                transform.position -= new Vector3(1, 0, 0);
            }
        }
        else
        {
            transform.position -= new Vector3(1, 0, 0);
            if (!isValidMove())
            {
                transform.position += new Vector3(1, 0, 0);
            }
        }
    }
    public void setMovable(bool tf)
    {
        movable = tf;
    }
    public bool getMovable()
    {
        return movable;
    }
    public void setParentBoard(GameObject bd)
    {
        parentBoard = bd;
        grid = parentBoard.GetComponent<TBoard>().grid;
    }
    public void DSave()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.FloorToInt(children.position.x - parentBoard.transform.position.x);
            int y = Mathf.FloorToInt(children.position.y - parentBoard.transform.position.y);
            grid[x, y] = null;
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ���� Ŭ���̾�Ʈ���� ����Ǹ�, ���� ������Ʈ�� ���¸� ��Ʈ��ũ�� ���� ����
            stream.SendNext(parentBoard.GetPhotonView().ViewID);
        }
        else
        {
            // ���� Ŭ���̾�Ʈ���� ����Ǹ�, ��Ʈ��ũ���� ���� ������Ʈ�� ���¸� �޾ƿ�
            int parentBoardID = (int)stream.ReceiveNext();
            parentBoard = PhotonView.Find(parentBoardID).gameObject;
        }
    }
    void PhotonSave()
    {
        List<int> xValues = new List<int>();
        List<int> yValues = new List<int>();

        foreach (Transform children in transform)
        {
            int x = Mathf.FloorToInt(children.position.x - parentBoard.transform.position.x);
            int y = Mathf.FloorToInt(children.position.y - parentBoard.transform.position.y);
            grid[x, y] = children;

            xValues.Add(x);
            yValues.Add(y);
        }
        //�ٸ� Ŭ���̾�Ʈ����(��� �÷��̾� Ŭ���̾�Ʈ) saveRPC�Լ� ȣ��
        photonView.RPC("saveRPC", RpcTarget.Others, xValues.ToArray(), yValues.ToArray());
    }
    [PunRPC]
    void saveRPC(int[] xValues, int[] yValues)
    { //���� Ŭ���̾�Ʈ�� ��� ���� ���� ����
        grid = parentBoard.GetComponent<TBoard>().grid;
        for (int i = 0; i < xValues.Length; i++)
        {
            int x = xValues[i];
            int y = yValues[i];
            grid[x, y] = transform.GetChild(i);
        }
    }
}