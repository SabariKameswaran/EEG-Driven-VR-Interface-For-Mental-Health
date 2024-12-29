using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Interaxon.Libmuse;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class InteraxonInterfacer : MonoBehaviour
{

    [HideInInspector] public bool connected;
    [HideInInspector] public string museList;
    [HideInInspector] public static InteraxonInterfacer Instance { get; private set; }

    [Header("Connection")]
    public string userMuse = "";
    public ConnectionState currentConnectionState;
    public ConnectionState previousConnectionState;
    public string bluetoothMac;

    [Header("Device Data")]
    public AccelerometerData Accelerometer;
    public GyroData Gyro;
    public BatteryData Battery;
    public HSIData HeadbandFit;
    public ArtifactData Artifacts;

    [Header("Interpreted Data - base Model")]
    public float calm;
    public float focus;
    public float flow;
    public float heartMonitor;
    [Header("PPG Data")]
    public PPGData PPG;
    [Header("EEG Data")]
    public EEGData EEG = new EEGData();
    public DRLRefData DRLRef = new DRLRefData();
    public ChannelData AlphaAbsolute = new ChannelData();
    public ChannelData BetaAbsolute = new ChannelData();
    public ChannelData DeltaAbsolute = new ChannelData();
    public ChannelData ThetaAbsolute = new ChannelData();
    public ChannelData GammaAbsolute = new ChannelData();
    public ChannelData AlphaRelative = new ChannelData();
    public ChannelData BetaRelative = new ChannelData();
    public ChannelData DeltaRelative = new ChannelData();
    public ChannelData ThetaRelative = new ChannelData();
    public ChannelData GammaRelative = new ChannelData();
    public ChannelData AlphaScore = new ChannelData();
    public ChannelData BetaScore = new ChannelData();
    public ChannelData DeltaScore = new ChannelData();
    public ChannelData ThetaScore = new ChannelData();
    public ChannelData GammaScore = new ChannelData();

    private MuseDataPacket packetCache = new MuseDataPacket();
    public void startScanning()
    {
        this.muse.startListening();
    }

    public void userSelectedMuse(string selectedMuse)
    {
        this.userMuse = selectedMuse;
        Debug.Log("Selected muse = " + this.userMuse);
    }

    public void connect()
    {
        if (this.userMuse == "")
        {
            //this.userPickedMuse = this.museList.options[0].text;
            Debug.LogWarning("No Muse selected.");
            return;
        }

        Debug.Log("Connecting to " + this.userMuse);
        this.muse.connect(this.userMuse);
        connected = true;
    }

    public void disconnect()
    {
        //Debug.Log("Disconnect");
        this.muse.disconnect();
    }

    private void OnApplicationQuit()
    {
        //#if PLATFORM_STANDALONE_WIN
        if (this.muse != null)
        {
            this.muse.disconnect();
        }
        //#endif
    }

    private bool initialized;
    private int apiLevel;
    private List<string> androidPermissions;
    private string dataBuffer = "";
    private string connectionBuffer = "";
    private string artifactBuffer = "";
    private LibmuseBridge muse;
    private baseModel baseModel = new baseModel();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    private void Start()
    {
        //Debug.Log("InteraxonInterfacer started.");

        this.userMuse = "";
        this.dataBuffer = "";
        this.connectionBuffer = "";
        this.androidPermissions = new List<string>();

#if PLATFORM_IOS
        muse = new LibmuseBridgeIos();
#elif PLATFORM_ANDROID
        muse = new LibmuseBridgeAndroid();
#elif PLATFORM_STANDALONE_WIN
        this.muse = new LibmuseBridgeWindows();
#endif
        Debug.Log("Libmuse version = " + this.muse.getLibmuseVersion());

#if PLATFORM_ANDROID
        if (IsMinimumApi(31))
        {
            this.androidPermissions.Add("android.permission.BLUETOOTH_CONNECT");
            this.androidPermissions.Add("android.permission.BLUETOOTH_SCAN");
        }
        else
        {
            this.androidPermissions.Add(Permission.FineLocation);
            this.androidPermissions.Add("android.permission.BLUETOOTH");
            this.androidPermissions.Add("android.permission.BLUETOOTH_ADMIN");
        }

        if (!HasPermissions)
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDenied += OnPermissionDenied;
            Permission.RequestUserPermissions(this.androidPermissions.ToArray(), callbacks);
        }
#endif
        if (this.userMuse == "" && HasPermissions)
        {
            AutoConnect();
        }
    }

    private bool HasPermissions
    {
        get
        {
            var allGranted = true;
#if PLATFORM_ANDROID

            foreach (var permission in this.androidPermissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    allGranted = false;
                    break;
                }
            }
#endif
            return allGranted;
        }
    }

    private bool IsMinimumApi(int minLevel)
    {
        if (this.apiLevel == 0)
        {
            var rx = new System.Text.RegularExpressions.Regex(@"API-(?<api>\d+)");
            var m = rx.Match(SystemInfo.operatingSystem);
            if (m.Success && int.TryParse(m.Groups["api"].Value, out this.apiLevel))
            {
                Debug.Log($"API level = {this.apiLevel}");
            }
        }

        return this.apiLevel >= minLevel;
    }

    private void OnPermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} permission granted");
        if (HasPermissions)
        {
            Initialize();
        }
    }

    private void OnPermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} permission denied");
    }

    private void AutoConnect()
    {
        //Debug.Log("AutoConnect");
        Initialize();
        startScanning();

        StartCoroutine(WaitMuseListAndConnection());
    }

    private IEnumerator WaitMuseListAndConnection()
    {
        float elapsedTime = 0f;

        //Debug.Log("Waiting for connection");
        do
        {
            yield return new WaitForSeconds(0.25f);
            elapsedTime += 0.25f;

            receiveMuseList(museList);
        } while (string.IsNullOrEmpty(museList) && elapsedTime < 5f);

        if (elapsedTime >= 5f)
        {
            yield break;
        }
        else if (!string.IsNullOrEmpty(museList))
        {
            var muses = museList.Split(' ').ToList();
            userMuse = muses[0];
            userSelectedMuse(userMuse);
            connect();
            connected = true;
        }
    }

    private void Initialize()
    {
        if (!this.initialized)
        {
            if (muse == null)
            {
                return;
            }
            registerListeners();
            registerAllData();
            initialized = true;
        }
    }

    private void registerListeners()
    {
        this.muse.registerMuseListener(name, nameof(receiveMuseList));
        this.muse.registerConnectionListener(name, nameof(receiveConnectionPackets));
        this.muse.registerDataListener(name, nameof(receiveDataPackets));
        this.muse.registerArtifactListener(name, nameof(receiveArtifactPackets));
    }

    private void registerAllData()
    {
        // This will register for all the available data from muse headband
        // Comment out the ones you don't want

        this.muse.listenForDataPacket("ACCELEROMETER");
        this.muse.listenForDataPacket("GYRO");
        this.muse.listenForDataPacket("EEG");
        this.muse.listenForDataPacket("BATTERY");
        this.muse.listenForDataPacket("DRL_REF");
        this.muse.listenForDataPacket("ALPHA_ABSOLUTE");
        this.muse.listenForDataPacket("BETA_ABSOLUTE");
        this.muse.listenForDataPacket("DELTA_ABSOLUTE");
        this.muse.listenForDataPacket("THETA_ABSOLUTE");
        this.muse.listenForDataPacket("GAMMA_ABSOLUTE");
        this.muse.listenForDataPacket("ALPHA_RELATIVE");
        this.muse.listenForDataPacket("BETA_RELATIVE");
        this.muse.listenForDataPacket("DELTA_RELATIVE");
        this.muse.listenForDataPacket("THETA_RELATIVE");
        this.muse.listenForDataPacket("GAMMA_RELATIVE");
        this.muse.listenForDataPacket("ALPHA_SCORE");
        this.muse.listenForDataPacket("BETA_SCORE");
        this.muse.listenForDataPacket("DELTA_SCORE");
        this.muse.listenForDataPacket("THETA_SCORE");
        this.muse.listenForDataPacket("GAMMA_SCORE");
        this.muse.listenForDataPacket("HSI_PRECISION");
        this.muse.listenForDataPacket("ARTIFACTS");
        this.muse.listenForDataPacket("PPG");
    }

    private void receiveConnectionPackets(string data)
    {
        //Debug.Log("Unity received connection packet: " + data);
        this.connectionBuffer = data;


        MuseConnectionPacket connectionPacket = new MuseConnectionPacket();
        connectionPacket = JsonConvert.DeserializeObject<MuseConnectionPacket>(data);

        //Debug.LogError("Check that the variables in the containers in Serialization.cs coincide with this receiveed data : \n" + data);
        //return;

        currentConnectionState = connectionPacket.CurrentConnectionState;
        previousConnectionState = connectionPacket.PreviousConnectionState;
        bluetoothMac = connectionPacket.BluetoothMac;
    }

    private void receiveDataPackets(string data)
    {
        if (data == null || data == "") { return; }

        // Debug.Log("Unity received data packet: " + data);
        this.dataBuffer = data;

        // Parse the data string into a list of doubles
        ParseDataValues(data);
    }

    private void receiveArtifactPackets(string data)
    {
        if (data == null || data == "") { return; }

        //Debug.Log("Unity received artifact packet: " + data);
        this.artifactBuffer = data;

        ParseArtifactValues(this.artifactBuffer);
    }

    private void receiveMuseList(string data)
    {
        // This method will receive a list of muses delimited by white space.
        //Debug.Log("Found list of muses = " + data);

        museList = data;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!HasPermissions) return;

        if (!this.initialized && HasPermissions)
        {
            Initialize();
        }
