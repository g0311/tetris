using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Transform[,] grid = new Transform[10, 20]; //각 좌표마다 타일을 저장할 배열

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
    bool checkLineFull() //최하 라인 체크
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
    void DestroyLine() //최하 라인 파괴
    {
        for(int i = 0; i < 10; i++)
        {
            Destroy(grid[i, 0].gameObject);
            grid[i, 0] = null;
            LineDown();
        }
    }
    void LineDown() //파괴 후 테트로들 한칸 낮추기?
    {
        for (int i = 0; i < 20; i++)
        {
            for(int j = 1; j < 10; j++)
            {

            }
        }
    }
}
