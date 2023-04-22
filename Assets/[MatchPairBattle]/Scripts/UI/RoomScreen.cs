using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoomScreen : UIScreen
    {
        [Header("ROOM PROPERTiES")]
        [SerializeField] TMPro.TextMeshProUGUI m_roomID;
        [SerializeField] Image m_opponentPanel;
        [SerializeField] TMPro.TextMeshProUGUI m_opponentName;
        [SerializeField] Button m_startGameButton;

        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RoomSetup(bool isHost, Action callbacks)
        {
            if (isHost)
            {
                m_startGameButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "START";
                m_startGameButton.interactable = false;
            }
            else
            {
                m_startGameButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "READY";
                m_startGameButton.interactable = true;
            }
            callbacks?.Invoke();
        }
        public void SetOpponnetName(string name)
        {
            m_opponentName.text = name;
        }
        public void SetOpponentReady()
        {
            m_startGameButton.interactable = true;
            m_startGameButton.GetComponent<Image>().color = Color.green;
        }
        public void OnStartGameButtonPressed()
        {
            m_startGameButton.interactable = false;
            Network.PhotonManager.Instance.SendEventForRoomData();
        }
        public void OnLeaveRoomButtonPressed()
        {
            Network.PhotonManager.Instance.LoadingForLeaveRoom();
        }
    }
}