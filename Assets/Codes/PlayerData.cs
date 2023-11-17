using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // Start is called before the first frame update
    public static PlayerData Instance { get; private set; }

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

    int _2P_Mode;
    string _1p_name;
    string _2p_name;
    public void Set2P_Mode(int mode)
    {
        _2P_Mode = mode;
    }
    public int Get2P_Mode()
    {
        return _2P_Mode;
    }
    public void SetP1Name(string pname)
    {
        _1p_name = pname;
    }
    public string GetP1Name()
    {
        return _1p_name;
    }
    public void SetP2Name(string pname)
    {
        _2p_name = pname;
    }
    public string GetP2Name()
    {
        return _2p_name;
    }
}
