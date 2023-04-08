using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Pikachu;
using Network;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using ElementSkill;
using UnityEngine.U2D.Animation;

public class GameManagement : MonoBehaviour
{
    private static GameManagement m_instance;
    public static GameManagement Instance => m_instance;

    [Header("BASIC PROPERTIES")]
    [Header("Table Properties")]
    [SerializeField] PikachuTable m_playerTable;
    [SerializeField] PikachuTable m_opponentTable;
    public PikachuTable PlayerTable => m_playerTable;
    public PikachuTable OpponentTable => m_opponentTable;

    [Header("Avatar Properties")]
    [SerializeField] Transform m_playerAvatar;
    [SerializeField] public Animator m_animator;
    [SerializeField] public SpriteLibrary m_spriteLibrary;

    [Header("PlayTime Properties")]
    [SerializeField] float m_targetTime;
    [SerializeField] Image m_countDownImage;
    [SerializeField] Gradient m_gradientColor;    
    private float m_currentTime;
    [Space]

    [Header("UI PROPERTIES")]
    [SerializeField] CanvasGroup m_playerPanel;            
    [SerializeField] CanvasGroup m_opponentPanel;
    [Space]
    [SerializeField] Canvas m_endGameCanvas;
    [SerializeField] TextMeshProUGUI m_textScore;
    [SerializeField] TextMeshProUGUI m_textResult;
    [Space]

    [Header("ELEMENTS PROPERTIES")]    
    [SerializeField] int m_stackAmount;    

    [Header("Fire")]
    [SerializeField] Image m_fireElement;
    [SerializeField] Button m_fireSkill;
    [SerializeField] GameObject m_fireskillfrefab;

    [Header("Ice")]
    [SerializeField] Image m_iceElement;
    [SerializeField] Button m_iceSkill;
    [SerializeField] GameObject m_iceFreezePrefab;

    [Header("Wood")]
    [SerializeField] Image m_woodElement;
    [SerializeField] Button m_woodSkill;

    [Header("Earth")]
    [SerializeField] Image m_earthElement;
    [SerializeField] Button m_earthSkill;

    [Header("Air")]
    [SerializeField] Image m_airElement;
    [SerializeField] Button m_airSkill;
    [Space]

    [SerializeField] GameStatus m_currentStatus = GameStatus.INITIALIZING;
    public enum GameStatus { VICTORY, DEFEATED, DRAW, PLAYING, INITIALIZING }
    public int FinalPoint => (m_playerTable.ButtonAmountDefault - m_playerTable.CurrentButtonAmount) + (int)(m_targetTime - m_currentTime);                

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;

