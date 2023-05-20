using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementSkill
{
    public class ButtonSkill : MonoBehaviour
    {
        [SerializeField] SkillType m_skillType;
        [SerializeField] int m_stackAmount;
        [SerializeField] float m_skillDelay;

        [SerializeField] Image m_stackImage;
        [SerializeField] Button m_skillButton;
        [SerializeField] GameObject m_skillPrefab;
        [SerializeField] TextMeshProUGUI m_countDownText;

        [SerializeField]
        private bool m_isDelaying = false;
        [SerializeField]
        private float m_countDownTime = 0f;
        public GameObject SkillPrefab => m_skillPrefab;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_isDelaying)
            {
                m_countDownTime -= Time.deltaTime;
                m_countDownText.text = Mathf.CeilToInt(m_countDownTime).ToString();

                if (m_countDownTime <= 0)
                {
                    m_isDelaying = false;
                    m_countDownText.gameObject.SetActive(false);

                    CheckSkillCanBeUsed();
                }
            }
        }
        private void CheckSkillCanBeUsed()
        {
            m_skillButton.interactable = (Mathf.Approximately(m_stackImage.fillAmount, 1f)) && !m_isDelaying;
        }
        public void DelayForSkill(float delayTime)
        {
            m_isDelaying = true;
            m_countDownTime = delayTime;
            m_countDownText.gameObject.SetActive(true);
        }
        public void SkillReset()
        {
            m_isDelaying = false;
            m_countDownTime = 0;
            m_countDownText.gameObject.SetActive(false);

            m_stackImage.fillAmount = 0;
            m_skillButton.interactable = false;
        }
        public void StackSkill()
        {
            m_stackImage.fillAmount += 1f / m_stackAmount;
            CheckSkillCanBeUsed();
        }
        public void UseSkill()
        {
            if (m_isDelaying) return;
            GameManagement.Instance.CastSkill(m_skillType, m_skillDelay);
            m_stackImage.fillAmount = 0f;
            m_skillButton.interactable = false;
        }
    }
}