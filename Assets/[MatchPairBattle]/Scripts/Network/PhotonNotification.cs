using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Network
{
    public class PhotonNotification : MonoBehaviourPunCallbacks
    {
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        
        public override void OnConnectedToMaster()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " connected to Master");
            if (!PhotonNetwork.InLobby)
            {
                Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joining Lobby");
                PhotonNetwork.JoinLobby();
                Debug.Log(PhotonNetwork.InLobby);
            }
        }
        public override void OnJoinedLobby()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined Lobby");
        }
        public override void OnJoinedRoom()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined room " + PhotonNetwork.CurrentRoom.Name);
            SceneManager.LoadScene("Room");
        }
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}