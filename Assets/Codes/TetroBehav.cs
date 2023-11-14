using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetroBehav : MonoBehaviour
{
    public GameObject parentBoard;
    private Transform[,] grid;
    private bool movable;
    // Start is called before the first frame update
    void Start()
    {
       grid = parentBoard.GetComponent<TBoard>().grid;
    }
    float curt = 0;
    // Update is called once per frame
    void Update()
    {
        if (movable)
        {
            curt += Time.deltaTime;
            if (curt > 1)
            {
                TFall();
                curt = 0;
            }
        }
    }

    //충돌 범위 설정 /떨어질때 마다 체크? 회전도?
    bool isValidMove()
    {
        foreach(Transform children in transform)
        {
            int x = (int)children.position.x - (int)parentBoard.transform.position.x; //한 타일의 x좌표
            int y = (int)children.position.y - (int)parentBoard.transform.position.y; //한 타일의 y좌표
            //근데 이거 얘네 좌표가 절대 좌표 기준 아님? 그러면 좌표알아야함
            if(x < 0 || x >= 10 || y < 0 || y >= 20)
            {
                return false;
            }

            if(grid[x,y] != null)
            {
                return false;
            }
        }
        return true;
    } //true일시 이동, false일시 이동 되돌리기
    void TFall() //떨어지게 설정
    {
        transform.position += new Vector3(0, -1, 0);
        if (!isValidMove())
        {
            transform.position -= new Vector3(0, -1, 0);
            save();
        }
    }
    void save()
    {
        foreach (Transform children in transform)
        {
            int x = (int)children.position.x - (int)parentBoard.transform.position.x; //한 타일의 x좌표
            int y = (int)children.position.y - (int)parentBoard.transform.position.y; //한 타일의 y좌표
            grid[x, y] = children;
        }
        movable = false;
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
    }
}
