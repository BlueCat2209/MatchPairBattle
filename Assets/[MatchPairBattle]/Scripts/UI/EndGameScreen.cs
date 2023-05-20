using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EndGameScreen : UIScreen
    {
        [SerializeField] TextMeshProUGUI m_resultText;
        [SerializeField] GameObject m_winPanel;
        [SerializeField] GameObject m_losePanel;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetResultText(string resultText)
        {
            switch (resultText)
            {
                case "VICTORY":                    
                case "DRAW":
                    {
                        m_winPanel.SetActive(true);
                        m_losePanel.SetActive(false);
                    }
                    break;

                case "DEFEATED":
                    {
                        m_winPanel.SetActive(false);
                        m_losePanel.SetActive(true);
                    }
                    break;               
            }
        }
        public void OnReplayButtonPressed()
        {
            GameManagement.Instance.RestartGame();
            Network.PhotonManager.Instance.LoadingForLeaveRoom();
        }
    }
}