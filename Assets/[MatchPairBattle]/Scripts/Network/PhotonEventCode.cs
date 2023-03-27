namespace Network
{
    public enum PhotonEventCode
    {
        // PhotonEvent
        NoEvent = 0,
        UpdateClientNameInRoom = 1,
        SetClientReady = 2,

        // Pikachu Event
        TransferTableData = 3,
        TransferPairData = 4,
        TransferSkill = 5,
    }
}
