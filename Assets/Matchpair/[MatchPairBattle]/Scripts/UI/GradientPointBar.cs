using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GradientPointBar : MonoBehaviour
    {
        [SerializeField] Gradient m_gradientColor;
        [SerializeField] Image m_colorImage;              

        // Update is called once per frame
        void Update()
        {            
            m_colorImage.color = m_gradientColor.Evaluate(m_colorImage.fillAmount);
        }
    }
}