using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace Network
{
    public class PhotonRoom : MonoBehaviour
    {
        [SerializeField] Text m_roomMemberText;
        [SerializeField] Button m_GameControlButton;
        bool m_isAllClientReady;

        private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
        private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;
        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                m_GameControlButton.GetComponentInChildren<Text>().text = "START";
            }
            else
            {
                m_GameControlButton.GetComponentInChildren<Text>().text = "READY";
                SendNickNameToMasterClient();
            }
        }

        protected virtual void OnEventReceive(EventData _dataReceive)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                OnMasterClientReceive(_dataReceive);
            }
            else
            {
                OnNormalClientReceive(_dataReceive);
            }
        }
        protected virtual void OnMasterClientReceive(EventData _dataReceive)
        {
            // Import data
            object[] data = (object[])_dataReceive.CustomData;
            
            switch (_dataReceive.Code)
            {
                case (byte) PhotonEventCode.UpdateClientNameInRoom:                    
                    // Change into the right data type
                    string memberName = data[0].ToString();
                    
                    // Use data received
                    m_roomMemberText.text = memberName;
                    
                    break;

                case (byte) PhotonEventCode.SetClientReady:
                    // Change into the right data type
                    bool isReady = (bool) data[0];

                    // Use data receive
                    m_isAllClientReady = isReady;
                    if (m_isAllClientReady) m_roomMemberText.GetComponentInParent<Image>().color = Color.green;
                    break;
            }
            SendNickNameToOtherClient();
        }
        protected virtual void OnNormalClientReceive(EventData _dataReceive)
        {
            // Import data
            object[] data = (object[])_dataReceive.CustomData;

            switch (_dataReceive.Code)
            {
                case (byte)PhotonEventCode.UpdateClientNameInRoom:
                    // Change into the right data type
                    string memberName = data[0].ToString();

                    // Use data received
                    m_roomMemberText.text = memberName;

                    break;

                case (byte)PhotonEventCode.SetClientReady:
                    // Doing nothing because only master client can receive this Event
                    break;
            }
        }
        protected virtual void SendNickNameToMasterClient()
        {
            // Export data from normal client 
            object[] dataSend = new object[] { PhotonNetwork.LocalPlayer.NickName };

            // Select only master client will receive this data
            RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };

            // Transfer date
            PhotonNetwork.RaiseEvent(
                (byte) PhotonEventCode.UpdateClientNameInRoom,
                dataSend,
                targetsOption,
                SendOptions.SendUnreliable
                );
        }
        protected virtual void SendNickNameToOtherClient()
        {
            // Export data from master client
            object[] dataSend = new object[] { PhotonNetwork.LocalPlayer.NickName };

            // Select other client to receive this data
            RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            // Transfer data
            PhotonNetwork.RaiseEvent(
                (byte) PhotonEventCode.UpdateClientNameInRoom,
                dataSend,
                targetsOption,
                SendOptions.SendUnreliable
                );
        }        

        public virtual void OnLeaveRoomButtonPressed()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " leave room " + PhotonNetwork.InRoom);
            PhotonNetwork.LeaveRoom();            
        }
        public virtual void OnGameControlButtonPressed()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartGame();
            }
            else
            {
                ReadyGame();
            }
        }
        protected virtual void ReadyGame()
        {
            // Set ready for member client
            m_roomMemberText.GetComponentInParent<Image>().color = Color.green;

            // Export data
            object[] dataSend = new object[] { true };

            // Select only master client receive this data
            RaiseEventOptions targetOption = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };

            PhotonNetwork.RaiseEvent(
                (byte) PhotonEventCode.SetClientReady,
                dataSend,
                targetOption,
                SendOptions.SendUnreliable
                );

        }
        protected virtual void StartGame()
        {
            if (m_isAllClientReady)
            {
                PhotonNetwork.LoadLevel("Game");
            }
            else
            {
                Debug.LogWarning("Please wait until other member ready!");
            }
        }
    }
}