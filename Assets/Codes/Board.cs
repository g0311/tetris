using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Transform[,] grid = new Transform[10, 20]; //�� ��ǥ���� Ÿ���� ������ �迭
    public GameObject[] tetroSpawner;
    private GameObject[] ControllTetro = new GameObject[3]; //���� ��Ʈ�� ���� ��Ʈ�� �� ������ ������ ��Ʈ�ι̳�
    // Start is called before the first frame update
    void Start() //�÷��̾ ���� ���� �ʿ�
    {
        for (int i = 0; i < 3; i++)
        {
            int tetroC = Random.Range(0, 7);
            ControllTetro[i] = Instantiate(tetroSpawner[tetroC]);
            ControllTetro[i].GetComponent<TetroBehav>().SetMovable(false);
        }
    }

    // Update is called once per frame

    void Update()
    {
        int point = 0;
        for (int i = 0; i < 20; i++) //��� �࿡ ���ؼ� �˻� �� �� �ı�
        {
            if (checkLineFull(i))
            {
                point++;
                DestroyLine();
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
    void DestroyLine() //���� ���� �ı�
    {
        for(int i = 0; i < 10; i++)
        {
            Destroy(grid[i, 0].gameObject);
            grid[i, 0] = null;
        }
        RowDown();
    }
    void RowDown() //�ı� �� ��Ʈ�ε� ��ĭ ���߱�
    {
        for (int y = 1; y < 20; y++)
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
}
