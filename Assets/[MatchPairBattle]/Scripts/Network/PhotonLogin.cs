using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Network
{
    public class PhotonLogin : MonoBehaviourPunCallbacks
    {
        [SerializeField] InputField m_inputUserName;        

        // Start is called before the first frame update

        public virtual void Login()
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is logging");
            PhotonNetwork.LocalPlayer.NickName = m_inputUserName.text;

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}