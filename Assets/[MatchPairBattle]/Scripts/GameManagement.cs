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
    [SerializeField] Image m_fireImage;
    [SerializeField] Button m_fireButton;
    [SerializeField] GameObject m_fireRainPrefab;

    [Header("Ice")]
    [SerializeField] Image m_iceImage;
    [SerializeField] Button m_iceButton;
    [SerializeField] GameObject m_iceFreezePrefab;

    [Header("Wood")]
    [SerializeField] Image m_woodImage;
    [SerializeField] Button m_woodButton;
    [SerializeField] GameObject m_woodExplosionPrefab;

    [Header("Earth")]
    [SerializeField] Image m_earthImage;
    [SerializeField] Button m_earthButton;
    [SerializeField] GameObject m_earthquakePrefab;

    [Header("Air")]
    [SerializeField] Image m_airImage;
    [SerializeField] Button m_airButton;
    [SerializeField] GameObject m_tornadoPrefab;
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
                SetTableData(data);
                break;
            case PhotonEventCode.TransferPairData:
                SetOpponentPairData(data);
                break;
            case PhotonEventCode.TransferSkill:
                SetOpponentSkill(data);
                break;
        }
    }
    protected virtual void SetTableData(object[] data)
    {
        // Get information about the table data        
        Vector2 tableSize = (Vector2)data[0];

        // Extract table data
        int column = (int)tableSize.x;
        int row = (int)tableSize.y;
        byte[] columnData = new byte[row];
        byte[,] tableData = new byte[column, row];        
        
        for (int i = 0; i < column; i++)
        {
            columnData = (byte[])data[i + 1];
            for (int j = 0; j < row; j++)
            {
                tableData[i, j] = columnData[j];
            }
        }

        // Create table from extracted data        
        m_opponentTable.CreateTable(tableData, tableSize);
        m_currentStatus = GameStatus.PLAYING;
    }
    protected virtual void SetOpponentPairData(object[] data)
    {        
        // Extract data
        Vector2 startCor = (Vector2)data[0];
        Vector2 endCor = (Vector2)data[1];
        AnimalType type = (AnimalType)((byte)data[2]);
        
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
                {
                    var skillTargets = new List<byte[]>();
                    for (int i = 1; i < data.Length; i++)
                    {
                        skillTargets.Add((byte[])data[i]);
                    }
                    HitFireSkill(skillTargets);
                }
                break;
            case SkillType.Ice:
                {
                    HitIceSkill();
                }
                break;
            case SkillType.Wood:
                {
                    var skillTargets = new List<byte[]>();
                    for (int i = 1; i < data.Length; i++)
                    {
                        skillTargets.Add((byte[])data[i]);
                    }
                    HitWoodSkill(skillTargets);
                }
                break;
            case SkillType.Earth:
                {
                    HitEarthSkill();   
                }
                break;
            case SkillType.Air:
                {
                    var tableSize = (Vector2)data[1];
                    var columnData = new byte[(int)tableSize.y];
                    var tableData = new byte[(int)tableSize.x, (int)tableSize.y];
                    for (int i = 0; i < tableSize.x; i++)
                    {
                        columnData = (byte[])data[i + 2];
                        for (int j = 0; j < tableSize.y; j++)
                        {
                            tableData[i, j] = columnData[j];
                        }
                    }
                    HitAirSkill(tableData, tableSize);
                }
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
            case SkillType.Wood:
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

            case SkillType.Air:
                {
                    var skillData = (object[])includedData;
                    dataSend = new object[skillData.Length + 1];
                    dataSend[0] = skillCode;

                    for (int i = 1; i < dataSend.Length; i++)
                    {
                        dataSend[i] = skillData[i - 1];
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
    public void SendPlayerPairData(AnimalButton start, AnimalButton end, AnimalType type)
    {        
        CalculatePlayerSkill(type);

        // Prepare start & end Cordinates
        Vector2 startCor = new Vector2(start.CoordinateX, start.CoordinateY);
        Vector2 endCor = new Vector2(end.CoordinateX, end.CoordinateY);        
        
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
    public void SendTableData(AnimalButton[,] tableData, Vector2 tableSize, bool isPlayerTableData = true)
    {
        // Prepare table size
        int column = (int)tableSize.x;
        int row = (int)tableSize.y;

        // Prepare table data
        object[] dataSend = new object[column + 1];        
        for (int i = 0; i < column; i++)
        {
            byte[] columnData = new byte[row];
            for (int j = 0; j < row; j++)
            {
                columnData[j] = (byte)tableData[i, j].Type;
            }
            dataSend[i + 1] = columnData;
        }

        // Packing data        
        dataSend[0] = tableSize;

        if (isPlayerTableData)
        {
            // Select other client to receive this data
            RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            // Transfer data
            PhotonNetwork.RaiseEvent(
                (byte)PhotonEventCode.TransferTableData,
                dataSend,
                targetsOption,
                SendOptions.SendUnreliable
                );
        }
        else
        {
            Debug.LogWarning("Send Skill");
            SendPlayerSkill((byte)SkillType.Air, dataSend);
        }
    }
    #endregion

    private void CalculatePlayerSkill(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.Fire:
                m_fireButton.interactable = (m_fireImage.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_fireImage.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalType.Ice:
                m_iceButton.interactable = (m_iceImage.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_iceImage.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalType.Earth:
                m_earthButton.interactable = (m_earthImage.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_earthImage.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalType.Wood:
                m_woodButton.interactable = (m_woodImage.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_woodImage.fillAmount += 1f / m_stackAmount;
                break;
            case AnimalType.Air:
                m_airButton.interactable = (m_airImage.fillAmount + 1f / m_stackAmount >= 1f) ? true : false;
                m_airImage.fillAmount += 1f / m_stackAmount;
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
        m_fireImage.fillAmount = 0;
        m_fireButton.interactable = false;        

        var targetButtonsType = new List<byte[]>();
        for (int i = 1; i <= 6; i++)
        {
            var currentButtonsType = m_playerTable.GetAnimalTypeCoordinateList((AnimalType)i);
            if (targetButtonsType.Count < currentButtonsType.Count)
            {
                targetButtonsType = currentButtonsType;
            }
        }
        var skill = Instantiate(m_fireRainPrefab, m_playerTable.transform, false);
        skill.transform.localPosition = Vector3.zero;

        SendPlayerSkill((int)SkillType.Fire, targetButtonsType);
        
        skill.GetComponent<Fire>().StartSkill(targetButtonsType, m_playerAvatar.position);
    }
    public void CastIceSkill()
    {
        m_iceImage.fillAmount = 0;
        m_iceButton.interactable = false;

        var skill = Instantiate(m_iceFreezePrefab, m_opponentTable.transform);

        SendPlayerSkill((int)SkillType.Ice);
        skill.GetComponent<Ice>().StartSkill();
    }
    public void CastWoodSkill()
    {
        m_woodImage.fillAmount = 0;
        m_woodButton.interactable = false;

        var targetButtonsType = new List<byte[]>();
        for (int i = 1; i <= 6; i++)
        {
            var currentButtonsType = m_opponentTable.GetAnimalTypeCoordinateList((AnimalType)i);
            if (targetButtonsType.Count < currentButtonsType.Count)
            {
                targetButtonsType = currentButtonsType;
            }
        }

        //var skill = Instantiate(m_woodExplosionPrefab, m_playerTable.transform, false);
        var skill = Instantiate(m_woodExplosionPrefab, m_opponentTable.transform, false);
        skill.transform.localPosition = Vector3.zero;

        SendPlayerSkill((int)SkillType.Wood, targetButtonsType);
        skill.GetComponent<Wood>().StartSkill(targetButtonsType, m_playerAvatar.transform.position);
    }
    public void CastEarthSkill()
    {
        m_earthImage.fillAmount = 0;
        m_earthButton.interactable = false;

        var skillTargets = m_opponentTable.GetAnimalTypeButtonList(AnimalType.None);
        var skill = Instantiate(m_earthquakePrefab, m_opponentTable.transform, false);
        skill.transform.localPosition = Vector3.zero;

        SendPlayerSkill((int)SkillType.Earth);
        skill.GetComponent<Earth>().StartSkill(skillTargets, m_opponentTable.TableSize);
    }
    public void CastAirSkill()
    {
        m_airImage.fillAmount = 0;
        m_airButton.interactable = false;

        var skill = Instantiate(m_tornadoPrefab, m_opponentTable.transform, false);
        var skillStartPosition = m_opponentTable.transform.InverseTransformPoint(m_playerAvatar.position);

        skill.GetComponent<Air>().StartSkillToOpponent(skillStartPosition);
    }
    #endregion

    #region Hit Skills
    public void HitFireSkill(List<byte[]> buttonTargets)
    {        
        var skill = Instantiate(m_fireRainPrefab, m_opponentTable.transform);        
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
    public void HitWoodSkill(List<byte[]> buttonTargets)
    {
        var skill = Instantiate(m_woodExplosionPrefab, m_playerTable.transform, false);
        skill.transform.localPosition = Vector3.zero;
        skill.GetComponent<Wood>().StartSkill
        (
            skillTargets: buttonTargets,
            skillStart: m_opponentTable.transform.TransformPoint(Vector3.zero),
            isPlayerCast: false
        );
    }
    public void HitEarthSkill()
    {
        var skillTargets = m_playerTable.GetAnimalTypeButtonList(AnimalType.None);
        var skill = Instantiate(m_earthquakePrefab, m_playerTable.transform, false);
        skill.transform.localPosition = Vector3.zero;
        
        skill.GetComponent<Earth>().StartSkill(skillTargets, m_playerTable.TableSize);
    }
    public void HitAirSkill(byte[,] tableData, Vector2 tableSize)
    {
        // Delay player to use skill

        var firePosition = m_fireButton.transform.localPosition;
        var delayOnFire = Instantiate(m_tornadoPrefab, m_fireButton.transform.parent);
        delayOnFire.transform.localPosition = new Vector3(firePosition.x, firePosition.z, -10);
        Destroy(delayOnFire, 3f);

        var icePosition = m_iceButton.transform.localPosition;
        var delayOnIce = Instantiate(m_tornadoPrefab, m_iceButton.transform.parent);        
        delayOnIce.transform.localPosition = new Vector3(icePosition.x, icePosition.y, -10);
        Destroy(delayOnIce, 3f);

        var woodPosition = m_woodButton.transform.localPosition;
        var delayOnWood = Instantiate(m_tornadoPrefab, m_woodButton.transform.parent);
        delayOnWood.transform.localPosition = new Vector3(woodPosition.x, woodPosition.z, -10);
        Destroy(delayOnWood, 3f);

        var earthPosition = m_earthButton.transform.localPosition;
        var delayOnEarth = Instantiate(m_tornadoPrefab, m_earthButton.transform.parent);
        delayOnEarth.transform.localPosition = new Vector3(earthPosition.x, earthPosition.z, -10);
        Destroy(delayOnEarth, 3f);

        var airPosition = m_airButton.transform.localPosition;
        var delayOnAir = Instantiate(m_tornadoPrefab, m_airButton.transform.parent);
        delayOnAir.transform.localPosition = new Vector3(airPosition.x, airPosition.z, -10);
        Destroy(delayOnAir, 3f);


        // Create a tornado to shuffle the player's table
        var skill = Instantiate(m_tornadoPrefab, m_playerTable.transform, false);
        var skillStartPosition = m_playerTable.transform.InverseTransformPoint(m_opponentTable.transform.position);                
        
        skill.GetComponent<Air>().StartSkillToPlayer(skillStartPosition, tableData, tableSize);                
    }
    #endregion               
}
