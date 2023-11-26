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
    private GameObject RoomUI; //�濡 ������������ UI
    [SerializeField]
    private Button CreateRoomBtn; //�� ����� ��ư
    [SerializeField]
    private Button ExitBtn; //������ ��ư
    [SerializeField]
    private GameObject CreatingUI; //�� ����� UI
    [SerializeField]
    private GameObject RoomList; //���� ����� ����ϴ� UI
    [SerializeField]
    private GameObject RoomEntity; //�� ��Ͽ� ����� ���� UI


    Dictionary<string, RoomInfo> dicRoomInfo = new Dictionary<string, RoomInfo>(); //���� ������ �����ϴ� ��ųʸ�
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.InRoom) //o2p �÷��� �� ������ ���ƿ��� ��
        {
            gameObject.SetActive(false);
            RoomUI.SetActive(true);
        }
        PhotonNetwork.AutomaticallySyncScene = false; //�� �̵� ����ȭ ����
        CreateRoomBtn.onClick.AddListener(CreateRoomBtnListener);
        ExitBtn.onClick.AddListener(ExitBtnListener);
        Button[] CreatingBtns = CreatingUI.GetComponentsInChildren<Button>();
        CreatingBtns[0].onClick.AddListener(CreateBtnListener);
        CreatingBtns[1].onClick.AddListener(CreateCancelBtnListener);
    }
    void CreateRoomBtnListener()
    {
        CreatingUI.SetActive(true);
    }
    void ExitBtnListener()
    {
        PhotonNetwork.Disconnect(); //���� ���� ����
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SceneManager.LoadScene("StartMenu");
    }

    void CreateCancelBtnListener()
    {
        CreatingUI.GetComponentInChildren<InputField>().text = "";
        CreatingUI.SetActive(false);
    }
    void CreateBtnListener()
    {
        RoomOptions roomOption = new RoomOptions();
        roomOption.MaxPlayers = 2; //�ִ� �ο��� ����
        roomOption.IsOpen = true; //���� �����ִ��� �����ִ��� ����
        roomOption.IsVisible = true; //����� �� ����

        // �÷��̾��� ������ ���� (ready ���� �� ready ���� ����)
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
            { "IsReadyChanged", true },
            { "IsReady", false }
        });
        // �� ����
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
    { //�� ��� ������Ʈ�� �ڵ����� ȣ��Ǵ� �޼���
        base.OnRoomListUpdate(roomList);
        Debug.Log("������Ʈ ��");
        UpdateRoomList(roomList);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        //���� �� ��� ����
        foreach (Transform child in RoomList.transform)
        {
            Destroy(child.gameObject);
        }

        //�� ���� ���� (������ �� ����)
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                if (dicRoomInfo.ContainsKey(room.Name))
                {
                    dicRoomInfo.Remove(room.Name);
                }
                continue;
            }
            dicRoomInfo[room.Name] = room; //���� ���� ���� �游 ��ųʸ��� �����
        }

        foreach (RoomInfo room in dicRoomInfo.Values)
        {
            // �� ���� ������ �ν��Ͻ��� ����
            GameObject roomEntity = Instantiate(RoomEntity, RoomList.transform);

            // �� ������ ǥ���ϴ� ���̺��� ������Ʈ
            Text roomNameLabel = roomEntity.GetComponentInChildren<Text>();
            roomNameLabel.text = room.Name;

            // �濡 ���� ��ư�� Ŭ�� �̺�Ʈ�� ����
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
        //�� ui Ű�� �κ� ui ����
        RoomUI.SetActive(true);
        gameObject.SetActive(false);
        //�� ui������ �غ� �� ���� ����
    }
}
