namespace Network
{
    public enum PhotonEventCode
    {
        // PhotonEvent
        NoEvent = 0,
        UpdateClientNameInRoom = 1,
        SetClientReady = 2,
        EndGame = 3,

        // Pikachu Event
        TransferTableData = 4,
        TransferPairData = 5,
        TransferSkill = 6,
    }
}
