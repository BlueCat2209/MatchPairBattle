using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Network
{
    public class PhotonLogout : MonoBehaviour
    {
        public virtual void Logout()
        {
            PhotonNetwork.Disconnect();
        }
    }
}