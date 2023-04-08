using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pikachu
{
    public class AnimalButton : MonoBehaviour
    {
        [SerializeField] AnimalType m_type;
        private PikachuTable m_PikachuTable;

        public enum AnimalType 
        {
            None = 0,
            Fire = 1, 
            Ice  = 2, 
            Wood = 3, 
            Earth= 4, 
            Air  = 5 
        }                
        public AnimalType Type => m_type;

        public RectTransform m_RectTransform;
        public Image m_Image;

        public bool m_IsObstacle;
        public int x;
        public int y;

        // Start is called before the first frame update
        void Start()
        {            
            m_RectTransform = GetComponent<RectTransform>();
            m_PikachuTable = GetComponentInParent<PikachuTable>();
        }

        public void OnButtonClicked()
        {
            m_PikachuTable.OnButtonClicked(this.gameObject.GetComponent<AnimalButton>());
        }
        public void OnHideButton()
        {
            m_IsObstacle = false;
            m_Image.enabled = false;
            m_type = AnimalType.None;
            this.gameObject.SetActive(false);
            this.GetComponent<Button>().interactable = false;            
        }
    }
}