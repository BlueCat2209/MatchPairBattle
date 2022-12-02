using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pikachu
{
    public class AnimalButton : MonoBehaviour
    {
        private PikachuManagement m_PikachuManagement;
        public enum AnimalType 
        { 
            BlackCat, BrownCat, GreyCat, OrangeCat, WhiteCat,
            BlackDuck, BlueDuck, GreenDuck, WhiteDuck, YellowDuck,
            BlackPig, BluePig, BrownPig, PinkPig, WhitePig,
            Special
        }        
        public AnimalType type;

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
            m_PikachuManagement.OnButtonClicked(this.gameObject.GetComponent<AnimalButton>());
        }  
    }
}