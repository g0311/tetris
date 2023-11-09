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
        for(int i = 0; i < 20; i++)
        {
            if (checkLineFull(i))
            {
                point++;
                DestroyLine(i);
            }
        }
    }

    bool checkLineFull(int y)
    {
         for(int i = 0; i < 10; i++)
        {
            if(grid[i,y] == null)
            {
                return false;
            }
        }
        return true;
    }
    void DestroyLine(int y)
    {
        for(int i = 0; i < 10; i++)
        {
            Destroy(grid[i, y].gameObject);
            grid[i, y] = null;
        }
    }
}
