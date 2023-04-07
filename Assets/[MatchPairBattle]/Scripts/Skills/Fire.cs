using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElementSkill
{
    public class Fire : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] GameObject m_firePrefab;
        

        void Update()
        {
            
        }

        public virtual void StartSkill(List<Pikachu.AnimalButton> skillTargets)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i < skillTargets.Count - 1)
                {
                    var target = skillTargets[i].transform.position;
                    var skill = Instantiate(m_firePrefab, transform);
                    skill.transform.position = target;
                    Destroy(skill, 2f);
                }
            }
        }
    }
}

