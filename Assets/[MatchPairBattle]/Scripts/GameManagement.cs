using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D.Animation;
using UnityEngine.SceneManagement;

using TMPro;
using Pikachu;
using Network;
using ElementSkill;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManagement : MonoBehaviour
{
    private static GameManagement m_instance;
    public static GameManagement Instance => m_instance;

    [Header("BASIC PROPERTIES")]
    
    [Header("Table Properties")]
    [SerializeField] CanvasGroup m_playerPanel;
    [SerializeField] PikachuTable m_playerTable;

    [SerializeField] CanvasGroup m_opponentPanel;
    [SerializeField] PikachuTable m_opponentTable;        
    public PikachuTable PlayerTable => m_playerTable;
    public PikachuTable OpponentTable => m_opponentTable;

    [Header("Avatar Properties")]
    [SerializeField] Transform m_avatarTransform;
    [SerializeField] public Animator m_avatarAnimator;
    [SerializeField] public SpriteLibrary m_spriteLibrary;

    [Header("PlayTime Properties")]
    [SerializeField] float m_targetTime;
    [SerializeField] Image m_countDownImage;
    [SerializeField] Gradient m_gradientColor;    
    private float m_currentTime;
    [Space]

    [Header("ELEMENTS PROPERTIES")]
    [SerializeField] ButtonSkill m_fireButtonSkill;
    [SerializeField] ButtonSkill m_iceButtonSkill;
    [SerializeField] ButtonSkill m_woodButtonSkill;
    [SerializeField] ButtonSkill m_earthButtonSkill;
    [SerializeField] ButtonSkill m_airButtonSkill;
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
    }
    private void Update()
    {
        if (m_currentStatus != GameStatus.PLAYING) return;

        if (m_currentTime < m_targetTime)
        {
            m_currentTime += Time.deltaTime;
            m_countDownImage.fillAmount = 1f - m_currentTime / m_targetTime;
            m_countDownImage.color = m_gradientColor.Evaluate(m_currentTime / m_targetTime);

            if (m_playerTable.IsTableEmpty)
            {
                SetGameFinishUI(GameStatus.VICTORY);
                SendPlayerResult(GameStatus.VICTORY);
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (m_playerTable.CurrentButtonAmount < m_opponentTable.CurrentButtonAmount)
            {
                SetGameFinishUI(GameStatus.VICTORY);
                SendPlayerResult(GameStatus.VICTORY);
            }
            else if (m_playerTable.CurrentButtonAmount > m_opponentTable.CurrentButtonAmount)
            {
                SetGameFinishUI(GameStatus.DEFEATED);
                SendPlayerResult(GameStatus.DEFEATED);
            }
            else
            {
                SetGameFinishUI(GameStatus.DRAW);
                SendPlayerResult(GameStatus.DRAW);
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
            case PhotonEventCode.EndGame:
                SetPlayerResult(data);
                break;
        }
    }
    protected virtual void SetPlayerResult(object[] data)
    {
        byte resultCode = (byte)data[0];
        switch ((GameStatus)resultCode)
        {
            case GameStatus.VICTORY:
                SetGameFinishUI(GameStatus.DEFEATED);
                break;
            case GameStatus.DRAW:
                SetGameFinishUI(GameStatus.DRAW);
                break;
            case GameStatus.DEFEATED:
                SetGameFinishUI(GameStatus.VICTORY);
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
        if (PhotonManager.Instance.IsHost)
        {
            PhotonManager.Instance.LoadingForStartGame(3f, () => { m_currentStatus = GameStatus.PLAYING; });
        }
        else
        {
            m_playerTable.CreatePlayerTable();
            PhotonManager.Instance.LoadingForStartGame(3f, () => { m_currentStatus = GameStatus.PLAYING; });
        }
        
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
        m_avatarAnimator.SetTrigger("Hit");
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

    public void SendPlayerResult(GameStatus finalStatus)
    {
        // Set current client game's status into END
        m_currentStatus = finalStatus;
        object[] dataSend = new object[] { (byte)m_currentStatus };
        
        // Select other client to receive this event
        RaiseEventOptions targetOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        // Transfer data
        PhotonNetwork.RaiseEvent(
            (byte) PhotonEventCode.EndGame,
            dataSend,
            targetOptions,
            SendOptions.SendUnreliable);
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

    #region Game Process
    private void CalculatePlayerSkill(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.Fire:
                m_fireButtonSkill.StackSkill();
                break;
            case AnimalType.Ice:
                m_iceButtonSkill.StackSkill();
                break;
            case AnimalType.Earth:
                m_earthButtonSkill.StackSkill();
                break;
            case AnimalType.Wood:
                m_woodButtonSkill.StackSkill();
                break;
            case AnimalType.Air:
                m_airButtonSkill.StackSkill();
                break;
        }        
    }
    private void SetGameFinishUI(GameStatus gameResult)
    {        
        switch (gameResult)
        {
            case GameStatus.VICTORY:
                {
                    PhotonManager.Instance.LoadingForEndGame("VICTORY");
                }
            break;

            case GameStatus.DEFEATED:
                {
                    PhotonManager.Instance.LoadingForEndGame("DEFEATED");
                }
            break;

            case GameStatus.DRAW:
                {
                    PhotonManager.Instance.LoadingForEndGame("DRAW");
                }
                break;
        }
    }

    public void StartGame()
    {
        m_playerTable.CreatePlayerTable();
    }
    #endregion

    #region Cast Skills
    private void DelayForAllSkill(float m_skillDelay)
    {
        m_fireButtonSkill.DelayForSkill(m_skillDelay);
        m_iceButtonSkill.DelayForSkill(m_skillDelay);
        m_woodButtonSkill.DelayForSkill(m_skillDelay);
        m_earthButtonSkill.DelayForSkill(m_skillDelay);
        m_airButtonSkill.DelayForSkill(m_skillDelay);
    }
    private void CastFireSkill()
    {     
        var targetButtonsType = new List<byte[]>();
        for (int i = 1; i <= 6; i++)
        {
            var currentButtonsType = m_playerTable.GetAnimalTypeCoordinateList((AnimalType)i);
            if (targetButtonsType.Count < currentButtonsType.Count)
            {
                targetButtonsType = currentButtonsType;
            }
        }
        var skill = Instantiate(m_fireButtonSkill.SkillPrefab, m_playerTable.transform, false);
        skill.transform.localPosition = Vector3.zero;

        SendPlayerSkill((int)SkillType.Fire, targetButtonsType);
        
        skill.GetComponent<Fire>().StartSkill(targetButtonsType, m_avatarTransform.position);
    }
    private void CastIceSkill()
    {
        var skill = Instantiate(m_iceButtonSkill.SkillPrefab, m_opponentTable.transform);

        SendPlayerSkill((int)SkillType.Ice);
        skill.GetComponent<Ice>().StartSkill();
    }
    private void CastWoodSkill()
    {
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
        var skill = Instantiate(m_woodButtonSkill.SkillPrefab, m_opponentTable.transform, false);
        skill.transform.localPosition = Vector3.zero;

        SendPlayerSkill((int)SkillType.Wood, targetButtonsType);
        skill.GetComponent<Wood>().StartSkill(targetButtonsType, m_avatarTransform.transform.position);
    }
    private void CastEarthSkill()
    {
        var skillTargets = m_opponentTable.GetAnimalTypeButtonList(AnimalType.None);
        var skill = Instantiate(m_earthButtonSkill.SkillPrefab, m_opponentTable.transform, false);
        skill.transform.localPosition = Vector3.zero;

        SendPlayerSkill((int)SkillType.Earth);
        skill.GetComponent<Earth>().StartSkill(skillTargets, m_opponentTable.TableSize);
    }
    private void CastAirSkill()
    {
        var skill = Instantiate(m_airButtonSkill.SkillPrefab, m_opponentTable.transform, false);
        var skillStartPosition = m_opponentTable.transform.InverseTransformPoint(m_avatarTransform.position);

        skill.GetComponent<Air>().StartSkillToOpponent(skillStartPosition);
    }

    public void CastSkill(SkillType skillCode, float delayTime)
    {
        m_avatarAnimator.SetTrigger("Cast");
        switch (skillCode)
        {
            case SkillType.Fire:
                CastFireSkill();
                break;
            case SkillType.Ice:
                CastIceSkill();
                break;
            case SkillType.Wood:
                CastWoodSkill();
                break;
            case SkillType.Earth:
                CastEarthSkill();
                break;
            case SkillType.Air:
                CastAirSkill();
                break;
        }
        DelayForAllSkill(delayTime);
    }
    #endregion

    #region Hit Skills
    public void HitFireSkill(List<byte[]> buttonTargets)
    {        
        var skill = Instantiate(m_fireButtonSkill.SkillPrefab, m_opponentTable.transform);        
        skill.GetComponent<Fire>().StartSkill
        (
            skillTargets: buttonTargets, 
            skillStart: m_opponentTable.transform.TransformPoint(Vector3.zero),
            isPlayerCast: false
        );
    }
    public void HitIceSkill()
    {
        var skill = Instantiate(m_iceButtonSkill.SkillPrefab, m_playerTable.transform);
        skill.GetComponent<Ice>().StartSkill();
    }
    public void HitWoodSkill(List<byte[]> buttonTargets)
    {
        var skill = Instantiate(m_woodButtonSkill.SkillPrefab, m_playerTable.transform, false);
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
        var skill = Instantiate(m_earthButtonSkill.SkillPrefab, m_playerTable.transform, false);
        skill.transform.localPosition = Vector3.zero;
        
        skill.GetComponent<Earth>().StartSkill(skillTargets, m_playerTable.TableSize);
    }
    public void HitAirSkill(byte[,] tableData, Vector2 tableSize)
    {
        // Delay player to use skill

        var firePosition = m_fireButtonSkill.transform.localPosition;
        var delayOnFire = Instantiate(m_airButtonSkill.SkillPrefab, m_fireButtonSkill.transform.parent);
        delayOnFire.transform.localPosition = new Vector3(firePosition.x, firePosition.z, -10);
        Destroy(delayOnFire, 3f);

        var icePosition = m_iceButtonSkill.transform.localPosition;
        var delayOnIce = Instantiate(m_airButtonSkill.SkillPrefab, m_iceButtonSkill.transform.parent);        
        delayOnIce.transform.localPosition = new Vector3(icePosition.x, icePosition.y, -10);
        Destroy(delayOnIce, 3f);

        var woodPosition = m_woodButtonSkill.transform.localPosition;
        var delayOnWood = Instantiate(m_airButtonSkill.SkillPrefab, m_woodButtonSkill.transform.parent);
        delayOnWood.transform.localPosition = new Vector3(woodPosition.x, woodPosition.z, -10);
        Destroy(delayOnWood, 3f);

        var earthPosition = m_earthButtonSkill.transform.localPosition;
        var delayOnEarth = Instantiate(m_airButtonSkill.SkillPrefab, m_earthButtonSkill.transform.parent);
        delayOnEarth.transform.localPosition = new Vector3(earthPosition.x, earthPosition.z, -10);
        Destroy(delayOnEarth, 3f);

        var airPosition = m_airButtonSkill.transform.localPosition;
        var delayOnAir = Instantiate(m_airButtonSkill.SkillPrefab, m_airButtonSkill.transform.parent);
        delayOnAir.transform.localPosition = new Vector3(airPosition.x, airPosition.z, -10);
        Destroy(delayOnAir, 3f);


        // Create a tornado to shuffle the player's table
        var skill = Instantiate(m_airButtonSkill.SkillPrefab, m_playerTable.transform, false);
        var skillStartPosition = m_playerTable.transform.InverseTransformPoint(m_opponentTable.transform.position);                
        
        skill.GetComponent<Air>().StartSkillToPlayer(skillStartPosition, tableData, tableSize);                
    }
    #endregion               
}
