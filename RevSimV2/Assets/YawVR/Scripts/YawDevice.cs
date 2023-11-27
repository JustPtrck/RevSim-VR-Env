using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace YawVR {

    [Serializable]
    public enum DeviceType {
        YAW1, YAW2
    }
    [Serializable]
    public enum DeviceState {
        STOPPED, STARTED, NOTRACKER, PARKING
    }
    /// <summary>
    /// Describes a YawDevice
    /// </summary>
    [Serializable]
    public class YawDevice
    {
        private IPAddress ipAddress;
        private int tcpPort;
        private int udpPort;
        private string id;
        private string name;
        private DeviceStatus status;

        public IPAddress IPAddress { get { return ipAddress; } }
        public int TCPPort { get { return tcpPort; } }
        public int UDPPort { get { return udpPort; } }
        public string Id { get { return id; } }
        public string Name { get { return name; } }
        public DeviceStatus Status { get { return status; } }

        public float batteryVoltage;
        public float batteryPercent;
        public OVector ActualPosition;
        public Parameters deviceParams;
        public DeviceType type;
        public byte[] temps = new byte[4];
        public DeviceState State;

        public YawDevice(IPAddress ipAddress,DeviceType type, int tcpPort, int udpPort, string id, string name, DeviceStatus status)
        {
            this.ipAddress = ipAddress;
            this.type = type;
            this.tcpPort = tcpPort;
            this.udpPort = udpPort;
            this.id = id;
            this.name = name;
            this.status = status;
        }

        public void SetStatus(DeviceStatus status)
        {
            this.status = status;
        }
    }
}