#if PLATFORM_STANDALONE_WIN
        LibmuseBridgeWindows.InvokeDispatchQueue();
#endif
        if (currentConnectionState == ConnectionState.CONNECTED)
        {
            connected = true;

            CalculateValuesFrombaseModel(BrainwaveSource.Scored);
        }
        else
        {
            connected = false;
        }
    }

    private void ParseArtifactValues(string data)
    {
        Artifacts = JsonConvert.DeserializeObject<ArtifactData>(data);

        //Debug.LogError("Check that the variables in the containers in Serialization.cs coincide with this receiveed data : \n" + data);
    }

    private void ParseDataValues(string data)
    {
        MuseDataPacket packet = JsonConvert.DeserializeObject<MuseDataPacket>(data);

        switch (packet.PacketType)
        {
            case MuseDataPacketType.ACCELEROMETER:
                Accelerometer.forward = packet.Values[0];
                Accelerometer.right = packet.Values[1];
                Accelerometer.down = packet.Values[2];
                break;
            case MuseDataPacketType.GYRO:
                Gyro.roll = packet.Values[0];
                Gyro.pitch = packet.Values[1];
                Gyro.yaw = packet.Values[2];
                break;
            case MuseDataPacketType.EEG:
                EEG.TP9 = packet.Values[0];
                EEG.AF7 = packet.Values[1];
                EEG.AF8 = packet.Values[2];
                EEG.TP10 = packet.Values[3];
                EEG.rightAux = packet.Values[4];
                EEG.leftAux = packet.Values[5];
                break;
            case MuseDataPacketType.BATTERY:
                Battery.level = packet.Values[0];
                Battery.voltage = packet.Values[1];
                Battery.temperature = packet.Values[2];
                break;
            case MuseDataPacketType.DRL_REF:
                DRLRef.DRL = packet.Values[0];
                DRLRef.REF = packet.Values[1];
                break;
            case MuseDataPacketType.HSI_PRECISION:
                HeadbandFit.fitTP9 = (int)packet.Values[0];
                HeadbandFit.fitAF7 = (int)packet.Values[1];
                HeadbandFit.fitAF8 = (int)packet.Values[2];
                HeadbandFit.fitTP10 = (int)packet.Values[3];
                break;
            case MuseDataPacketType.PPG:
                PPG.ambient = packet.Values[0];
                PPG.infrared = packet.Values[1];
                PPG.red = packet.Values[2];
                break;
            case MuseDataPacketType.ALPHA_ABSOLUTE:
                AlphaAbsolute.TP9 = packet.Values[0];
                AlphaAbsolute.AF7 = packet.Values[1];
                AlphaAbsolute.AF8 = packet.Values[2];
                AlphaAbsolute.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.BETA_ABSOLUTE:
                BetaAbsolute.TP9 = packet.Values[0];
                BetaAbsolute.AF7 = packet.Values[1];
                BetaAbsolute.AF8 = packet.Values[2];
                BetaAbsolute.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.DELTA_ABSOLUTE:
                DeltaAbsolute.TP9 = packet.Values[0];
                DeltaAbsolute.AF7 = packet.Values[1];
                DeltaAbsolute.AF8 = packet.Values[2];
                DeltaAbsolute.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.THETA_ABSOLUTE:
                ThetaAbsolute.TP9 = packet.Values[0];
                ThetaAbsolute.AF7 = packet.Values[1];
                ThetaAbsolute.AF8 = packet.Values[2];
                ThetaAbsolute.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.GAMMA_ABSOLUTE:
                GammaAbsolute.TP9 = packet.Values[0];
                GammaAbsolute.AF7 = packet.Values[1];
                GammaAbsolute.AF8 = packet.Values[2];
                GammaAbsolute.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.ALPHA_RELATIVE:
                AlphaRelative.TP9 = packet.Values[0];
                AlphaRelative.AF7 = packet.Values[1];
                AlphaRelative.AF8 = packet.Values[2];
                AlphaRelative.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.BETA_RELATIVE:
                BetaRelative.TP9 = packet.Values[0];
                BetaRelative.AF7 = packet.Values[1];
                BetaRelative.AF8 = packet.Values[2];
                BetaRelative.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.DELTA_RELATIVE:
                DeltaRelative.TP9 = packet.Values[0];
                DeltaRelative.AF7 = packet.Values[1];
                DeltaRelative.AF8 = packet.Values[2];
                DeltaRelative.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.THETA_RELATIVE:
                ThetaRelative.TP9 = packet.Values[0];
                ThetaRelative.AF7 = packet.Values[1];
                ThetaRelative.AF8 = packet.Values[2];
                ThetaRelative.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.GAMMA_RELATIVE:
                GammaRelative.TP9 = packet.Values[0];
                GammaRelative.AF7 = packet.Values[1];
                GammaRelative.AF8 = packet.Values[2];
                GammaRelative.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.ALPHA_SCORE:
                AlphaScore.TP9 = packet.Values[0];
                AlphaScore.AF7 = packet.Values[1];
                AlphaScore.AF8 = packet.Values[2];
                AlphaScore.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.BETA_SCORE:
                BetaScore.TP9 = packet.Values[0];
                BetaScore.AF7 = packet.Values[1];
                BetaScore.AF8 = packet.Values[2];
                BetaScore.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.DELTA_SCORE:
                DeltaScore.TP9 = packet.Values[0];
                DeltaScore.AF7 = packet.Values[1];
                DeltaScore.AF8 = packet.Values[2];
                DeltaScore.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.THETA_SCORE:
                ThetaScore.TP9 = packet.Values[0];
                ThetaScore.AF7 = packet.Values[1];
                ThetaScore.AF8 = packet.Values[2];
                ThetaScore.TP10 = packet.Values[3];
                break;
            case MuseDataPacketType.GAMMA_SCORE:
                GammaScore.TP9 = packet.Values[0];
                GammaScore.AF7 = packet.Values[1];
                GammaScore.AF8 = packet.Values[2];
                GammaScore.TP10 = packet.Values[3];
                break;
            default:
                Debug.LogError("InteraxonInterfacer does not have a way to deserialize MuseDataPacketType " + packet.PacketType);
                break;
        }
    }

    private List<double> ParseDataValuesToList(string data)
    {
        MuseDataPacket packet = new MuseDataPacket();
        packet = JsonConvert.DeserializeObject<MuseDataPacket>(data);

        List<double> values = new List<double>();

        foreach (var value in packet.Values)
        {
            values.Add((value));
            Debug.Log($"{packet.PacketType} : {value}");
        }

        return values;
    }

    private enum BrainwaveSource
    {
        Absolute,
        Relative,
        Scored
    }

    private void CalculateValuesFrombaseModel(BrainwaveSource source)
    {

        switch (source)
        {
            case BrainwaveSource.Absolute:
                calm = baseModel.GetCalm(AlphaAbsolute, BetaAbsolute, DeltaAbsolute, ThetaAbsolute, GammaAbsolute);
                focus = baseModel.GetFocus(AlphaAbsolute, BetaAbsolute, DeltaAbsolute, ThetaAbsolute, GammaAbsolute);
                flow = baseModel.GetFlow(AlphaAbsolute, BetaAbsolute, DeltaAbsolute, ThetaAbsolute, GammaAbsolute);
                break;
            case BrainwaveSource.Relative:
                calm = baseModel.GetCalm(AlphaRelative, BetaRelative, DeltaRelative, ThetaRelative, GammaRelative);
                focus = baseModel.GetFocus(AlphaRelative, BetaRelative, DeltaRelative, ThetaRelative, GammaRelative);
                flow = baseModel.GetFlow(AlphaRelative, BetaRelative, DeltaRelative, ThetaRelative, GammaRelative);
                break;
            case BrainwaveSource.Scored:
                calm = baseModel.GetCalm(AlphaScore, BetaScore, DeltaScore, ThetaScore, GammaScore);
                focus = baseModel.GetFocus(AlphaScore, BetaScore, DeltaScore, ThetaScore, GammaScore);
                flow = baseModel.GetFlow(AlphaScore, BetaScore, DeltaScore, ThetaScore, GammaScore);
                break;

        }

        heartMonitor = baseModel.GetPPGRate(PPG);
    }

}