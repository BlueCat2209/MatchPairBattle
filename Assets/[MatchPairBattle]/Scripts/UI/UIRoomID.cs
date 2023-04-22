using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRoomID : MonoBehaviour
    {
        [SerializeField] string m_roomID;
        [SerializeField] Button m_roomIDButton;
        [SerializeField] TMPro.TextMeshProUGUI m_roomIDText;

        public string RoomID
        {
            get => m_roomID;
            set => m_roomID = value;
        }
        public Action<string> RoomSelectedCallbacks;

        // Start is called before the first frame update
        void Start()
        {
            m_roomIDText.text = m_roomID;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnRoomSelected()
        {
            m_roomIDButton.interactable = false;
            RoomSelectedCallbacks?.Invoke(m_roomID);
        }
    }
}