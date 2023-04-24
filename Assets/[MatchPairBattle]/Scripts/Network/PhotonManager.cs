using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using UI;
using ExitGames.Client.Photon;

namespace Network
{
    public class PhotonManager : MonoBehaviourPunCallbacks
    {
        private static PhotonManager m_instance;
        public static PhotonManager Instance => m_instance;
        public bool IsHost => PhotonNetwork.IsMasterClient;
        
        [Header("PHOTON UI")]
        [SerializeField] LoginScreen m_loginScreen;
        [SerializeField] LobbyScreen m_lobbyScreen;
        [SerializeField] RoomScreen m_roomScreen;
        [SerializeField] EndGameScreen m_endGameScreen;
        [SerializeField] LoadingScreen m_loadingScreen;

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;
        }

        void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }            
        }
        // Start is called before the first frame update
        void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                m_loginScreen.ShowScreen();
            }

        }

        // Update is called once per frame
        void Update()
        {            
    
        }        

        #region PHOTON EVENTs
        private void OnEventReceive(EventData dataReceive)
        {
            if (IsHost)
            {
                OnMasterClientReceive(dataReceive);
            }
            else
            {
                OnNormalClientReceive(dataReceive);
            }
        }
        private void OnMasterClientReceive(EventData _dataReceive)
        {
            // Import data
            object[] data = (object[])_dataReceive.CustomData;

            switch (_dataReceive.Code)
            {
                case (byte)PhotonEventCode.UpdateClientNameInRoom:
                    // Change into the right data type
                    string memberName = data[0].ToString();

                    // Use data received
                    m_roomScreen.SetOpponnetName(memberName);

                    break;

                case (byte)PhotonEventCode.SetClientReady:
                    // Change into the right data type
                    bool isReady = (bool)data[0];

                    // Use data receive
                    m_roomScreen.SetOpponentReady();
                    break;
            }
            SendNickNameToOtherClient();
        }
        private void OnNormalClientReceive(EventData _dataReceive)
        {
            // Import data
            object[] data = (object[])_dataReceive.CustomData;

            switch (_dataReceive.Code)
            {
                case (byte)PhotonEventCode.UpdateClientNameInRoom:
                    // Change into the right data type
                    string memberName = data[0].ToString();

                    // Use data received
                    m_roomScreen.SetOpponnetName(memberName);
                    break;

                case (byte)PhotonEventCode.SetClientReady:
                    // Doing nothing because only master client can receive this Event
                    break;
            }
        }        
       
        private void SendNickNameToMasterClient()
        {            
            // Export data from normal client 
            object[] dataSend = new object[] { PhotonNetwork.LocalPlayer.NickName };

            // Select only master client will receive this data
            RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };

            // Transfer date
            PhotonNetwork.RaiseEvent(
                (byte)PhotonEventCode.UpdateClientNameInRoom,
                dataSend,
                targetsOption,
                SendOptions.SendUnreliable
                );
        }
        private void SendNickNameToOtherClient()
        {            
            // Export data from master client
            object[] dataSend = new object[] { PhotonNetwork.LocalPlayer.NickName };

            // Select other client to receive this data
            RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            // Transfer data
            PhotonNetwork.RaiseEvent(
                (byte)PhotonEventCode.UpdateClientNameInRoom,
                dataSend,
                targetsOption,
                SendOptions.SendUnreliable
                );
        }
        private void SendClientStateReady()
        {            
            // Export data
            object[] dataSend = new object[] { true };

            // Select only master client receive this data
            RaiseEventOptions targetOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent(
                (byte)PhotonEventCode.SetClientReady,
                dataSend,
                targetOption,
                SendOptions.SendUnreliable
                );
        }
        private void SendHostStartGame()
        {
            HideAllUIScreen();
            m_loadingScreen.ShowScreen();

            GameManagement.Instance.StartGame();
        }
        
        public void SendEventForRoomData()
        {
            if (IsHost)
            {
                SendHostStartGame();
            }
            else
            {
                SendClientStateReady();
            }
        }
        #endregion        

        #region LOADING SCREEN
        private void HideAllUIScreen()
        {
            m_endGameScreen.HideScreen();
            m_loadingScreen.HideScreen();
            m_lobbyScreen.HideScreen();
            m_loginScreen.HideScreen();
            m_roomScreen.HideScreen();
        }
        public void LoadingForLogin(string playerName)
        {
            HideAllUIScreen();
            m_loadingScreen.StartLoading();
            
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.AutomaticallySyncScene = true;    
            
            PhotonNetwork.ConnectUsingSettings();
        }
        public void LoadingForJoinRoom(string roomName)
        {
            HideAllUIScreen();
            m_loadingScreen.StartLoading();

            PhotonNetwork.JoinRoom(roomName);
        }
        public void LoadingForCreateRoom(string roomName, RoomOptions roomSettings)
        {
            HideAllUIScreen();
            m_loadingScreen.StartLoading();

            PhotonNetwork.CreateRoom(roomName, roomSettings);
        }
        public void LoadingForStartGame(float countDownTime, System.Action callbacks = null)
        {            
            HideAllUIScreen();
            callbacks += HideAllUIScreen;
            m_loadingScreen.StartCountDown(countDownTime, callbacks);
        }
        public void LoadingForEndGame(string resultText)
        {
            HideAllUIScreen();
            m_endGameScreen.ShowScreen();
            m_endGameScreen.SetResultText(resultText);
        }
        public void LoadingForLeaveRoom()
        {
            HideAllUIScreen();
            m_loadingScreen.StartLoading();

            PhotonNetwork.LeaveRoom();
        }
        #endregion

        #region PUN CALLBACKS
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                Debug.Log(roomList[i].Name + "|" + roomList[i].IsOpen + "|" + roomList[i].PlayerCount + "|" + roomList[i].MaxPlayers);
            }
            m_lobbyScreen.UpdateRoomListUI(roomList);
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " connected to Master");
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        public override void OnJoinedLobby()
        {
            HideAllUIScreen();
            m_lobbyScreen.ShowScreen();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined Lobby: " + PhotonNetwork.CurrentLobby.ToString());
        }        
        public override void OnJoinedRoom()
        {
            HideAllUIScreen();
            m_roomScreen.ShowScreen();
            if (PhotonNetwork.IsMasterClient)
            {
                m_roomScreen.RoomSetup(true);
            }
            else
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                m_roomScreen.RoomSetup(false, SendNickNameToMasterClient);
            }            
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined Room: " + PhotonNetwork.CurrentRoom.Name);
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined  Room failed!");
            HideAllUIScreen();
            m_lobbyScreen.ShowScreen();
        }
        #endregion
    }
}