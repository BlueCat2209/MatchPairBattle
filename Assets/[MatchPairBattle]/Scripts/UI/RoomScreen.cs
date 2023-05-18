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
        [SerializeField] Button m_startGameButton;
        [SerializeField] TMPro.TextMeshProUGUI m_roomID;
        [SerializeField] TMPro.TextMeshProUGUI m_notificationText;

        [Header("Player panel")]
        [SerializeField] Image m_playerAvatar;
        [SerializeField] TMPro.TextMeshProUGUI m_playerName;

        [Header("Opponent panel")]
        [SerializeField] Image m_opponentAvatar;
        [SerializeField] TMPro.TextMeshProUGUI m_opponentName;        
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RoomSetup(bool isHost, string roomID, string playerName, Action callbacks = null)
        {            
            if (isHost)
            {
                m_startGameButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "BEGIN";
                m_startGameButton.interactable = false;
            }
            else
            {
                m_startGameButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "READY";
                m_startGameButton.interactable = true;
            }

            m_playerName.text = playerName;
            callbacks?.Invoke();
        }
        public void SetOpponnetName(string name)
        {
            m_opponentName.text = name;
        }
        public void ResetRoom()
        {
            m_notificationText.text = "Waiting for opponent...";
            m_opponentName.text = "???";
        }
        public void SetOpponentReady()
        {
            m_notificationText.text = "Let the battle begin!!!";
            m_startGameButton.interactable = true;            
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