using UnityEngine;
using System.Collections.Generic;

namespace ElementSkill
{
    public class Fire : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] GameObject m_firePrefab;
        [SerializeField] float m_speed;

        private int m_fireAmount;
        
        private List<Vector3> m_fireBallTarget = new List<Vector3>();        
        private List<Transform> m_fireBallTransform = new List<Transform>();        
        private List<Pikachu.AnimalButton> m_animalButtons = new List<Pikachu.AnimalButton>();

        void Update()
        {
            if (!m_isCasting) return;

            int countDown = 0;
            for (int i = 0; i < m_fireAmount; i++)
            {
                if (Vector3.Distance(m_fireBallTransform[i].localPosition, m_fireBallTarget[i]) > 0.1f)
                {
                    m_fireBallTransform[i].localPosition = Vector3.MoveTowards(m_fireBallTransform[i].localPosition, m_fireBallTarget[i], m_speed * Time.deltaTime);
                    m_fireBallTransform[i].up = (m_fireBallTransform[i].localPosition - m_fireBallTarget[i]);
                }
                else
                {
                    countDown++;
                    m_fireBallTransform[i].localPosition = m_fireBallTarget[i];
                }
            }
            if (countDown == m_fireAmount)
            {
                m_isCasting = false;
                for (int i = 0; i < m_fireAmount; i++)
                {
                    m_targetTable.HideButton(m_animalButtons[i]);
                    Destroy(m_fireBallTransform[i].gameObject, 1f);
                }
                Destroy(gameObject, 1f);
            }
        }

        public virtual void StartSkill(List<byte[]> skillTargets, Vector3 skillStart, bool isPlayerCast = true)
        {
            m_isCasting = true;
            m_fireAmount = (skillTargets.Count >= 6) ? 6 : (skillTargets.Count / 2) * 2;
            m_targetTable = (isPlayerCast) ? GameManagement.Instance.PlayerTable : GameManagement.Instance.OpponentTable;

            for (int i = 0; i < m_fireAmount; i++)
            {
                // Get fireBallTarget information
                var animalButton = m_targetTable.Table[skillTargets[i][0], skillTargets[i][1]];
                m_animalButtons.Add(animalButton);
                m_fireBallTarget.Add(new Vector3(animalButton.transform.localPosition.x, animalButton.transform.localPosition.y, -10));

                // Create fireball
                var skill = Instantiate(m_firePrefab, transform);
                skill.transform.position = skillStart;
                m_fireBallTransform.Add(skill.transform);
            }
        }
    }
}

