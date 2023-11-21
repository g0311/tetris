using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button CreateRoomBtn;
    [SerializeField]
    private Button ExitBtn;
    [SerializeField]
    private GameObject CreatingUI;
    [SerializeField]
    private GameObject RoomUI;
    [SerializeField]
    private GameObject RoomList;
    [SerializeField]
    private GameObject RoomEntity;


    Dictionary<string, RoomInfo> dicRoomInfo = new Dictionary<string, RoomInfo>();
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        CreateRoomBtn.onClick.AddListener(CreateRoomBtnListener);
        ExitBtn.onClick.AddListener(ExitBtnListener);
        CreatingUI.GetComponentInChildren<Button>().onClick.AddListener(CreateBtnListener);
    }

    void CreateRoomBtnListener()
    {
        CreatingUI.SetActive(true);
    }
    void ExitBtnListener()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SceneManager.LoadScene("StartMenu");
    }

    void CreateBtnListener()
    {
        RoomOptions roomOption = new RoomOptions();
        roomOption.MaxPlayers = 2; //�ִ� �ο��� ����
        roomOption.IsOpen = true; //���� �����ִ��� �����ִ��� ����
        roomOption.IsVisible = true; //����� �� ����

        // �غ� ���¸� ����
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
            { "IsReadyChanged", true },
            { "IsReady", false }
        });
        PhotonNetwork.CreateRoom(CreatingUI.GetComponentInChildren<InputField>().text, roomOption, null);
    }
    public override void OnCreatedRoom()
    {//�� ���� �Ϸ� �� �ڵ����� �� ���� ��
        base.OnCreatedRoom();
        CreatingUI.SetActive(false);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError("�游������" + message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        //���� �� ��� ����
        foreach (Transform child in RoomList.transform)
        {
            Destroy(child.gameObject);
        }
        //�� ���� ���� (������ �� ����)
        foreach (RoomInfo room in roomList)
        {
            if (dicRoomInfo.ContainsKey(room.Name))
            {
                if (room.RemovedFromList)
                {
                    dicRoomInfo.Remove(room.Name);
                    continue;
                }
            }
            dicRoomInfo[room.Name] = room;
        }
        foreach (RoomInfo room in dicRoomInfo.Values)
        {
            // �� ���� �������� �ν��Ͻ��� �����մϴ�
            GameObject roomEntity = Instantiate(RoomEntity, RoomList.transform);

            // �� ������ ǥ���ϴ� ���̺��� ������Ʈ�մϴ�.
            Text roomNameLabel = roomEntity.GetComponentInChildren<Text>();
            roomNameLabel.text = room.Name;

            // �뿡 �����ϱ� ���� ��ư�� Ŭ�� �̺�Ʈ�� �����մϴ�.
            Button joinButton = roomEntity.GetComponentInChildren<Button>();
            joinButton.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(room.Name);
                // �غ� ���¸� ����
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
                    { "IsReadyChanged", true },
                    { "IsReady", false }
                });
            });
        }
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        //�κ� ui ���� �� ui Ű��
        gameObject.SetActive(false);
        RoomUI.SetActive(true);
        //�� ui������ �غ� �� ���� ����
    }
}
