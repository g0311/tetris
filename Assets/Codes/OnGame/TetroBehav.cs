using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TetroBehav : MonoBehaviour, IPunObservable
{
    public GameObject parentBoard;
    private Transform[,] grid;
    public bool movable = false;
    float curt;
    PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        curt = 0;
    }
    // Update is called once per frame
    void Update()
    {
        bool x = true;
        foreach (Transform children in transform)
        {
            x = x && !children.gameObject.activeSelf;
        }

        if (PhotonNetwork.IsConnected)
        {
            if (movable)
            {
                curt += Time.deltaTime;
                if (curt > 1)
                {
                    TFall();
                }
            }
        }
        else if (movable)
        {
            if (x)
            {
                Destroy(gameObject);
            }
            curt += Time.deltaTime;
            if (curt > 1)
            {
                TFall();
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
    //충돌 범위 설정 /떨어질때 마다 체크? 회전도?
    public bool isValidMove()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.position.x - parentBoard.transform.position.x); // 한 타일의 x좌표
            int y = Mathf.RoundToInt(children.position.y - parentBoard.transform.position.y); // 한 타일의 y좌표
            //근데 이거 얘네 좌표가 절대 좌표 기준 아님? 그러면 좌표알아야함
            if (x < 0 || x >= 10 || y < 0 || y >= 22)
            {
                return false;
            }
            if (grid[x, y] != null)
            {
                //Debug.Log(x + " error " + y);
                return false;
            }
        }
        return true;
    } //true일시 이동, false일시 이동 되돌리기
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
                Debug.Log("포톤세이브");
                PhotonSave();
            }
            else
            {
                save();
            }
        }
    }
    void save()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.FloorToInt(children.position.x - parentBoard.transform.position.x);
            int y = Mathf.FloorToInt(children.position.y - parentBoard.transform.position.y);
            //Debug.Log(x + " save " + y);
            grid[x, y] = children;
        }
    }
    public void TRotate() //회전 구현
    {
        transform.Rotate(new Vector3(0, 0, 90));
        if (!isValidMove())
        {
            transform.Rotate(new Vector3(0, 0, -90));
        }
    }
    public void TMove(int way) //이동 구현
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
    public void setCurt(int ct)
    {
        curt = ct;
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
            //Debug.Log(x + " save " + y);
            grid[x, y] = null;
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 클라이언트에서 실행되며, 게임 오브젝트의 상태를 네트워크를 통해 전송합니다.
            stream.SendNext(parentBoard.GetPhotonView().ViewID);
        }
        else
        {
            // 원격 클라이언트에서 실행되며, 네트워크에서 게임 오브젝트의 상태를 받아옵니다.
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
        photonView.RPC("saveRPC", RpcTarget.Others, xValues.ToArray(), yValues.ToArray());
    }
    [PunRPC]
    void saveRPC(int[] xValues, int[] yValues)
    {
        grid = parentBoard.GetComponent<TBoard>().grid;
        for (int i = 0; i < xValues.Length; i++)
        {
            int x = xValues[i];
            int y = yValues[i];
            grid[x, y] = transform.GetChild(i);
        }
    }
}