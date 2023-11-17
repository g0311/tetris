using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // Start is called before the first frame update
    public static PlayerData Instance { get; private set; }

    int _2P_Mode = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Set2P_Mode(int mode)
    {
        _2P_Mode = mode;
    }
    public int Get2P_Mode()
    {
        return _2P_Mode;
    }
}
