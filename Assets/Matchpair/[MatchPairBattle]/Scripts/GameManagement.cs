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
    [SerializeField] PikachuManagement m_pikachuManagement;
    [SerializeField] float m_targetPoint;
    [SerializeField] float m_playerPoint;
    [SerializeField] float m_opponentPoint;

    [Header("UI PROPERTIES")]
    [SerializeField] CanvasGroup m_playerTable;
    [SerializeField] Image m_playerPointBar;
    [SerializeField] RectTransform m_playerResultPanel;
    [Space]
    [SerializeField] CanvasGroup m_opponentTable;
    [SerializeField] Image m_opponentPointBar;
    [SerializeField] RectTransform m_opponentResultPanel;

    [Header("SKILL PROPERTIES")]    
    [Header("Freeze")]
    [SerializeField] GameObject m_freezeImage;
    [Header("Steal")]
    [SerializeField] GameObject m_chestImage;
    [SerializeField] GameObject m_arrowImage;

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;    
    private void Update()
    {
        if (m_pikachuManagement.CheckTableEmpty())
        {
            m_pikachuManagement.CreatePlayerTable();
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
        m_pikachuManagement.CreateOpponentTable(tableCode, tableSize);
    }
    protected virtual void SetOpponentPairData(object[] data)
    {           
        Vector2 startCor = (Vector2)data[0];
        Vector2 endCor = (Vector2)data[1];
        AnimalButton.AnimalType type = (AnimalButton.AnimalType)((byte)data[2]);

        AnimalButton start = m_pikachuManagement.m_opponentTable[(int)startCor.x, (int)startCor.y];
        AnimalButton end = m_pikachuManagement.m_opponentTable[(int)endCor.x, (int)endCor.y];

        CalculateOpponentPoint(type);
        m_pikachuManagement.HidePair(start, end);
    }
    
    public void SendPlayerPairData(AnimalButton start, AnimalButton end, AnimalButton.AnimalType type)
    {
        CalculatePlayerPoint(type);

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
        int row = (int)m_pikachuManagement.m_tableSize.x;
        int column = (int)m_pikachuManagement.m_tableSize.y;
        Vector2 tableSize = new Vector2(row, column);
            
        // Prepare table data
        List<byte[]> tableCode = new List<byte[]>();        
        for (int i = 0; i < row; i++)
        {
            byte[] tmp = new byte[column];
            for (int j = 0; j < column; j++)
            {
                tmp[j] = (byte)m_pikachuManagement.m_playerTable[i, j].type;
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

    private void CalculatePlayerPoint(AnimalButton.AnimalType type)
    {
        switch (type)
        {
            case AnimalButton.AnimalType.Special:
                int skillCode = Random.Range(0, 4);
                switch (skillCode)
                {
                    case 0:
                        CastFreeze();
                        break;
                    case 1:
                        CastFreeze();
                        break;
                    case 2:
                        CastFreeze();
                        break;
                    case 3:
                        CastFreeze();
                        break;
                }
                break;
            default:
                m_playerPoint += 10;
                m_playerPointBar.fillAmount = m_playerPoint / m_targetPoint;
                break;
        }
        CheckGameFinished();
    }
    private void CalculateOpponentPoint(AnimalButton.AnimalType type)
    {
        switch (type)
        {
            case AnimalButton.AnimalType.Special:
                int skillCode = Random.Range(0, 4);
                switch (skillCode)
                {
                    case 0:
                        HitFreeze();
                        break;
                    case 1:
                        HitFreeze();
                        break;
                    case 2:
                        HitFreeze();
                        break;
                    case 3:
                        HitFreeze();
                        break;
                }
                break;
            default:
                m_opponentPoint += 10;
                m_opponentPointBar.fillAmount = m_opponentPoint / m_targetPoint;
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
    // CAST SKILL
    private void CastFreeze()
    {
        // Player effect

        // Opponent effect
        var freezeImage = Instantiate(m_freezeImage, m_opponentTable.transform);
        StartCoroutine(IE_Freeze(freezeImage.GetComponent<Image>(), 1));        
    }
    private void CastSteal()
    {
        // Player effect
        var chestImage = Instantiate(m_chestImage, m_playerTable.transform);
        StartCoroutine(IE_StartSteal(chestImage.GetComponent<Image>(), 1f));
        // Opponent effect
    }

    // HIT SKILL
    private void HitFreeze()
    {
        // Player effect
        var freezeImage = Instantiate(m_freezeImage, m_playerTable.transform);
        StartCoroutine(IE_Freeze(freezeImage.GetComponent<Image>(), 1));

        // Opponent effect

    }
    private void HitSteal()
    {
        var chestImage = Instantiate(m_chestImage, m_opponentTable.transform);
        StartCoroutine(IE_StartSteal(chestImage.GetComponent<Image>(), 1f));
    }
    #endregion

    #region Coroutine
    // FREEZE SKILL
    private IEnumerator IE_Freeze(Image freezeImage, float speed)
    {
        float time = 0;
        while (time < 1)
        {
            freezeImage.fillAmount = time;
            time += Time.deltaTime * speed;            
            yield return null;
        }        
        freezeImage.fillAmount = 1;
        StartCoroutine(IE_Unfreeze(freezeImage, 0.1f));
    }
    private IEnumerator IE_Unfreeze(Image freezeImage, float speed)
    {
        float time = 0;
        while (time < 1)
        {
            freezeImage.fillAmount = 1 - time;
            time += Time.deltaTime * speed;            
            yield return null;
        }
        freezeImage.fillAmount = 0;
        Destroy(freezeImage.gameObject);
    }

    // STEAL SKILL
    private IEnumerator IE_StartSteal(Image chestImage, float speed)
    {
        m_opponentTable.blocksRaycasts = true;
        float time = 0;
        while (time < 1)
        {
            chestImage.fillAmount = time;
            time += Time.deltaTime * speed;
            yield return null;
        }
        chestImage.fillAmount = 1;
        StartCoroutine(IE_EndSteal(chestImage, 0.1f));
    }
    private IEnumerator IE_EndSteal(Image chestImage, float speed)
    {
        float time = 0;
        while (time < 1)
        {
            chestImage.fillAmount = 1 - time;
            time += Time.deltaTime * speed;
            yield return null;
        }
        chestImage.fillAmount = 0;
        Destroy(chestImage.gameObject);
        m_opponentTable.blocksRaycasts = false;
    }
    #endregion               
}
