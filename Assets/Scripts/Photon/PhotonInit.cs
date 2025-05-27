using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//(UI 버전에서 사용)
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PhotonInit : MonoBehaviour
{
    public string initialLevel = "";
    //App의 버전 정보 (번들 버전과 일치 시키자...)
    public string version = "Ver 0.1.0";
    public PhotonLogLevel LogLevel = PhotonLogLevel.Full;

    //플레이어의 이름을 입력하는 UI 항목 연결을 위한 레퍼런스 (using UnityEngine.UI 추가해야함) (UI 버전에서 사용)
    public TMP_InputField userId;
    //룸 이름을 입력받을 UI 항목 연결 레퍼런스 (UI 버전에서 사용)
    public TMP_InputField roomName;
    //RoomItem이 차일드로 생성될 Parent 객체의 레퍼런스 (UI 버전에서 사용)
    public GameObject scrollContents;
    //UserItem이 차일드로 생성될 Parent 객체의 레퍼런스 
    public Transform userList;
    //룸 목록만큼 생성될 RoomItem 프리팹 연결 레퍼런스 (UI 버전에서 사용)
    public GameObject roomItem;
    //유저 목록만큼 생성될 UserItem 프리팹 연결 레퍼런스
    public GameObject userItem;

    //플레어의 생성 위치 저장 레퍼런스
    public Transform playerPos;
    public GameObject roomPanel;
    public GameObject UserPanel;
    void Awake()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(version);
            PhotonNetwork.logLevel = LogLevel;
            PhotonNetwork.playerName = "GUEST " + Random.Range(1, 9999);
        }

        //룸 이름을 무작위로 설정 (UI 버전에서 사용)
        roomName.text = "ROOM_" + Random.Range(0, 999).ToString("000");

        // ScrollContents의 Pivot 좌표를 Top, Left로 설정 하자. (UI 버전에서 사용)
        scrollContents.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);
        PhotonNetwork.automaticallySyncScene = true;
    }
    void Start()
    {
        
    }

    void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby !!!");
        userId.text = GetUserId();
    }

    //로컬에 저장된 플레이어 이름을 반환하거나 랜덤 생성하는 함수 (UI 버전에서 사용)
    string GetUserId()
    {
        string userId = PlayerPrefs.GetString("USER_ID");

        //유저 아이디가 NULL일 경우 랜덤 아이디 생성 
        if(string.IsNullOrEmpty(userId))
        {
            // 자릿수 맞춰서 반환
            userId = "USER_" + Random.Range(0, 999).ToString("000");
        }
        return userId;
    }

    void OnPhotonRandomJoinFailed()
    {
        //랜텀 매치 메이킹이 실패한 후 Console 뷰에 나타나는 메시지 설정
        Debug.Log("No Rooms !!!");
        CreateRoom();
    }

    void CreateRoom()
    {
        string _roomName = roomName.text;
        if (string.IsNullOrEmpty(roomName.text))
        {
            // 자릿수 맞춰서 반환
            _roomName = userId.text + "의 방";
        }

        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = userId.text;

        //생성할 룸의 조건 설정 1
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
       

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["isReady"] = false;
        PhotonNetwork.player.SetCustomProperties(props);
    }

    public void OnClickCreateRoom()
    {
        CreateRoom();
        roomPanel.SetActive(false);
    }
    void OnCreatedRoom()
    {
        int rand = Random.Range(0, 2);
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["GunRandom"] = rand;
        PhotonNetwork.room.SetCustomProperties(props);
        Debug.Log("Random " +  (int)PhotonNetwork.room.CustomProperties["GunRandom"] + " / " + rand);
    }
    void OnJoinedRoom()
    {
        Debug.Log("Enter Room");
        UserPanel.SetActive(true);
        CreateUserList();
    }

    void CreateUserList()
    {
        foreach (Transform child in userList)
        {
            Destroy(child.gameObject);
        }

        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            GameObject newUserItem = Instantiate(userItem, userList);
            UserData userData = newUserItem.GetComponent<UserData>();

            if(userData != null)
            {
                userData.SetUserID(player);
            }
        }
    }
    IEnumerator LoadStage()
    {
        //씬을 전환하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
        //(Instantiate, RPC 메시지 그리고 모든 네트워크 이벤트를 안받음 )
        //차후 전환된 scene의 초기화 설정 작업이 완료후 이 속성을 true로 변경
        PhotonNetwork.isMessageQueueRunning = false;

        //백그라운드로 씬 로딩
        AsyncOperation ao = SceneManager.LoadSceneAsync(initialLevel);

        // 씬 로딩이 완료 될때까기 대기...
        yield return ao;

        Debug.Log("로딩 완료");
    }

    public void OnClickJoinRandomRoom()
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = userId.text;
        roomPanel.SetActive(false);
        //무작위로 추출된 룸으로 입장 
        PhotonNetwork.JoinRandomRoom();
    }

    

     //생성된 룸 목록이 변경됐을 때 호출되는 콜백 함수 (최초 룸 접속시 호출) (UI 버전에서 사용)
    void OnReceivedRoomListUpdate()
    {
        // 포톤 클라우드 서버에서는 룸 목록의 변경이 발생하면 클라이언트로 룸 목록을 재전송하기
        // 때문에 밑에 로직이 없으면 다른 클라이언트에서 룸을 나갈때마다 룸 목록이 쌓인다.
        // 룸 목록을 다시 받았을 때 새로 갱신하기 위해 기존에 생성된 RoomItem을 삭제  
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ROOM_ITEM"))
        {
            Destroy(obj);
        }

        //Grid Layout Group 컴포넌트의 Constraint Count 값을 증가시킬 변수
        int rowCount = 0;
        //스크롤 영역 초기화
        //scrollContents.GetComponent<RectTransform>().sizeDelta = new Vector2(0 ,0);
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        //수신받은 룸 목록의 정보로 RoomItem 프리팹 객체를 생성
        //GetRoomList 함수는 RoomInfo 클래스 타입의 배열을 반환
        foreach (RoomInfo _room in PhotonNetwork.GetRoomList())
        {
            Debug.Log(_room.Name);
            //RoomItem 프리팹을 동적으로 생성 하자
            GameObject room = (GameObject)Instantiate(roomItem);
            //생성한 RoomItem 프리팹의 Parent를 지정
            room.transform.SetParent(scrollContents.transform, false);

            //생성한 RoomItem에 룸 정보를 표시하기 위한 텍스트 정보 전달
            RoomData roomData = room.GetComponent<RoomData>();
            roomData.roomName = _room.Name;
            roomData.connectPlayer = _room.PlayerCount;
            roomData.maxPlayers = _room.MaxPlayers;

            //텍스트 정보를 표시 
            roomData.DisplayRoomData();

            //RoomItem의  Button 컴포넌트에 클릭 이벤트를 동적으로 연결
            roomData.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { OnClickRoomItem(roomData.roomName); Debug.Log("Room Click " + roomData.roomName); });
            /*
             * delegate (인자) { 실행코드 };  => 인자는 생략 가능하다
             * delegate (room.name) { OnClickRoomItem( room.name ); Debug.Log("Room Click " + room.name); };
             * delegate { OnClickRoomItem( roomData.roomName ); };
             */

            //Grid Layout Group 컴포넌트의 Constraint Count 값을 증가시키자
            scrollContents.GetComponent<GridLayoutGroup>().constraintCount = ++rowCount;
            //스크롤 영역의 높이를 증가시키자
            scrollContents.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 20);
        }
    }

    void OnClickRoomItem(string roomName)
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.player.NickName = userId.text;
        roomPanel.SetActive(false);

        //인자로 전달된 이름에 해당하는 룸으로 입장
        PhotonNetwork.JoinRoom(roomName);
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        CreateUserList();
    }   

    void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        CreateUserList();
    }
    public void SetReady()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["isReady"] = true;
        PhotonNetwork.player.SetCustomProperties(props);
        Debug.Log(PhotonNetwork.player.NickName + " is Ready!");
        CheckAllPlayersReady();
    }

    void CheckAllPlayersReady()
    {
        if (PhotonNetwork.playerList.Length < 2)
        {
            Debug.Log("플레이어가 2명이 아닙니다.");
            return; 
        }

        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            object isReady;
            if (!player.CustomProperties.TryGetValue("isReady", out isReady) || (bool)isReady == false)
            {
                Debug.Log(player.NickName + " is not ready!");
                return; // 아직 준비되지 않은 플레이어가 있음 -> 게임 시작 불가
            }
        }

        if (PhotonNetwork.isMasterClient)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        Debug.Log("All players are ready! Starting game...");

        PhotonNetwork.LoadLevel(initialLevel);
    }
    void SetValues()
    {
        
    }


    void OnPhotonPlayerPropertiesChanged(object[] playerAndProps)
    {
        PhotonPlayer changedPlayer = playerAndProps[0] as PhotonPlayer;

        foreach (Transform child in userList)
        {
            UserData userData = child.GetComponent<UserData>();
            if (userData != null && userData.IsPlayer(changedPlayer))
            {
                userData.UpdateReadyState();
            }
        }

        if (PhotonNetwork.isMasterClient)
        {
            CheckAllPlayersReady();
        }
    }
}
