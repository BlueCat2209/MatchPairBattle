using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElementSkill
{
    public class Earth : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] Image m_skillImage;
        [SerializeField] float m_castSpeed = 10f;
        [SerializeField] float m_releaseSpeed = 1f;
                
        private List<Pikachu.AnimalButton> m_targetButtonList = new List<Pikachu.AnimalButton>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            CastAndRelease();

        }
        
        private void CastAndRelease()
        {
            if (m_isCasting)
            {
                if (m_skillImage.fillAmount < 1f)
                {
                    m_skillImage.fillAmount += Time.deltaTime * m_castSpeed;
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
                    m_skillImage.fillAmount -= Time.deltaTime * m_castSpeed;
                }
                else
                {
                    ReleaseSkill();
                }
            }
        }
        private void ReleaseSkill()
        {
            float countDown = 0;
            for (int i = 0; i < m_targetButtonList.Count; i++)
            {
                if (m_targetButtonList[i].Image.color.a > 0)
                {
                    var tmp = m_targetButtonList[i].Image.color;
                    tmp.a -= Time.deltaTime * m_releaseSpeed;
                    m_targetButtonList[i].Image.color = tmp;
                }
                else
                {
                    countDown++;
                }
            }

            if (countDown == m_targetButtonList.Count)
            {
                for (int i = 0; i < m_targetButtonList.Count; i++)
                {
                    m_targetButtonList[i].OnHideButton();                    
                }                
                Destroy(gameObject);
            }
        }

        public virtual void StartSkill(List<Pikachu.AnimalButton> targetTypeButtons, Vector2 tableSize)
        {
            m_isCasting = true;
            for (int i = 0; i < targetTypeButtons.Count; i++)
            {                
                bool isOnTheEdgeOfRow = (targetTypeButtons[i].CoordinateY == 0 || targetTypeButtons[i].CoordinateY == tableSize.y - 1);
                bool isOnTheEdgeOfColumn = (targetTypeButtons[i].CoordinateX == 0 || targetTypeButtons[i].CoordinateX == tableSize.x - 1);                
                if (!isOnTheEdgeOfRow && !isOnTheEdgeOfColumn)
                {                    
                    m_targetButtonList.Add(targetTypeButtons[i]);

                    targetTypeButtons[i].IsObstacle = true;
                    targetTypeButtons[i].Image.sprite = m_skillImage.sprite;
                    targetTypeButtons[i].Image.color = new Color(1, 1, 1, 1);
                    targetTypeButtons[i].GetComponent<Button>().interactable = true;

                    Debug.LogWarning(targetTypeButtons[i].Image.sprite + "|" + targetTypeButtons[i].Image.color.a);
                }            
            }
        }
    }
}