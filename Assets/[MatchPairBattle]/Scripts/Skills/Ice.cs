using UnityEngine;
using UnityEngine.UI;

namespace ElementSkill
{
    public class Ice : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] Image m_skillImage;
        [SerializeField] float m_skillSpeed;

        private const float m_castingTime = 10f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (m_isCasting)
            {
                if (m_skillImage.fillAmount < 1f)
                {
                    m_skillImage.fillAmount += Time.deltaTime * m_castingTime;
                }
                else
                {
                    m_isRelease = true;
                    m_isCasting = false;
                }
            }
            if (m_isRelease)
            {
                if (m_skillImage.fillAmount > 0f)
                {
                    m_skillImage.fillAmount -= Time.deltaTime * m_skillSpeed;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}