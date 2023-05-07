using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EndGameScreen : UIScreen
    {
        [SerializeField] TextMeshProUGUI m_resultText;        

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
            m_resultText.text = resultText;
        }
        public void OnReplayButtonPressed()
        {
            GameManagement.Instance.RestartGame();
            Network.PhotonManager.Instance.LoadingForLeaveRoom();
        }
    }
}