using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pikachu
{
    public enum ElementalType
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Wood = 3,
        Earth = 4,
        Air = 5,        
    }

    public class AnimalButton : MonoBehaviour
    {
        [SerializeField] ElementalType m_type;
        [SerializeField] bool m_isObstacle;

        [SerializeField] int m_coordinateX;
        [SerializeField] int m_coordinateY;

        private Image m_image;
        private PikachuTable m_pikachuTable;
        private RectTransform m_rectTransform;
        
        public ElementalType Type
        {
            get => m_type;
            set => m_type = value;            
        }
        public bool IsObstacle
        {
            get => m_isObstacle;
            set => m_isObstacle = value;
        }

        public Image Image
        {
            get
            {
                if (m_image == null)
                {
                    m_image = GetComponentInChildren<Image>();
                }
                return m_image;
            }
            set => m_image = value;
        }
        public PikachuTable PikachuTable
        {
            get
            {
                if (m_pikachuTable == null)
                {
                    m_pikachuTable = GetComponentInParent<PikachuTable>();
                }
                return m_pikachuTable;
            }
            set => m_pikachuTable = value;
        }
        public RectTransform RectTransform
        {
            get
            {
                if (m_rectTransform == null)
                {
                    m_rectTransform = GetComponent<RectTransform>();
                }
                return m_rectTransform;
            }
            set => m_rectTransform = value;
        }                

        public int CoordinateX
        {
            get => m_coordinateX;
            set => m_coordinateX = value;
        }
        public int CoordinateY
        {
            get => m_coordinateY;
            set => m_coordinateY = value;
        }

        // Start is called before the first frame update
        void Awake()
        {            
        }

        [ContextMenu("Click button")]
        public void OnButtonClicked()
        {
            PikachuTable.OnButtonClicked(this.gameObject.GetComponent<AnimalButton>());
        }

        [ContextMenu("Hide button")]
        public void OnHideButton()
        {
            m_isObstacle = false;
            m_type = ElementalType.None;
            Image.color = new Color(0, 0, 0, 0);
            
            this.GetComponent<Button>().interactable = false;
        }
    }
}