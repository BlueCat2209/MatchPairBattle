using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Pikachu;
using Network;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManagement : MonoBehaviour
{
    [Header("BASIC PROPERTIES")]
    [SerializeField] PikachuTable m_playerTable;
    [SerializeField] PikachuTable m_opponentTable;
    [SerializeField] float m_targetPoint;
    [SerializeField] float m_playerPoint;
    [SerializeField] float m_opponentPoint;

    [Header("UI PROPERTIES")]
    [SerializeField] CanvasGroup m_playerPanel;    
    [SerializeField] RectTransform m_playerResultPanel;
    [Space]
    [SerializeField] CanvasGroup m_opponentPanel;    
    [SerializeField] RectTransform m_opponentResultPanel;

    [Header("ELEMENTS PROPERTIES")]
    [SerializeField] int m_stackAmount;
    [Header("Ice")]
    [SerializeField] Image m_iceElement;
    [SerializeField] Button m_iceSkill;
    [Header("Air")]
    [SerializeField] Image m_airElement;
    [SerializeField] Button m_airSkill;
    [Header("Wood")]
    [SerializeField] Image m_woodElement;
    [SerializeField] Button m_woodSkill;
    [Header("Fire")]
    [SerializeField] Image m_fireElement;
    [SerializeField] Button m_fireSkill;
    [Header("Earth")]
    [SerializeField] Image m_earthElement;
    [SerializeField] Button m_earthSkill;

    public static GameManagement Instance => m_instance;
    private static GameManagement m_instance;
    private bool m_isBeingStealed = false; 

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;
    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }
    }
    private void Update()
    {
        if (m_playerTable.CheckTableEmpty())
        {
            m_playerTable.CreatePlayerTable();
        }
    }

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
    }
    protected virtual void SetOpponentPairData(object[] data)
    {        
        // Extract data
        Vector2 startCor = (Vector2)data[0];
        Vector2 endCor = (Vector2)data[1];
        AnimalButton.AnimalType type = (AnimalButton.AnimalType)((byte)data[2]);
        
        // Process data
        AnimalButton start = m_opponentTable.m_table[(int)startCor.x, (int)startCor.y];
        AnimalButton end = m_opponentTable.m_table[(int)endCor.x, (int)endCor.y];
        m_opponentTable.HidePair(start, end);
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
        int row = (int)m_playerTable.m_tableSize.x;
        int column = (int)m_playerTable.m_tableSize.y;
        Vector2 tableSize = new Vector2(row, column);
            
        // Prepare table data
        List<byte[]> tableCode = new List<byte[]>();        
        for (int i = 0; i < row; i++)
        {
            byte[] tmp = new byte[column];
            for (int j = 0; j < column; j++)
            {
                tmp[j] = (byte)m_playerTable.m_table[i, j].type;
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
        CheckGameFinished();
    }
    private void CheckGameFinished()
    {
        if (m_playerPoint == m_targetPoint)
        {
            m_playerResultPanel.gameObject.SetActive(true);
            m_playerResultPanel.GetChild(0).gameObject.SetActive(true);

            m_opponentResultPanel.gameObject.SetActive(true);
            m_opponentResultPanel.GetChild(1).gameObject.SetActive(true);
        }
        if (m_opponentPoint == m_targetPoint)
        {
            m_playerResultPanel.gameObject.SetActive(true);
            m_playerResultPanel.GetChild(1).gameObject.SetActive(true);

            m_opponentResultPanel.gameObject.SetActive(true);
            m_opponentResultPanel.GetChild(0).gameObject.SetActive(true);
        }
    }

    #region Special Skills
    public void CastFireSkill()
    {
        m_fireElement.fillAmount = 0;
        m_fireSkill.interactable = false;        
    }
    public void CastIceSkill()
    {
        m_iceElement.fillAmount = 0;
        m_iceSkill.interactable = false;
    }
    public void CastWoodSkill()
    {
        m_woodElement.fillAmount = 0;
        m_woodSkill.interactable = false;
    }
    public void CastEarthSkill()
    {
        m_earthElement.fillAmount = 0;
        m_earthSkill.interactable = false;
    }
    public void CastAirSkill()
    {
        m_airElement.fillAmount = 0;
        m_airSkill.interactable = false;
    }
    #endregion

    #region Coroutine

    #endregion               
}
