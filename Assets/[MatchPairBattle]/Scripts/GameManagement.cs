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
    [SerializeField] PhotonNotification m_photonNotification;

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEventReceive;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceive;

    protected virtual void OnEventReceive(EventData _dataReceive)
    {
        // Import data
        object[] data = (object[])_dataReceive.CustomData;
        // Unpackage data
        switch ((PhotonEventCode) _dataReceive.Code)
        {
            case PhotonEventCode.SetOpponentTableData:
                SetOpponentTableData(data);
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
        m_pikachuManagement.CreateTable(tableCode, tableSize);
    }


    public void SendTableData()
    {
        // Package data
        int row = (int)m_pikachuManagement.m_tableSize.x;
        int column = (int)m_pikachuManagement.m_tableSize.y;

            // Prepare table size
        Vector2 tableSize = new Vector2(row, column);
            
            // Prepare table data
        List<byte[]> tableCode = new List<byte[]>();        
        for (int i = 0; i < row; i++)
        {
            byte[] tmp = new byte[column];
            for (int j = 0; j < column; j++)
            {
                tmp[j] = (byte)m_pikachuManagement.m_table[i, j].color;
            }
            tableCode.Add(tmp);            
        }


        // Package data from master client
        object[] dataSend = new object[11];
        dataSend[0] = tableSize;
        for (int i = 1; i < dataSend.Length; i++)
        {
            dataSend[i] = tableCode[i - 1];
        }

        // Select other client to receive this data
        RaiseEventOptions targetsOption = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // Transfer data
        PhotonNetwork.RaiseEvent(
            (byte) PhotonEventCode.SetOpponentTableData,
            dataSend,
            targetsOption,
            SendOptions.SendUnreliable
            );
    }
}
