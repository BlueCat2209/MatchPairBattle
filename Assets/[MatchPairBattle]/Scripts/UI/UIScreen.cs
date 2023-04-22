using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] GameObject m_panel;
        public GameObject Panel => m_panel;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void ShowScreen()
        {
            m_panel.SetActive(true);
        }
        public virtual void HideScreen()
        {            
            m_panel.SetActive(false);
        }
    }
}