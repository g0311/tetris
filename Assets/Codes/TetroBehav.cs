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
        movable = true;
        grid = parentBoard.GetComponent<Board>().grid;
    }
    float curt = 0;
    // Update is called once per frame
    void Update()
    {
        if (movable)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TMove(0);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                TMove(1);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                TRotate();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                curt = 1;
            }

            curt += Time.deltaTime;
            if (curt >= 1)
            {
                TFall();
                curt = 0;
            }
        }
    }

    //�浹 ���� ���� /�������� ���� üũ? ȸ����?
    bool isValidMove()
    {
        foreach(Transform children in transform)
        {
            int x = (int)children.position.x - (int)parentBoard.transform.position.x; //�� Ÿ���� x��ǥ
            int y = (int)children.position.y - (int)parentBoard.transform.position.y; //�� Ÿ���� y��ǥ
            //�ٵ� �̰� ��� ��ǥ�� ���� ��ǥ ���� �ƴ�? �׷��� ��ǥ�˾ƾ���
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
    } //true�Ͻ� �̵�, false�Ͻ� �̵� �ǵ�����
    void TFall() //�������� ����
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
            int x = (int)children.position.x - (int)parentBoard.transform.position.x; //�� Ÿ���� x��ǥ
            int y = (int)children.position.y - (int)parentBoard.transform.position.y; //�� Ÿ���� y��ǥ
            grid[x, y] = children;
        }
        movable = false;
    }
    void TRotate() //ȸ�� ����
    {
        transform.Rotate(new Vector3(0, 0, 90));
        if (!isValidMove())
        {
            transform.Rotate(new Vector3(0, 0, -90));
        }
    }
    void TMove(int way) //�̵� ����
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
}
