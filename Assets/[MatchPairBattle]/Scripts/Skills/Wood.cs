using UnityEngine;
using System.Collections.Generic;

namespace ElementSkill
{
    public class Wood : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] GameObject m_misslePrefab;
        [SerializeField] GameObject m_infectedPrefab;
        [SerializeField] GameObject m_explosionPrefab;        
        [SerializeField] float m_speed;
        [SerializeField] float m_time;

        private bool m_hasExplosed;        
        private int m_infectedAmount;
        private float m_countDownTime = 0f;

        private GameObject m_missle;
        private GameObject m_explosion;

        private List<Transform> m_infectedList = new List<Transform>();
        private List<Vector3> m_infectedTargetList = new List<Vector3>();        

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isCasting) return;
            if (m_hasExplosed)
            {
                if (m_countDownTime < m_time)
                {
                    m_countDownTime += Time.deltaTime;
                }
                else
                {
                    m_isCasting = false;
                    for (int i = 0; i < m_infectedList.Count; i++)
                    {
                        Destroy(m_infectedList[i].gameObject, 0.5f);
                    }
                    Destroy(gameObject, 0.5f);
                }                
            }
            else if (Vector3.Distance(m_missle.transform.position, m_targetTable.transform.position) > 0.1f)
            {
                m_missle.transform.position = Vector3.MoveTowards(m_missle.transform.position, m_targetTable.transform.position, m_speed * Time.deltaTime);
            }
            else
            {                
                m_missle.transform.position = m_targetTable.transform.position;
                m_explosion.GetComponent<ParticleSystem>().Play();
                m_hasExplosed = true;

                for (int i = 0; i < m_infectedAmount; i++)
                {
                    var infected = Instantiate(m_infectedPrefab,transform);
                    infected.transform.position = m_infectedTargetList[i];
                    infected.transform.SetParent(transform, false);                    
                    m_infectedList.Add(infected.transform);
                }
            }            
        }

        public virtual void StartSkill(List<byte[]> skillTargets, Vector3 skillStart, bool isPlayerCast = true)
        {            
            m_isCasting = true;
            m_infectedAmount = skillTargets.Count;
            m_targetTable = (isPlayerCast) ? GameManagement.Instance.OpponentTable : GameManagement.Instance.PlayerTable;

            for (int i = 0; i < m_infectedAmount; i++)
            {
                if (m_targetTable.Table[skillTargets[i][0], skillTargets[i][1]] == null)
                {
                    m_infectedTargetList.Add(Vector3.zero);
                    continue;
                }                
                var animalButton = m_targetTable.Table[skillTargets[i][0], skillTargets[i][1]];
                m_infectedTargetList.Add(animalButton.transform.position);
            }

            // Instantiate missle prefab
            m_missle = Instantiate(m_misslePrefab, transform);
            m_missle.transform.SetParent(transform, false);
            m_missle.transform.position = skillStart;

            // Instantiate explosetion prefab
            m_explosion = Instantiate(m_explosionPrefab, transform);
            m_explosion.transform.SetParent(transform, false);
            m_explosion.transform.localPosition = new Vector3(0, 0, -50f);
            m_explosion.GetComponent<ParticleSystem>().Pause();
        }
    }
}