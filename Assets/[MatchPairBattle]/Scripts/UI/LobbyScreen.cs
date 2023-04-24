using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class LobbyScreen : UIScreen
    {
        [Header("LOBBY PROPERTIES")]
        [SerializeField] GameObject m_backButton;
        [Space]
        
        [Header("Create Room")]
        [SerializeField] GameObject m_createRoomPanel;        
        [SerializeField] TMPro.TMP_InputField m_createRoomNameInput;
        [Space]

        [Header("Join Room")]
        [SerializeField] GameObject m_joinRoomPanel;
        [SerializeField] Transform m_roomIDHolder;
        [SerializeField] GameObject m_roomIDPrefab;
        [SerializeField] TMPro.TMP_InputField m_joinRoomNameInput;

        private List<Photon.Realtime.RoomInfo> m_roomList = new List<Photon.Realtime.RoomInfo>();
        
        #region Create room
        public void OnCreateRoomButtonPressed()
        {
            m_backButton.SetActive(true);
            m_joinRoomPanel.SetActive(false);
            m_createRoomPanel.SetActive(true);
        }
        public void CreateRoom()
        {
            var roomName = m_createRoomNameInput.text;
            var roomSettings = new Photon.Realtime.RoomOptions();
            roomSettings.MaxPlayers = 2;

            Network.PhotonManager.Instance.LoadingForCreateRoom(roomName, roomSettings);
        }
        #endregion

        #region Join room
        private void FillInText(string text)
        {
            m_joinRoomNameInput.text = text;
        }
        public void OnJoinRoomButtonPressed()
        {
            m_backButton.SetActive(true);
            m_joinRoomPanel.SetActive(true);
            m_createRoomPanel.SetActive(false);            
        }
        public void JoinRoom()
        {
            var roomName = m_joinRoomNameInput.text;
            Network.PhotonManager.Instance.LoadingForJoinRoom(roomName);
        }        
        public void UpdateRoomListUI(List<Photon.Realtime.RoomInfo> roomInfos)
        {
            // Get roomInfos List
            m_roomList = roomInfos;
            
            // Add more filters for list
            for (int i = 0; i < roomInfos.Count; i++)
            {                
                if (roomInfos[i].RemovedFromList || !roomInfos[i].IsOpen || roomInfos[i].PlayerCount == roomInfos[i].MaxPlayers)
                {
                    m_roomList.Remove(roomInfos[i]);
                }
            }

            // Remove old content from previous update
            foreach (Transform content in m_roomIDHolder)
            {
                Destroy(content.gameObject);
            }

            // Add new content for this update
            for (int i = 0; i < m_roomList.Count; i++)
            {
                var roomID = Instantiate(m_roomIDPrefab, m_roomIDHolder);
                roomID.GetComponent<UIRoomID>().RoomID = m_roomList[i].Name;
                roomID.GetComponent<UIRoomID>().RoomSelectedCallbacks += FillInText;
            }
        }
        #endregion

        public void OnBackButtonPressed()
        {
            m_backButton.SetActive(false);
            m_joinRoomPanel.SetActive(false);
            m_createRoomPanel.SetActive(false);            
        }
        public override void HideScreen()
        {
            base.HideScreen();

            m_backButton.SetActive(false);
            m_joinRoomPanel.SetActive(false);
            m_createRoomPanel.SetActive(false);

            m_createRoomNameInput.text = "";
            m_joinRoomNameInput.text = "";
        }
    }
}