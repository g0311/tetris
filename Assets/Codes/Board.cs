using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Transform[,] grid = new Transform[10, 20]; //�� ��ǥ���� Ÿ���� ������ �迭

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame

    int point = 0;
    void Update()
    {
        if (checkLineFull())
        {
            point++;
            DestroyLine();
        }
    }
    bool checkLineFull() //���� ���� üũ
    {
         for(int i = 0; i < 10; i++)
        {
            if(grid[i,0] == null)
            {
                return false;
            }
        }
        return true;
    }
    void DestroyLine() //���� ���� �ı�
    {
        for(int i = 0; i < 10; i++)
        {
            Destroy(grid[i, 0].gameObject);
            grid[i, 0] = null;
            LineDown();
        }
    }
    void LineDown() //�ı� �� ��Ʈ�ε� ��ĭ ���߱�?
    {
        for (int i = 0; i < 20; i++)
        {
            for(int j = 1; j < 10; j++)
            {

            }
        }
    }
}
