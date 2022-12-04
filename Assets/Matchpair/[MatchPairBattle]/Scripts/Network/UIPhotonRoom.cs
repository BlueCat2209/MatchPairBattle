using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Network
{ 
    public class UIPhotonRoom : MonoBehaviour
    {
        [SerializeField] protected Text m_roomName;
        [SerializeField] protected RoomProfile m_roomProfile;
        [SerializeField] protected InputField m_inputField;

        private void Awake()
        {
            var roomControl = FindObjectOfType<PhotonLobby>();
            m_inputField = roomControl.GetJoinRoomInput();
        }

        public void SetUIRoomProfile(RoomProfile _profile)
        {
            this.m_roomName.text = _profile.name;
            this.m_roomProfile = _profile;
        }
        public void OnRoomButtonClicked()
        {
            m_inputField.text = m_roomName.text;
        }

        public UIPhotonRoom(RoomProfile _roomProfile)
        {
            this.m_roomName.text = _roomProfile.name;
            this.m_roomProfile = _roomProfile;
        }
    }
}