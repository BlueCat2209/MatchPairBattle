using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LoadingScreen : UIScreen
    {
        [SerializeField] GameObject m_loadingImage;
        [SerializeField] GameObject m_countDonwPanel;
        [SerializeField] TextMeshProUGUI m_countDownText;

        private bool m_isCounting = false;
        private float m_timeToCountDown = 0f;
        private System.Action m_countDownCallbacks;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_isCounting)
            {
                m_timeToCountDown -= Time.deltaTime;
                m_countDownText.text = Mathf.CeilToInt(m_timeToCountDown).ToString();

                if (m_timeToCountDown <= 0)
                {
                    m_countDonwPanel.SetActive(false);
                    m_countDownCallbacks?.Invoke();
                    m_timeToCountDown = 0f;
                    m_isCounting = false;
                }
            }

        }

        public void StartLoading()
        {
            ShowScreen();
            m_countDonwPanel.SetActive(false);
            m_loadingImage.SetActive(true);
        }
        public void StartCountDown(float countDownTime, System.Action callbacks = null)
        {
            ShowScreen();
            m_isCounting = true;
            m_timeToCountDown = countDownTime;
            m_countDownCallbacks += callbacks;
            
            m_countDonwPanel.SetActive(true);
            m_loadingImage.SetActive(false);
        }
    }
}