    #region Unity Message
    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }
    }
    private void Start()
    {
        m_playerTable.CreatePlayerTable();
    }
    private void Update()
    {
        if (m_currentStatus != GameStatus.PLAYING) return;

        if (m_currentTime < m_targetTime)
        {
            m_currentTime += Time.deltaTime;
            m_countDownImage.fillAmount = 1f - m_currentTime / m_targetTime;
            m_countDownImage.color = m_gradientColor.Evaluate(m_currentTime / m_targetTime);

            if (m_playerTable.IsTableEmpty) SetGameFinish(GameStatus.VICTORY);
            if (m_opponentTable.IsTableEmpty) SetGameFinish(GameStatus.DEFEATED);
        }
        else
        {
            if (m_playerTable.CurrentButtonAmount < m_opponentTable.CurrentButtonAmount)
            {
                SetGameFinish(GameStatus.VICTORY);
            }
            else
            if (m_playerTable.CurrentButtonAmount > m_opponentTable.CurrentButtonAmount)
            {
                SetGameFinish(GameStatus.DEFEATED);
            }
            else
            {
                SetGameFinish(GameStatus.DRAW);
            }            
        }    
    }
    #endregion

    #region Photon Event
    protected virtual void OnEventReceive(EventData _dataReceive)
    {
        // Import data
        object[] data = (object[])_dataReceive.CustomData;
        // Unpackage data
        switch ((PhotonEventCode) _dataReceive.Code)
        {
            case PhotonEventCode.TransferTableData:
                SetOpponentTableData(data);
                break;
            case PhotonEventCode.TransferPairData:
                SetOpponentPairData(data);
                break;
            case PhotonEventCode.TransferSkill:
                SetOpponentSkill(data);
                break;
        }
    }
    protected virtual void SetOpponentTableData(object[] data)
    {
        // Get table size
        Vector2 tableSize = (Vector2)data[0];
        int row = (int)tableSize.x; int column = (int)tableSize.y;
        
        // Get table data
        byte[,] tableCode = new byte[row, column];
        for (int i = 1; i < data.Length; i++)
        {
            byte[] tmp = (byte[])data[i];
            for (int j = 0; j < tmp.Length; j++)
            {
                tableCode[i - 1, j] = tmp[j];
            }
        }

        // Create table
        m_opponentTable.CreateTable(tableCode, tableSize);
        m_currentStatus = GameStatus.PLAYING;   
    }
    protected virtual void SetOpponentPairData(object[] data)
    {        
        // Extract data
        Vector2 startCor = (Vector2)data[0];
        Vector2 endCor = (Vector2)data[1];
        AnimalButton.AnimalType type = (AnimalButton.AnimalType)((byte)data[2]);
        
        // Process data
        AnimalButton start = m_opponentTable.Table[(int)startCor.x, (int)startCor.y];
        AnimalButton end = m_opponentTable.Table[(int)endCor.x, (int)endCor.y];
        m_opponentTable.HidePair(start, end);
    }
    protected virtual void SetOpponentSkill(object[] data)
    {
        // Extract data
        byte skillCode = (byte)data[0];

        // Process data
        switch ((SkillType)skillCode)
        {
            case SkillType.Fire:
                var skillTargets = new List<byte[]>();
                for (int i = 1; i < data.Length; i++)
                {
                    skillTargets.Add((byte[])data[i]);
                }
                HitFireSkill(skillTargets);
                break;
            case SkillType.Ice:
                HitIceSkill();
                break;
            case SkillType.Wood:
                HitWoodSkill();
                break;
            case SkillType.Earth:
                HitEarthSkill();
                break;
            case SkillType.Air:
                HitAirSkill();
                break;
        }
    }

    public void SendPlayerSkill(byte skillCode, object includedData = null)
    {
        // Package data
        object[] dataSend;
        switch ((SkillType)skillCode)
        {
            case SkillType.Fire:
                {
                    var targetList = (List<byte[]>)includedData;
                    dataSend = new object[targetList.Count + 1];
                    dataSend[0] = skillCode;

                    for (int i = 0; i < targetList.Count; i++)
                    {
                        dataSend[i + 1] = targetList[i];
                    }
                }                
                break;

            default:
                {
                    dataSend = new object[] { skillCode };
                }
                break;
        }        
        

        // Select other client to receive this data
        RaiseEventOptions targetOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // Transfer data
        PhotonNetwork.RaiseEvent(
            (byte)PhotonEventCode.TransferSkill,
            dataSend,
            targetOption,
            SendOptions.SendUnreliable
            );
    }
    public void SendPlayerPairData(AnimalButton start, AnimalButton end, AnimalButton.AnimalType type)
    {        
        CalculatePlayerSkill(type);

        // Prepare start & end Cordinates
        Vector2 startCor = new Vector2(start.x, start.y);
        Vector2 endCor = new Vector2(end.x, end.y);        
        
        // Package data
        object[] dataSend = new object[] { startCor, endCor, (byte) type};

        // Select other client to receive this data
        RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // Transfer data
        PhotonNetwork.RaiseEvent(
            (byte) PhotonEventCode.TransferPairData,
            dataSend,
            targetsOption,
            SendOptions.SendUnreliable
            );
    }
    public void SendPlayerTableData()
    {
        // Prepare table size
        int row = (int)m_playerTable.TableSize.x;
        int column = (int)m_playerTable.TableSize.y;
        Vector2 tableSize = new Vector2(row, column);
            
        // Prepare table data
        List<byte[]> tableCode = new List<byte[]>();        
        for (int i = 0; i < row; i++)
        {
            byte[] tmp = new byte[column];
            for (int j = 0; j < column; j++)
            {
                tmp[j] = (byte)m_playerTable.Table[i, j].Type;
            }
            tableCode.Add(tmp);            
        }


        // Package data
        object[] dataSend = new object[row + 1];
        dataSend[0] = tableSize;
        for (int i = 1; i < dataSend.Length; i++)
        {
            dataSend[i] = tableCode[i - 1];
        }

        // Select other client to receive this data
        RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // Transfer data
        PhotonNetwork.RaiseEvent(
            (byte) PhotonEventCode.TransferTableData,
            dataSend,
            targetsOption,
            SendOptions.SendUnreliable
            );
    }
    #endregion

    private void CalculatePlayerSkill(AnimalButton.AnimalType type)
    {
        switch (type)
        {
            case AnimalButton.AnimalType.Fire:
                m_fireSkill.interactable = (m_fireElement.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_fireElement.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalButton.AnimalType.Ice:
                m_iceSkill.interactable = (m_iceElement.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_iceElement.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalButton.AnimalType.Earth:
                m_earthSkill.interactable = (m_earthElement.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_earthElement.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalButton.AnimalType.Wood:
                m_woodSkill.interactable = (m_woodElement.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_woodElement.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalButton.AnimalType.Air:
                m_airSkill.interactable = (m_airElement.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_airElement.fillAmount += 1f / m_stackAmount;
                break;
        }        
    }
    private void SetGameFinish(GameStatus gameResult)
    {
        m_currentStatus = gameResult;
        switch (gameResult)
        {
            case GameStatus.VICTORY:
                {
                    m_endGameCanvas.gameObject.SetActive(true);
                    m_textScore.text = "Score: " + FinalPoint;
                    m_textResult.text = "VICTORY";                    
                }
            break;

            case GameStatus.DEFEATED:
                {
                    m_endGameCanvas.gameObject.SetActive(true);
                    m_textScore.text = "Score: " + FinalPoint;
                    m_textResult.text = "DEFEATED";                    
                }
            break;

            case GameStatus.DRAW:
                {
                    m_endGameCanvas.gameObject.SetActive(true);
                    m_textScore.text = "Score: " + FinalPoint;
                    m_textResult.text = "DRAW";                    
                }
                break;
        }
    }

    #region Cast Skills
    public void CastFireSkill()
    {
        m_fireElement.fillAmount = 0;
        m_fireSkill.interactable = false;

        var targetButtonsType = new List<byte[]>();
        for (int i = 1; i <= 6; i++)
        {
            var currentButtonsType = m_playerTable.GetAnimalTypeList((AnimalButton.AnimalType)i);
            if (targetButtonsType.Count < currentButtonsType.Count)
            {
                targetButtonsType = currentButtonsType;
            }
        }
        var skill = Instantiate(m_fireskillfrefab, m_playerTable.transform);

        SendPlayerSkill((int)SkillType.Fire, targetButtonsType);
        skill.GetComponent<Fire>().StartSkill(targetButtonsType, m_playerAvatar.position);        
    }
    public void CastIceSkill()
    {
        m_iceElement.fillAmount = 0;
        m_iceSkill.interactable = false;

        var skill = Instantiate(m_iceFreezePrefab, m_opponentTable.transform);

        SendPlayerSkill((int)SkillType.Ice);
        skill.GetComponent<Ice>().StartSkill();
    }
    public void CastWoodSkill()
    {
        m_woodElement.fillAmount = 0;
        m_woodSkill.interactable = false;

        SendPlayerSkill((int)SkillType.Wood);
    }
    public void CastEarthSkill()
    {
        m_earthElement.fillAmount = 0;
        m_earthSkill.interactable = false;

        SendPlayerSkill((int)SkillType.Earth);
    }
    public void CastAirSkill()
    {
        m_airElement.fillAmount = 0;
        m_airSkill.interactable = false;

        SendPlayerSkill((int)SkillType.Air);
    }
    #endregion

    #region Hit Skills
    public void HitFireSkill(List<byte[]> buttonTargets)
    {        
        var skill = Instantiate(m_fireskillfrefab, m_opponentTable.transform);        
        skill.GetComponent<Fire>().StartSkill
        (
            skillTargets: buttonTargets, 
            skillStart: m_opponentTable.transform.TransformPoint(Vector3.zero),
            isPlayerCast: false
        );
    }
    public void HitIceSkill()
    {
        var skill = Instantiate(m_iceFreezePrefab, m_playerTable.transform);
        skill.GetComponent<Ice>().StartSkill();
    }
    public void HitWoodSkill()
    {

    }
    public void HitEarthSkill()
    {
    
    }
    public void HitAirSkill()
    {
    
    }
    #endregion               
}
