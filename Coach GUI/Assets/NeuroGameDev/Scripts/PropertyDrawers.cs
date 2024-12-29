using System;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine;

[Serializable]
public class AccelerometerData
{
    public double forward;
    public double right;
    public double down;
}

[Serializable]
public class GyroData
{
    public double roll;
    public double pitch;
    public double yaw;
}

[Serializable]
public class EEGData
{
    public double TP9;
    public double AF7;
    public double AF8;
    public double TP10;
    public double rightAux;
    public double leftAux;
}

[Serializable]
public class ChannelData
{
    public double TP9;
    public double AF7;
    public double AF8;
    public double TP10;
}

[Serializable]
public class BatteryData
{
    public double level;
    public double voltage;
    public double temperature;
}

[Serializable]
public class HSIData
{
    //Each channel represents the fit at that location. A value of 1 represents a good fit, 2 represents a mediocre fit, and a value or 4 represents a poor fit.
    public int fitTP9;
    public int fitAF7;
    public int fitAF8;
    public int fitTP10;
}

[Serializable]
public class DRLRefData
{
    public double DRL;
    public double REF;
}

[Serializable]
public class PPGData
{
    public double ambient;
    public double infrared;
    public double red;
}


[Serializable]
public class ArtifactData
{
    public bool headbandOn;
    public bool blink;
    public bool jawClench;
    public long timestamp;
    public string bluetoothMac;
}