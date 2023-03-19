using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Network
{
    public class PhotonLobby : MonoBehaviourPunCallbacks
    {
        [Header("Lobby Properties")]
        [SerializeField] InputField m_createRoomInput;
        [SerializeField] InputField m_joinRoomInput;

        [Header("Room Properties")]
        [SerializeField] UIPhotonRoom m_roomUIPrefab;
        [SerializeField] Transform m_roomContent;
        [SerializeField] List<RoomInfo> m_recentlyUpdateRoom;
        [SerializeField] List<RoomProfile> m_roomList;

        public InputField GetJoinRoomInput() => this.m_joinRoomInput;
        public virtual void CreateRoom()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " create a room with name: " + m_createRoomInput.text);
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.CreateRoom(m_createRoomInput.text, roomOptions);
        }
        public virtual void JoinRoom()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " join a room with name: " + m_joinRoomInput.text);
            PhotonNetwork.JoinRoom(m_joinRoomInput.text);                       
        }
        public virtual void LeaveRoom()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " leave room");
            PhotonNetwork.LeaveRoom();
        }
        public virtual void RoomAdd(RoomInfo room)
        {
            Debug.Log("New room create: " + room.Name);
            if (!m_roomList.Contains(new RoomProfile(room.Name))) m_roomList.Add(new RoomProfile(room.Name));
        }
        public virtual void RoomRemove(RoomInfo room)
        {
            if (m_roomList.Contains(new RoomProfile(room.Name)))
            {
                Debug.Log("Remove a room: " + room.Name);
                m_roomList.Remove(new RoomProfile(room.Name));
            }
        }    
        public virtual void UpdateRoomListUI()
        {
            Debug.Log("Update UI List: " + m_roomList.Count);
            foreach (Transform content in m_roomContent)
            {
                Destroy(content.gameObject);
            }

            foreach (RoomProfile profile in m_roomList)
            {
                Debug.Log(profile.name);
                UIPhotonRoom roomUI = Instantiate(m_roomUIPrefab);
                roomUI.SetUIRoomProfile(profile);
                roomUI.transform.SetParent(m_roomContent);
            }
        }
        
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("Update Date List");
            m_recentlyUpdateRoom = roomList;

            foreach (var room in roomList)
            {
                if (room.RemovedFromList || room.PlayerCount == room.MaxPlayers) RoomRemove(room);
                else RoomAdd(room);
            }
            UpdateRoomListUI();
        }        
    }
}