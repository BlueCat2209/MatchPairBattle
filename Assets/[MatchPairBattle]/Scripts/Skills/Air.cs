using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementSkill
{
    public class Air : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] SpriteRenderer m_skillSprite;
        [SerializeField] Vector3 m_scaleForPlayerTable;
        [SerializeField] Vector3 m_scaleForOpponentTable;
        [SerializeField] float m_castingTime;
        [SerializeField] float m_releaseTime;

        private byte[,] m_tableData;
        private Vector2 m_tableSize;

        private Vector3 m_skillStartPosition;
        private Vector3 m_skillTargetPosition = new Vector3(0, 0, -10f);

        private Vector3 m_defaultScale;
        private Vector3 m_targetScale;
        
        private float m_countDown = 0f;
        private bool m_isPlayerCast;

        // Start is called before the first frame update
        void Start()
        {
            m_defaultScale = transform.localScale;
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
                if (m_countDown < m_castingTime)
                {
                    m_countDown += Time.deltaTime;
                    transform.localScale = Vector3.Lerp(m_defaultScale, m_targetScale, m_countDown / m_castingTime);
                    transform.localPosition = Vector3.Lerp(m_skillStartPosition, m_skillTargetPosition, m_countDown / m_castingTime);                    
                }
                else
                {
                    if (m_isPlayerCast)
                    {
                        m_targetTable.ShuffleTable();
                        GameManagement.Instance.SendTableData(m_targetTable.Table, m_targetTable.TableSize, false);                        
                    }
                    else
                    {
                        Debug.LogWarning("Hit skill");
                        m_targetTable.CreateTable(m_tableData, m_tableSize, false);
                    }

                    m_isCasting = false;
                    m_isRelease = true;
                    m_countDown = 0;
                }
            }
            if (m_isRelease)
            {
                if (m_countDown < m_releaseTime)
                {
                    m_countDown += Time.deltaTime;
                    m_skillSprite.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(0, 0, 0, 0), m_countDown / m_releaseTime);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        public virtual void StartSkillToPlayer(Vector3 skillStart, byte[,] tableData, Vector2 tableSize)
        {
            m_skillStartPosition = new Vector3(skillStart.x, skillStart.y, -10f);
            m_targetTable = GameManagement.Instance.PlayerTable;
            m_targetScale = m_scaleForPlayerTable;

            m_tableData = tableData;
            m_tableSize = tableSize;

            m_isPlayerCast = false;
            m_isCasting = true;
        }
        public virtual void StartSkillToOpponent(Vector3 skillStart)
        {
            m_skillStartPosition = new Vector3(skillStart.x, skillStart.y, -10f);
            m_targetTable = GameManagement.Instance.OpponentTable;
            m_targetScale = m_scaleForOpponentTable;            
            
            m_isPlayerCast = true;
            m_isCasting = true;
        }
    }
}