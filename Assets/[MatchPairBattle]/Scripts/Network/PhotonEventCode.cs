namespace Network
{
    public enum PhotonEventCode
    {
        // PhotonEvent
        NoEvent = 0,
        UpdateClientNameInRoom = 1,
        SetClientReady = 2,

        // Pikachu Event
        SendOpponentTableData = 3,
        SendPairData = 4,
    }
}
