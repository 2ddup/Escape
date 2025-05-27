using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// (UI 버전에서 사용)
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;
using TempNamespace.Player;
using Unity.VisualScripting;

public class PhotonStage : MonoBehaviour
{
    public  TMP_InputField inputMsg;
    // 포톤 추가////////////////////////////////////////////////
    //접속된 플레이어 수를 표시할 Text UI 항목 연결 레퍼런스 (Text 컴포넌트 연결 레퍼런스)
    public TextMeshProUGUI txtConnect;

    //접속 로그를 표시할 Text UI 항목 연결 레퍼런스 선언
    public TextMeshProUGUI txtLogMsg;

    //RPC호출을 위한 PhotonVeiw연결 레퍼런스
    PhotonView pv;

    //플레어의 생성 위치 저장 레퍼런스
    private Transform[] playerPos;
    ////////////////////////////////////////////////////////////
    public UnityEvent<GameObject> OnPlayerCreated;

    // (네트워크 UI 버전에서 ...)
    //Enemy 프리팹을 위한 레퍼런스
    //public GameObject Enemy;

    //게임 끝
    private bool gameEnd;
    bool isFocused = false;
    void Awake()
    {
        // 포톤 추가////////////////////////////////////////////////
        //PhotonView 컴포넌트를 레퍼런스에 할당
        pv = GetComponent<PhotonView>();

        playerPos = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponentsInChildren<Transform>();

        //포톤 클라우드로부터 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.isMessageQueueRunning = true;

        //룸에 입장한 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();
        ////////////////////////////////////////////////////////////
    }
    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.playerList.Length == 2);
        StartCoroutine(CreatePlayer());
        // 포톤 추가/////////////////////////////////////////////////////////////
        /*
         * 유니티 마크업 태그
         * 글자크기 => <size="글자크기"> 표시할 내용 </size>
         * 글자색 => <color="컬러"> 표시할 내용 </color>
         * 진하게 => <b> 표시할 내용 </b>
         * 이탤릭 => <i> 표시할 내용 </i>
         * 
         *  EX)
         *  string sHp = "<color=yellow><b>HP: ##</b></color>";
         *  string sScore = "<color=#00ff00><b>SCORE: ##</b></color>";
         *  
         *  GUI.Lable ( new Rect(10, 10, 120, 50), sHp.Replace("##", 생명력.ToString() ) );
         *  GUI.Lable ( new Rect(50, 10, 120, 50), sScore.Replace("##", "" + score ) );
         *  
         *  Text에 색을 넣어서 사용해야 할때가 있는데 아래 처럼 사용하면 됨. (rgp색 16진수로 조합해서 사용)
         *  
         */

        // 로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#00ff00>["
                    + PhotonNetwork.player.NickName
                    + "] Connected</color>";

        //RPC 함수 호출
        //(CF) 플레이어 접속,종료 시 호출되는 콜백 함수에서 메시지를 표시하는 루틴을 추가하여도
        //상관없지만, 뒤늦게 룸에 조인한 플레이어의 로그 창에 로그 메시지를 띄우기 위함.(로그 메시지를 Buffered RPC 처리)
        pv.RPC( "LogMsg", PhotonTargets.AllBuffered, msg );
        

        //룸에 있는 네트워크 객체 간의 통신이 완료될 때까지 잠시 대기
        yield return new WaitForSeconds( 1.0f );  
        ///////////////////////////////////////////////////////////////////////////////////////
    }

    // 포톤 추가
    // 플레이어를 생성하는 함수
    IEnumerator CreatePlayer()
    {
        int index = 0;

        // 플레이어 ID를 기반으로 인덱스를 정렬
        PhotonPlayer[] sortedPlayers = PhotonNetwork.playerList;
        System.Array.Sort(sortedPlayers, (a, b) => a.ID.CompareTo(b.ID));

        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            if (sortedPlayers[i] == PhotonNetwork.player)
            {
                index = i;
                break;
            }
        }

        GameObject player = PhotonNetwork.Instantiate("MainPlayer", playerPos[index + 1].position, playerPos[index + 1].rotation, 0);
        player.tag = "LocalPlayer";
        player.GetComponent<PhotonNetworkPlayer>().PlayerID = player.GetComponent<PhotonView>().viewID;
        OnPlayerCreated?.Invoke(player);

        yield return null;
    }
    

    //포톤 추가
    //룸 접속자 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        //현재 입장한 룸 정보를 받아옴(레퍼런스 연결)
        Room currRoom = PhotonNetwork.room;

        //현재 룸의 접속자 수와 최대 접속 가능한 수를 문자열로 구성한 다음 Text UI 항목에 출력
        txtConnect.text = currRoom.PlayerCount.ToString()
                            + "/"
                            + currRoom.MaxPlayers.ToString();
    }

    //포톤 추가
    //네트워크 플레이어가 룸으로 접속했을 때 호출되는 콜백 함수
    //PhotonPlayer 클래스 타입의 파라미터가 전달(서버에서...)
    //PhotonPlayer 파라미터는 해당 네트워크 플레이어의 정보를 담고 있다.
    void OnPhotonPlayerConnected ( PhotonPlayer newPlayer )
    {
        // 플레이어 ID (접속 순번), 이름, 커스텀 속성
        Debug.Log(newPlayer.ToStringFull());

        // 룸에 현재 접속자 정보를 display
        GetConnectPlayerCount();

    }

    // 포톤 추가
    //네트워크 플레이어가 룸을 나가거나 접속이 끊어졌을 경우 호출되는 콜백 함수
    void OnPhotonPlayerDisconnected ( PhotonPlayer outPlayer)
    {
        // 룸에 현재 접속자 정보를 display
        GetConnectPlayerCount();
    }

    // 포톤 추가
    [PunRPC]
    void LogMsg( string msg)
    {
        //로그 메시지 Text UI에 텍스트를 누적시켜 표시
        txtLogMsg.text = txtLogMsg.text + msg; 
    }

    // 포톤 추가
    // 룸 나가기 버튼 클릭 이벤트에 연결될 함수
    public void OnClickExitRoom()
    {
        // 로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#ff0000>["
                    + PhotonNetwork.player.NickName
                    + "]Disconnected</color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", PhotonTargets.AllBuffered, msg);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["isReady"] = false;
        PhotonNetwork.player.SetCustomProperties(props);

        //현재 룸을 빠져나가며 생성한 모든 네트워크 객체를 삭제
        PhotonNetwork.LeaveRoom();

        //(!) 서버에 통보한 후 룸에서 나가려는 클라이언트가 생성한 모든 네트워크 객체및 RPC를 제거하는 과정 진행(포톤 서버에서 진행)
    }

    // 포톤 추가
    //룸에서 접속 종료됐을 때 호출되는 콜백 함수 ( (!) 과정 후 포톤이 호출 )
    void OnLeftRoom()
    {
        // 로비로 이동
        SceneManager.LoadScene("Lobby");
    }

    public void Send()
    {
        string msg = "\n<color=#ffffff>"
                    + PhotonNetwork.player.NickName
                    + " : " + inputMsg.text 
                    + "</color>";
        
        txtLogMsg.text = txtLogMsg.text + msg;

        string othermsg = "\n<color=#00ff00>"
                    + PhotonNetwork.player.NickName
                    + " : " + inputMsg.text 
                    + "</color>";
        
        pv.RPC("LogMsg", PhotonTargets.Others, othermsg);

        inputMsg.text = "";
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(!isFocused)
            {
                inputMsg.ActivateInputField();
                isFocused = true;
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(inputMsg.text))
                {
                    Send();
                    inputMsg.text = "";
                    inputMsg.ActivateInputField();
                }
                else
                {
                    inputMsg.DeactivateInputField();
                    isFocused = false;
                }
            }
        }
    }
}

