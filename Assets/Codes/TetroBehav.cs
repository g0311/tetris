using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetroBehav : MonoBehaviour
{
    public GameObject parentBoard;
    private Transform[,] grid;
    public bool movable = false;
    float curt;
    // Start is called before the first frame update
    void Start()
    {
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
            //�ٵ� �̰� ��� ��ǥ�� ���� ��ǥ ���� �ƴ�? �׷��� ��ǥ�˾ƾ���
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
    } //true�Ͻ� �̵�, false�Ͻ� �̵� �ǵ�����
    public void TFall()
    {
        curt = 0;
        transform.position += new Vector3(0, -1, 0);

        if (!isValidMove())
        {
            movable = false;
            transform.position -= new Vector3(0, -1, 0);
            save();
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
}