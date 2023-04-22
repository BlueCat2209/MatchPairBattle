using UnityEngine;

namespace UI
{
    public class LoginScreen : UIScreen
    {
        [Header("LOGIN PROPERTIES")]
        [SerializeField] TMPro.TMP_InputField m_nameInput;        

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void OnLoginButtonPressed()
        {            
            Network.PhotonManager.Instance.LoadingForLogin(m_nameInput.text);
        }
    }
}