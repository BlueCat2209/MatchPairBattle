using Pikachu;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace ElementSkill
{

    public class Fire : Skill
    {
        [Header("SKILL PROPERTIES")]
        [SerializeField] private GameManagement m_skill;
        [SerializeField] private PikachuTable m_PiKaChuTable;
        [SerializeField] private List<AnimalButton> FireList = new List<AnimalButton>();
        [SerializeField] private GameObject fire;
        [SerializeField] private Transform transform;
        void Update()
        {
            if (m_isCasting)
            {
                m_skill.m_animator.SetTrigger("Cast");
            }
        }

        //Tinh toan so button con lai tren table, sau do tra ve random AnimalType con du
        private AnimalButton.AnimalType CalculateType(List<AnimalButton> button)
        {
            //Tinh so button va phan loai type con du
            var typeCounts = new Dictionary<AnimalButton.AnimalType, int>();
            foreach (AnimalButton butto in button)
            {
                if (butto.gameObject.activeSelf)
                {
                    if (typeCounts.ContainsKey(butto.Type) && butto.Type != AnimalButton.AnimalType.None)
                    {
                        typeCounts[butto.Type]++;
                    }
                    else
                    {
                        typeCounts[butto.Type] = 1;
                    }
                }
            }
            // So type tren 6 cai
            var typesWithCountGreaterThanSix = new List<AnimalButton.AnimalType>();
            foreach (var type in typeCounts.Keys)
            {
                if (typeCounts[type] >= 6)
                {
                    typesWithCountGreaterThanSix.Add(type);
                }
            }
            //Tra ve randomtype
            if (typesWithCountGreaterThanSix.Count > 0)
            {
                var randomIndex = Random.Range(0, typesWithCountGreaterThanSix.Count);
                var randomType = typesWithCountGreaterThanSix[randomIndex];
                return randomType;
            }
            //Khong co type nao tren 6
            else
            {
                return AnimalButton.AnimalType.None;
            }


        }

        public void FireSkill()
        {
            //Add type vao firelist
            var targetType = CalculateType(m_PiKaChuTable.m_buttonList);

            foreach (AnimalButton button in m_PiKaChuTable.m_buttonList)
            {
                if (button.Type == targetType && button.gameObject.activeSelf)
                {
                    FireList.Add(button);
                }
            }

            //Random Type trong FireList
            RandomizeList(FireList);

            //Instantiate FireBall
            for (int i = 0; i < 6; i++)
            {
                var skill = Instantiate(fire, Vector3.zero, Quaternion.identity, m_skill.m_playerTable.transform);
                skill.transform.localPosition = m_PiKaChuTable.m_table[FireList[i].x, FireList[i].y].transform.localPosition;

                // Add speed to fireball
                var rb = skill.AddComponent<Rigidbody>();
                if (rb != null)
                {

                    Vector3 direction = m_PiKaChuTable.m_table[FireList[i].x, FireList[i].y].transform.localPosition;

                    // Destroy the button after the fireball reaches it
                    StartCoroutine(DestroyButtonAfterTime(1f, m_PiKaChuTable.m_table[FireList[i].x, FireList[i].y]));
                }
            }
            //Xoa sach FireList
            FireList.Clear();

        }

        IEnumerator DestroyButtonAfterTime(float timeToDestroy, AnimalButton button)
        {
            yield return new WaitForSeconds(timeToDestroy);

            // Deactivate the button
            button.OnHideButton();
        }

        private void RandomizeList(List<AnimalButton> list)
        {
            // Shuffle the list using Fisher-Yates shuffle algorithm
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                AnimalButton temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}

