using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementSkill
{
    public enum SkillType
    {
        Fire = 1,
        Ice = 2,
        Wood = 3,
        Earth = 4,
        Air = 5
    }

    public class Skill : MonoBehaviour
    {
        [Header("BASIC PROPERTIES")]
        [SerializeField] protected SkillType m_skillType;
        protected Pikachu.PikachuTable m_targetTable;
        protected bool m_isCasting;
        protected bool m_isRelease;

        public SkillType SkillType => m_skillType;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void StartSkill()
        {
            m_isCasting = true;
        }
    }
}
