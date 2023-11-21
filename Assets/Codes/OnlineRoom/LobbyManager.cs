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
        roomOption.MaxPlayers = 2; //최대 인원수 설정
        roomOption.IsOpen = true; //방이 열려있는지 닫혀있는지 설정
        roomOption.IsVisible = true; //비공개 방 여부

        // 준비 상태를 설정
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
            { "IsReadyChanged", true },
            { "IsReady", false }
        });
        PhotonNetwork.CreateRoom(CreatingUI.GetComponentInChildren<InputField>().text, roomOption, null);
    }
    public override void OnCreatedRoom()
    {//방 생성 완료 시 자동으로 방 접속 됨
        base.OnCreatedRoom();
        CreatingUI.SetActive(false);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError("방만들기실패" + message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        //현재 방 모두 삭제
        foreach (Transform child in RoomList.transform)
        {
            Destroy(child.gameObject);
        }
        //룸 정보 갱신 (삭제된 룸 제거)
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
            // 룸 정보 프리팹의 인스턴스를 생성합니다
            GameObject roomEntity = Instantiate(RoomEntity, RoomList.transform);

            // 룸 정보를 표시하는 레이블을 업데이트합니다.
            Text roomNameLabel = roomEntity.GetComponentInChildren<Text>();
            roomNameLabel.text = room.Name;

            // 룸에 참가하기 위한 버튼의 클릭 이벤트를 설정합니다.
            Button joinButton = roomEntity.GetComponentInChildren<Button>();
            joinButton.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(room.Name);
                // 준비 상태를 설정
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
        //로비 ui 끄고 룸 ui 키기
        gameObject.SetActive(false);
        RoomUI.SetActive(true);
        //룸 ui에서는 준비 및 게임 시작
    }
}
