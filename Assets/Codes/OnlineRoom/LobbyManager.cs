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
    private GameObject RoomUI; //방에 접속했을때의 UI
    [SerializeField]
    private Button CreateRoomBtn; //방 만들기 버튼
    [SerializeField]
    private Button ExitBtn; //나가기 버튼
    [SerializeField]
    private GameObject CreatingUI; //방 만들기 UI
    [SerializeField]
    private GameObject RoomList; //방의 목록을 출력하는 UI
    [SerializeField]
    private GameObject RoomEntity; //방 목록에 생기는 방의 UI


    Dictionary<string, RoomInfo> dicRoomInfo = new Dictionary<string, RoomInfo>(); //방의 정보를 저장하는 딕셔너리
    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.InRoom) //o2p 플레이 후 씬으로 돌아왔을 시
        {
            gameObject.SetActive(false);
            RoomUI.SetActive(true);
        }
        PhotonNetwork.AutomaticallySyncScene = false; //씬 이동 동기화 해제
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
        PhotonNetwork.Disconnect(); //서버 접속 해제
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
        roomOption.MaxPlayers = 2; //최대 인원수 설정
        roomOption.IsOpen = true; //방이 열려있는지 닫혀있는지 설정
        roomOption.IsVisible = true; //비공개 방 여부

        // 플레이어의 정보를 설정 (ready 상태 및 ready 변경 상태)
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
            { "IsReadyChanged", true },
            { "IsReady", false }
        });
        // 방 생성
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
    { //방 목록 업데이트시 자동으로 호출되는 메서드
        base.OnRoomListUpdate(roomList);
        Debug.Log("업데이트 됨");
        UpdateRoomList(roomList);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        //현재 방 모두 삭제
        foreach (Transform child in RoomList.transform)
        {
            Destroy(child.gameObject);
        }

        //방 정보 갱신 (삭제된 룸 제거)
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
            dicRoomInfo[room.Name] = room; //삭제 되지 않은 방만 딕셔너리에 저장됨
        }

        foreach (RoomInfo room in dicRoomInfo.Values)
        {
            // 방 정보 프리팹 인스턴스를 생성
            GameObject roomEntity = Instantiate(RoomEntity, RoomList.transform);

            // 방 정보를 표시하는 레이블을 업데이트
            Text roomNameLabel = roomEntity.GetComponentInChildren<Text>();
            roomNameLabel.text = room.Name;

            // 방에 참가 버튼의 클릭 이벤트를 설정
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
        //룸 ui 키고 로비 ui 끄기
        RoomUI.SetActive(true);
        gameObject.SetActive(false);
        //룸 ui에서는 준비 및 게임 시작
    }
}
