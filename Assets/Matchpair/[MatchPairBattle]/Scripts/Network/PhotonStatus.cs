using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Network
{
    public class PhotonStatus : MonoBehaviour
    {
        Text m_statusText;

        void Start()
        {
            m_statusText = GetComponent<Text>();
        }
        // Update is called once per frame
        void Update()
        {
            m_statusText.text = PhotonNetwork.NetworkClientState.ToString();
        }
    }
}