/* RPC(Remote Procedure Call)함수의 네트웍 전달 대상 설정인 PhotonTargets 열거형 인자 옵션 설정
 *  옵션                      설명
 *  All                       모든 네트웍 유저에게 RPC 데이타를 전송하고 자신은 즉시 RPC 함수를 실행    
 *  Others                    자기 자신을 제외한 모든 네트웍 유저에게 RPC 데이타를 전송 
 *  MasterClient              Master Client에게만 RPC 데이타를 전송
 *  AllBuffered               All 옵션과 같으며, 또한 나중에 입장한 네트웍 유저에게 버퍼에 저장돼 있던 RPC 데이타가 전송
 *  OtherBuffered             Others 옵션과 같으며, 또한 나중에 입장한 네트웍 유저에게 버퍼에 저장해둔 RPC 데이타를 전송
 *  AllViaServer              모든 네트웍 유저에게 거의 동일한 시간에 RPC 데이타를 전송하기 위하여 서버에서 연결된 
 *                            모든 클라이언트들에게 RPC 데이타를 동시에 전송
 *  AllBufferedViaServer      AllViaServer 옵션과 같으며, 버퍼에 저장해둔 RPC 데이타를 나중에 입장한 네트웍 유저에게 전송 
 *  
 *  사용 방식: 1
 *  //자신의 아바타일 경우는 로컬함수를 호출하여 케논을 발포
 *  FireStart(100);
 *
 *  //원격 네트워크 플레이어의 자신의 아바타 케릭터에는 RPC로 원격으로 FireStart 함수를 호출 
 *  pv.RPC( "FireStart", PhotonTargets.Others, 100 );
 *
 * 사용 방식: 2
 *  //모든 네트웍 유저에게 RPC 데이타를 전송하여 RPC 함수를 호출, 로컬 플레이어는 로컬 Fire 함수를 바로 호출 
 *  //pv.RPC("FireStart", PhotonTargets.All, 100);
 *  
 *   [PunRPC]
 *   //플레이어 발사
 *  private void FireStart(int power)
 *  {
 *  }
 */
