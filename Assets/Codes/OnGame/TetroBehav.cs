using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TetroBehav : MonoBehaviour, IPunObservable
{
    private GameObject parentBoard; //부모 보드
    private Transform[,] grid; //부모 보드의 정보
    private bool movable = false; //이동 가능 판단 변수
    private float curt;
    PhotonView photonView; //동기화에 필요한 변수

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
            } //테트로미노의 모든 블록이 비활성화된 상태인지 확인
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
    //충돌 범위 설정 /떨어질때 마다 체크? 회전도?
    public bool isValidMove()
    {
        foreach (Transform children in transform)
        {
            int x = Mathf.RoundToInt(children.position.x - parentBoard.transform.position.x); // 한 타일의 x좌표
            int y = Mathf.RoundToInt(children.position.y - parentBoard.transform.position.y); // 한 타일의 y좌표
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
            // 로컬 클라이언트에서 실행되며, 게임 오브젝트의 상태를 네트워크를 통해 전송
            stream.SendNext(parentBoard.GetPhotonView().ViewID);
        }
        else
        {
            // 원격 클라이언트에서 실행되며, 네트워크에서 게임 오브젝트의 상태를 받아옴
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
        //다른 클라이언트에게(상대 플레이어 클라이언트) saveRPC함수 호출
        photonView.RPC("saveRPC", RpcTarget.Others, xValues.ToArray(), yValues.ToArray());
    }
    [PunRPC]
    void saveRPC(int[] xValues, int[] yValues)
    { //로컬 클라이언트의 상대 보드 정보 갱신
        grid = parentBoard.GetComponent<TBoard>().grid;
        for (int i = 0; i < xValues.Length; i++)
        {
            int x = xValues[i];
            int y = yValues[i];
            grid[x, y] = transform.GetChild(i);
        }
    }
}