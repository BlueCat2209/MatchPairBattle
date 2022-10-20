using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pikachu
{
    public class ColorButton : MonoBehaviour
    {
        [SerializeField] PikachuManagement m_PikachuManagement;

        public enum ColorType { red, yellow, green, mint, blue, darkblue, violet, pink }
        public ColorType color;

        public RectTransform m_RectTransform;
        public Image m_Image;

        public bool m_IsObstacle;
        public int x;
        public int y;

        // Start is called before the first frame update
        void Start()
        {
            m_IsObstacle = true;
            m_PikachuManagement = FindObjectOfType<PikachuManagement>();
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void OnButtonClicked()
        {
            m_PikachuManagement.OnButtonClicked(this.gameObject.GetComponent<ColorButton>());
        }
    }
}