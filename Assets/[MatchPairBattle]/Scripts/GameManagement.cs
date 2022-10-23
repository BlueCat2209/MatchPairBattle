using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pikachu;
using Network;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManagement : MonoBehaviour
{
    [Header("Something here")]
    [SerializeField] PikachuManagement m_pikachuManagement;

    [Header("UI Properties")]
    [SerializeField] RectTransform m_canvas;

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;

    void Start()
    {
        m_pikachuManagement.CreatePlayerTable();
    }

    protected virtual void OnEventReceive(EventData _dataReceive)
    {
        // Import data
        object[] data = (object[])_dataReceive.CustomData;
        // Unpackage data
        switch ((PhotonEventCode) _dataReceive.Code)
        {
            case PhotonEventCode.SendOpponentTableData:
                SetOpponentTableData(data);
                break;
            case PhotonEventCode.SendPairData:
                SetPairData(data);
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
    protected virtual void SetPairData(object[] data)
    {        
        Vector2 startCor = (Vector2)data[0];
        Vector2 endCor = (Vector2)data[1];

        ColorButton start = m_pikachuManagement.m_opponentTable[(int)startCor.x, (int)startCor.y];
        ColorButton end = m_pikachuManagement.m_opponentTable[(int)endCor.x, (int)endCor.y];

        m_pikachuManagement.HidePair(start, end);
    }

    public void SendPairData(ColorButton start, ColorButton end)
    {
        // Prepare start & end Cordinates
        Vector2 startCor = new Vector2(start.x, start.y);
        Vector2 endCor = new Vector2(end.x, end.y);

        // Package data
        object[] dataSend = new object[] { startCor, endCor };

        // Select other client to receive this data
        RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // Transfer data
        PhotonNetwork.RaiseEvent(
            (byte) PhotonEventCode.SendPairData,
            dataSend,
            targetsOption,
            SendOptions.SendUnreliable
            );
    }
    public void SendTableData()
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
                tmp[j] = (byte)m_pikachuManagement.m_playerTable[i, j].color;
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
            (byte) PhotonEventCode.SendOpponentTableData,
            dataSend,
            targetsOption,
            SendOptions.SendUnreliable
            );
    }
}
