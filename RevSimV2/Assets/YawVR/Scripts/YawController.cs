using System.Collections;
using UnityEngine;
using System;
using System.Net;
using System.Threading;
using UnityEditor;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Events;

namespace YawVR {


    /// <summary>
    /// OVector is a Vector3D with yaw,pitch,roll named variables.
    /// </summary>
    [Serializable]
    public struct OVector {

        public float yaw, pitch, roll;

        public OVector(float yaw, float pitch, float roll) {
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;
        }
    }
    [Serializable]
    public struct Parameters {
        public byte Power, RollLimit, PitchLimitF, PitchLimitB;
        public UInt32 YawLimit;
        public bool hasYawLimit;

    }
    /// <summary>
    /// Buzzer info
    /// Amplitudes and hz
    /// </summary>
    [Serializable]
    public class Buzzer {
        public bool isOn;
        public int right_amp, center_amp, left_amp, hz;

        public void SetBuzzerAmps(int right, int center, int left) {
            this.right_amp = right;
            this.center_amp = center;
            this.left_amp = left;
        }
        public void SetHz(int buzzerHz) {
            this.hz = buzzerHz;

        }

        public void SetOn(bool b) {
            isOn = b;
        }

    }
    /// <summary>
    /// Game Limits
    /// The limits are applied to the YawVR Tracker
    /// </summary>
    [Serializable]
    public class Limits {
        public float yaw = -1, pitch = -1, roll = -1;
       
        public Limits(float yaw, float pitch, float roll) {
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;
        }
    }
    /// <summary>
    /// The script, that needs to receive notifications, is need to inherited from YawControllerDelegate
    /// </summary>
    public interface YawControllerDelegate {
        void ControllerStateChanged(ControllerState state);

        /// <summary>
        /// A found is device on network
        /// </summary>
        void DidFoundDevice(YawDevice device);

        /// <summary>
        /// Disconnected from device
        /// </summary>
        void DidDisconnectFrom(YawDevice device);
     

        void DeviceStoppedFromApp(); // will be called when device stopped from app
        void DeviceStartedFromApp();// will be called when device started from app
    }

    public interface YawControllerType {
        //Properties 
        ControllerState State { get; }
        YawDevice Device { get; }
        YawControllerDelegate ControllerDelegate { get; set; }

        //Motion related properties
       
        Vector3 RotationMultiplier { get; }

        Limits Limits { get; }

        Buzzer Buzzer { get; }


        //Game related setters
        void SetGameName(string gameName);
      

        //Methods triggering delegate functions
        void DiscoverDevices(int onPort);
        void SetTiltLimits(float yawLimit, float pitchLimit, float rollLimit);
     
        //Methods with success/error action callbacks
        void ConnectToDevice(YawDevice yawDevice, Action onSuccess, Action<string> onError);
        void StartDevice(Action onSuccess, Action<string> onError);
        void StopDevice(bool park, Action onSuccess, Action<string> onError);
        void DisconnectFromDevice(Action onSuccess, Action<string> onError);
        void CalibrateDevice(bool allAxis);
       
        void SetRotationMultiplier(float yaw, float pitch, float roll);
    }
    [Serializable]
    public class StateChangeEvent : UnityEvent<DeviceState> { }
    public class YawController : MonoBehaviour, YawControllerType, YawTCPClientDelegate, YawUDPClientDelegate {

     
        private static YawController instance;
        public static List<Action> OnConnectReceivers = new List<Action>();
        private YawTCPClient tcpCLient;
        private YawUDPClient udpClient;
        [SerializeField]
        private YawDevice device = null;
        private ControllerState state = ControllerState.Initial;
        private int discoveryPort = 0;
        private CallBacks callBacks = new CallBacks();
        private CallbackTimeouts callbackTimeouts = new CallbackTimeouts();
        private Orientation orientation;
        private YawTracker yawTracker;

        #region PROPERTIES

        public YawTracker TrackerObject {  get { return yawTracker; } }
        public ControllerState State { get { return state; } }
        public YawDevice Device { get { return device; } }
        public YawControllerDelegate ControllerDelegate { get; set; }

        public Vector3 RotationMultiplier { get { return rotationMultiplier; } }
      
        public Limits Limits {  get { return gameLimits; } }

        public Buzzer Buzzer { get { return buzzer; } }
        #endregion
      
        [SerializeField]
        Transform referenceTransform; // we ill copy this objects rotation, and send it to the sim
        [SerializeField]
        string gameName; //name of the game

        [SerializeField]
        private ConnectType connectType; //connect type, for debug purposes
        [SerializeField]
        private string debug_ipAddress; //ip to connect in debug mode

        [SerializeField]
        int udpClientPort;
      
    
        private OVector referenceRotation; // the rotation of the YAWTracker

        [SerializeField]
        private Vector3 rotationMultiplier = new Vector3(1, 1, 1); // multiplier for YAWTracker
        [SerializeField]
        private Limits gameLimits;
        [SerializeField]
        private Buzzer buzzer;
        [SerializeField]
        private byte smartPlug;

        [Header("Camera Cancellation")]
        [SerializeField]
        private MotionCompensation cancellation;



        [Header("Events")]
        [SerializeField]
        private UnityEvent onConnected;
        [SerializeField]
        private UnityEvent onDisconnected;
        [SerializeField]
        private UnityEvent<int> ButtonPressed;
        [SerializeField]
        private StateChangeEvent onStateChanged;
        public static YawController Instance() {
            if (instance == null) {
                throw new Exception("Please drag YawController prefab into your scene");
            }
            return instance;
        }

        //MARK: - Lifecycle methods

        void Awake() {
            //Creating singleton instance
            if (instance == null) {
                instance = this;
            }
            else if (instance != this) {
                DestroyImmediate(gameObject);
            }

            orientation = GetComponentInChildren<Orientation>();
           // Debug.Log(orientation);
            yawTracker = GetComponentInChildren<YawTracker>();

            //Make gameObject persitent through multiple scenes
             DontDestroyOnLoad(gameObject); 

            //Initialize tcp client
            tcpCLient = new YawTCPClient();
            tcpCLient.tcpDelegate = this;

            //Initialize udp client and start listening on given listening port
            udpClient = new YawUDPClient(udpClientPort);
            udpClient.udpDelegate = this;
            udpClient.StartListening();

            Debug.Log("YawController initialized");
        }
        private void Start() {
            if (connectType == ConnectType.CONNECT_FIRST_FOUND_DEVICE) AutoConnectFirst();

            if(connectType == ConnectType.DEBUG_CONNECT_TO_IP) {
                ConnectToDevice(new YawDevice(IPAddress.Parse(debug_ipAddress),DeviceType.YAW1,50020,50010,"001","DEBUG",DeviceStatus.Available),
                    null,null
                    
                    );
            }
        }
 
        void FixedUpdate() {
            referenceRotation.pitch = orientation.pitch;
            referenceRotation.yaw = orientation.yaw;
            referenceRotation.roll = orientation.roll;
            //If we are in game, sending rotation command to simulator  based on the latest processed motion data
            if (state == ControllerState.Started || state == ControllerState.Connected) {
                SendMotionData();
            }
        }

        void OnDestroy() // NNN
        {
            Debug.Log("Destroying YawController");
            if (state != ControllerState.Initial && state != ControllerState.Disconnecting && device != null) {
                DisconnectFromDevice(null, null);
            }
            //Closing tcp & udp clients
            tcpCLient.CloseConnection();
            udpClient.StopListening();
            Thread.Sleep(1000);
            instance = null;

            //  Debug.Log("Destroying YawController finish");
        }
        void OnApplicationQuit() {
            //If our application terminates, sending Exit command to simulator if needed 
            if (state != ControllerState.Initial && state != ControllerState.Disconnecting && device != null) {
                DisconnectFromDevice(null, null);
            }
            //Closing tcp & udp clients
            tcpCLient.CloseConnection();
            udpClient.StopListening();
        }
        /// <summary>
        /// Sets the GameName
        /// </summary>
        public void SetGameName(string gameName) {
            this.gameName = gameName;
        }

        //MARK: - Methods triggering delegate functions

        /// <summary>
        /// Sends a broadcast to the network
        /// </summary>
        public void DiscoverDevices(int onPort) {
            //Save a reference to port, which will be used in creating yawDevices when discovery responses arrive
            //We have to use their listening port (this) - not from which it sends response
            discoveryPort = onPort;
            //Send the discovery broadcast
            udpClient.SendBroadcast(onPort, Commands.DEVICE_DISCOVERY);
        }

        /// <summary>
        /// Connect to a YawDevice
        /// </summary>
        public void ConnectToDevice(YawDevice yawDevice, Action onSuccess, Action<String> onError) {
            if (state == ControllerState.Initial) {
                SetState(ControllerState.Connecting);

                //Start tcp connection timeout
                callbackTimeouts.tcpConnectionAttemptTimeout = StartCoroutine(ResponseTimeout((error) => {
                    onError("Failed to create TCP connection");
                    SetState(ControllerState.Initial);
                    tcpCLient.StopConnecting();
                    Debug.Log("TCP client connecting timeout- initial set before");

                }));

                //Start connecting to simulator's tcp server
                tcpCLient.Initialize(yawDevice.IPAddress.ToString(),
                                     yawDevice.TCPPort,
                                     () => {
                                         //Connected to tcp server
                                         //Stop tcp connection timeout
                                         StopCoroutine(callbackTimeouts.tcpConnectionAttemptTimeout);
                                         callbackTimeouts.tcpConnectionAttemptTimeout = null;
                                         //Set connected device to this device 
                                         device = yawDevice;
                                         //Start listening for tcp messages
                                         tcpCLient.BeginRead();

                                         //Start sending CHECK_IN command to connected tcp server
                                         //Set CHECK_IN command callbacks and start command timeout
                                         callBacks.connectingError = onError;
                                         callBacks.connectingSuccess = onSuccess;
                                         callbackTimeouts.connectingTimeout = StartCoroutine(ResponseTimeout((error) => {
                                             onError(error);
                                             SetState(ControllerState.Initial);
                                         }));
                                         //Send CHECK_IN command
                                         tcpCLient.BeginSend(Commands.CHECK_IN(udpClientPort, gameName));
                                        
                                         StartCoroutine(DeviceHeartbeat());
                                        

                                     },
                                     (error) => {
                                         //Could not connect to tcp server
                                         //Stop tcp connection timeout
                                         StopCoroutine(callbackTimeouts.tcpConnectionAttemptTimeout);
                                         callbackTimeouts.tcpConnectionAttemptTimeout = null;
                                         onError(error);
                                         //Set state back to initial
                                         SetState(ControllerState.Initial);
                                     });
            } else {
                //If we are already connected to a device, disconnect from it, then connect to new one
                DisconnectFromDevice(
                    () => {
                        ConnectToDevice(yawDevice, onSuccess, onError);
                    },
                    (error) => {
                        onError(error);
                        ConnectToDevice(yawDevice, onSuccess, onError);
                    });
            }
        }
        /// <summary>
        /// Send a START command to the connected simulator
        /// </summary>
        public void StartDevice(Action onSuccess = null, Action<String> onError = null) {
            if (state == ControllerState.Connected) {
                //Set START command callbacks and start command timeout
                callBacks.startSuccess = onSuccess;
                callBacks.startError = onError;
                callbackTimeouts.startTimeout = StartCoroutine(ResponseTimeout(onError));
                SetState(ControllerState.Starting);
                //Send START command
                tcpCLient.BeginSend(Commands.START);
                //Set state to starting
               
            } else {
                onError("Attempted to start device when device has not been in connected ready state");
            }
        }

        /// <summary>
        /// Send a STOP command to the connected simulator
        /// </summary>
        public void StopDevice(bool park, Action onSuccess = null, Action<String> onError = null) {
            if (state == ControllerState.Started) {
                //Set STOP command callbacks and start command timeout
                callBacks.stopSuccess = onSuccess;
                callBacks.stopError = onError;
                callbackTimeouts.stopTimeout = StartCoroutine(ResponseTimeout(onError));
                SetState(ControllerState.Stopping);
                //Send STOP command
                tcpCLient.BeginSend(new byte[] { Commands.STOP, (byte)(park ? 1 : 0) });
                //Set state to stopping
                
            } else {
                onError("Attempted to stop simulator when simulator had not been in started state");
            }
        }

        /// <summary>
        /// Send a CALIBRATE command to the connected simulator
        /// </summary>
        public void CalibrateDevice(bool allAxis) {
            if (state == ControllerState.Connected) {

                tcpCLient.BeginSend(new byte[2] { Commands.CALIBRATE[0], (byte)(allAxis ? 1 : 0) });
                //Set state to starting

            }
           
        }
        /// <summary>
        /// Disconnect from the connected yawdevice, onSuccess is called afterwards
        /// </summary>
        public void DisconnectFromDevice(Action onSuccess, Action<String> onError) {
            if (state != ControllerState.Initial) {
                //Set EXIT command callbacks and start command timeout
                callBacks.exitSuccess = onSuccess;
                callBacks.exitError = onError;
               
                callbackTimeouts.exitTimeout = StartCoroutine(ResponseTimeout((error) => {
                    //If we reach timeout without server response, set state back to initial
                    //This way we reach disconnected and ready state anyway
                    SetState(ControllerState.Initial);
                    if (onError != null) {
                        onError(error);
                    }
                }));
                //Send EXIT command
                tcpCLient.BeginSend(Commands.EXIT);
                //Set state to disconnecting
                SetState(ControllerState.Disconnecting);

                onDisconnected.Invoke();
            } else {
                onError("Attempted to disconnect when no device was connected");
            }
        }

        /// <summary>
        /// This function handles the incoming UDP packages from the yawDevice
        /// </summary>
        public void DidRecieveUDPMessage(string message, IPEndPoint remoteEndPoint) {

             //Console.WriteLine(message);

            MatchCollection regular = Regex.Matches(message, @"(?:S?([YPR]|U))\[(-?[0-9]+(?:\.[0-9]+))\]");
            
            //at least 3 matches (Y,P,R), and U (battery) is optional
            if (regular.Count >= 3) {

                //Iterate through matches
                foreach (Match m in regular) {
                    if (m.Groups.Count >= 2) {
                        switch (m.Groups[1].Value) {
                            case "Y":
                                //device.ActualPosition.yaw = float.Parse(m.Groups[2].Value);
                                float.TryParse(m.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out device.ActualPosition.yaw);
                                break;
                            case "P":
                                float.TryParse(m.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture,out device.ActualPosition.pitch);
                                break;
                            case "R":
                                float.TryParse(m.Groups[2].Value,NumberStyles.Float, CultureInfo.InvariantCulture, out device.ActualPosition.roll);
                                break;
                            case "U":
                                float.TryParse(m.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture,out device.batteryVoltage);
                                device.batteryPercent = Mathf.InverseLerp(2.8f, 4.2f, device.batteryVoltage);
                                break;

                        }

                    }
                }
            }


            if (message.Contains("YAWDEVICE")) {

                //We recieved a device discovery answer message
                //example device discovery answer: "YAWDEVICE;MacAddrId;MyDeviceName;" + tcpServerPort + (state == DeviceState.Available ? ";OK" : ";RESERVED");
                var messageParts = message.Split(';');
                var ip = remoteEndPoint.Address;
                var udp = discoveryPort;
                int tcp;

                if (messageParts.Length >= 5 && int.TryParse(messageParts[3], out tcp)) {
                    DeviceType type = message.Contains("YAWDEVICE2") ? DeviceType.YAW2 : DeviceType.YAW1;
                    DeviceStatus status = messageParts[4] == "AVAILABLE" ? DeviceStatus.Available : DeviceStatus.Reserved;
                    var yawDevice = new YawDevice(ip,type, tcp, udp, messageParts[1], messageParts[2], status);
                    //Call delegate function if we have a delegate
                    if (ControllerDelegate != null) {
                        ControllerDelegate.DidFoundDevice(yawDevice);

                    }
                }
            }
           
        }

        /// <summary>
        /// This function handles the incoming TCP commands from the yawDevice
        /// </summary>
        public void DidRecieveTCPMessage(byte[] data) {
            //data.Length can't be 0 - YawTcpClient would not dispatch it
            //Read command id from the array
            byte commandId = data[0];
            // Debug.Log(commandId + " " + data.Length);
            switch (commandId) {
                case CommandIds.CHECK_IN_ANS:

                    Invoke("UpdateIMUOffset", 0.1f);

                    //Stop timeout
                    if (callbackTimeouts.connectingTimeout != null) {
                        StopCoroutine(callbackTimeouts.connectingTimeout);
                        callbackTimeouts.connectingTimeout = null;
                    }
                    if (state == ControllerState.Connecting) {
                        string message = System.Text.Encoding.ASCII.GetString(data, 1, data.Length - 1);
                     //   Debug.Log(message);
                        if (message.Contains("AVAILABLE")) {
                            device.type = message.Contains("YAWDEVICE2") ? DeviceType.YAW2 : DeviceType.YAW1;
                            foreach (Action a in OnConnectReceivers) {
                                a.Invoke();
                            }
                            onConnected.Invoke();
                            //Simulator is available, we have succesfully checked in, set state to connected
                            udpClient.SetRemoteEndPoint(device.IPAddress, device.UDPPort);
                            SetState(ControllerState.Connected);
                            //Call success callback
                            if (callBacks.connectingSuccess != null) {
                                callBacks.connectingSuccess();
                                callBacks.connectingSuccess = null;
                                callBacks.connectingError = null;
                            }



                        } else {
                            //Simulator is reserved, setting state back to initial
                            var messageParts = message.Split(';');
                            if (messageParts.Length != 3) return;
                            var reservingGameName = messageParts[1];
                            var reservingIp = messageParts[2];
                            SetState(ControllerState.Initial);
                            //Call error callback
                            if (callBacks.connectingError != null) {
                                callBacks.connectingError("Device is in use from: " + reservingIp + " with game: " + gameName);
                                callBacks.connectingError = null;
                                callBacks.connectingSuccess = null;
                            }
                        }
                    }
                    break;

                case CommandIds.START:

                    //Stop timeout
                    if (callbackTimeouts.startTimeout != null) {
                        StopCoroutine(callbackTimeouts.startTimeout);
                        callbackTimeouts.startTimeout = null;
                    }
                    if (state == ControllerState.Starting) {
                        //Set state to started
                        SetState(ControllerState.Started);
                        //Call success callback
                        if (callBacks.startSuccess != null) {
                            callBacks.startSuccess();
                            callBacks.startSuccess = null;
                            callBacks.startError = null;
                        }

                    }
                    else {
                        SetState(ControllerState.Started);
                        if (ControllerDelegate != null) {

                            ControllerDelegate.DeviceStartedFromApp();
                        }
                    }
                    break;

                case CommandIds.STOP:
                    //Stop timeout
                    if (callbackTimeouts.stopTimeout != null) {
                        StopCoroutine(callbackTimeouts.stopTimeout);
                        callbackTimeouts.stopTimeout = null;
                    }
                    if (state != ControllerState.Initial && state != ControllerState.Disconnecting) {
                        //Set state back to connected
                        SetState(ControllerState.Connected);
                        //Call success callback
                        if (callBacks.stopSuccess != null) {
                            callBacks.stopSuccess();
                            callBacks.stopSuccess = null;
                            callBacks.stopError = null;
                        } else {
                            SetState(ControllerState.Connected);
                            if (ControllerDelegate != null) ControllerDelegate.DeviceStoppedFromApp();

                        }
                    }
            
                    break;

                case CommandIds.EXIT:
                    //Stop timeout
                    if (callbackTimeouts.exitTimeout != null) {
                        StopCoroutine(callbackTimeouts.exitTimeout);
                        callbackTimeouts.exitTimeout = null;
                    }
                    //Whenever we got an exit command from connected simulator, we close connection, not only if we invoked that
                    SetState(ControllerState.Initial);
                    //Call success callback - if we have one
                    if (callBacks.exitSuccess != null) {
                        callBacks.exitSuccess();
                        callBacks.exitSuccess = null;
                        callBacks.exitError = null;
                    }
                break;

                case CommandIds.BUTTON_PRESSED:
                    ButtonPressed.Invoke(data[1]);
                    break;

                case CommandIds.SET_POWER:
                    //  byte[] buffer = new byte[34 + 34];
                    if (data.Length > 5) {
                        Console.WriteLine("GETALL_PARAMS response " + data.Length);
                        device.deviceParams.Power = data[4];
                        device.deviceParams.PitchLimitF = data[13];
                        device.deviceParams.PitchLimitB = data[9];
                        device.deviceParams.RollLimit = data[17];
                        device.deviceParams.YawLimit = Helpers.ReadInt(data, 19, false);
                        device.deviceParams.hasYawLimit = data[24] == 1;
                    }
                    else {
                        device.deviceParams.Power = data[4];

                    }
                    break;
                case CommandIds.GET_STATE:
                    string statestring = Encoding.ASCII.GetString(data, 2, data.Length - 2).Trim();
                    DeviceState newState = DeviceState.STOPPED;
                    switch (statestring) {
                        case "disabled":
                            newState = DeviceState.STOPPED;
                            break;
                        case "simulation mode":
                            newState = DeviceState.STARTED;
                            break;
                        case "emergency mode":
                            newState = DeviceState.NOTRACKER;
                            break;
                        case "parking":
                            newState = DeviceState.PARKING;
                            break;
                    }
                    if (device.State != newState) onStateChanged.Invoke(newState);
                    this.device.State = newState;
                    break;

                case CommandIds.GET_TEMPS:
                    device.temps[0] = data[1];
                    device.temps[1] = data[2];
                    device.temps[2] = data[3];

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Lost server connection. The controllerDelegates corresponding function's will be called
        /// </summary>
        public void DidLostServerConnection() {
            onDisconnected.Invoke();
            Debug.Log("TCP Client have disconnected");
            if (ControllerDelegate != null) {
                ControllerDelegate.DidDisconnectFrom(device);
            }
            SetState(ControllerState.Initial);
        }

        /// <summary>
        /// Set rotation limits for the YawTracker
        /// </summary>     
        public void SetTiltLimits(float yawLimit, float pitchLimit, float rollLimit) {
            gameLimits.yaw = yawLimit;
            gameLimits.pitch = pitchLimit;
            gameLimits.roll = rollLimit;

            int pitchFrontMax = (int)pitchLimit;
            int pitchBackMax = (int)pitchLimit; 
            int rollMax = (int)rollLimit;
            tcpCLient.BeginSend(Commands.SET_TILT_LIMITS(pitchFrontMax, pitchBackMax, rollMax));

            int yawMax = (int)yawLimit;
            tcpCLient.BeginSend(Commands.SET_YAW_LIMIT(yawMax));
        }
        /// <summary>
        /// Set rotation multiplier for the YawTracker
        /// </summary>
        public void SetRotationMultiplier(float yaw, float pitch, float roll) {
            rotationMultiplier.x = pitch;
            rotationMultiplier.y = yaw;
            rotationMultiplier.z = roll;
        }
        private void SendMotionData() {

            if (device == null) return;

            float yaw = 0, pitch = 0, roll = 0;
  
            yaw = SignedForm(referenceRotation.yaw);
            pitch = SignedForm(referenceRotation.pitch);
            roll = SignedForm(referenceRotation.roll);

            
            SendRotation(new OVector(yaw,pitch,roll));
        }

        //MARK: - UDP command sender functions

        /// <summary>
        /// Send gamedata to the device
        /// </summary>
        private void SendRotation(OVector rotation) {
             udpClient.Send(Commands.MOTION_DATA(rotation.yaw, rotation.pitch, rotation.roll, buzzer,smartPlug));
    
        }

        public void SendLED(Color32[] colors) {
            if (colors.Length != 129) return;

            udpClient.Send(Commands.UDP_LED_CMD(colors));
        }

        public void SendLED(Color32 color) {
            udpClient.Send(Commands.UDP_LED_CMD(color));
        }

        /// <summary>
        /// Mark the current IMU data as origin for the Camera rotation cancellation
        /// </summary>
        //public void UpdateIMUOffset() {
        //    cancellation.UpdateOffset();
        //}

        //MARK: - Helper functions

        /// <summary>
        /// Sets the SDK's inner state
        /// </summary>
        private void SetState(ControllerState newState) {
            state = newState;
         //   Debug.Log("state changed to " + state);
            if (newState == ControllerState.Initial) {
                device = null;
                if (tcpCLient.Connected) {
                    tcpCLient.CloseConnection();
                }
            }
            if (ControllerDelegate != null) {
                ControllerDelegate.ControllerStateChanged(newState);
            }
        }

        private IEnumerator ResponseTimeout(Action<string> onError) {
            if (onError == null) yield break;
            yield return new WaitForSeconds(10f);
            onError("Command timeout");
        }

        private float SignedForm(float angle) {
            return angle >= 180 ? angle - 360 : angle;
        }

        private float UnsignedForm(float angle) {
            return angle < 0 ? 360 + angle : angle;
        }
        //MARK: - Helper structs

        private struct CallBacks {
            public Action connectingSuccess;
            public Action<string> connectingError;
            public Action startSuccess;
            public Action<string> startError;
            public Action stopSuccess;
            public Action<string> stopError;
            public Action exitSuccess;
            public Action<string> exitError;

         //   public Action deviceStoppedFromApp;
        }
        private struct CallbackTimeouts {
            public Coroutine connectingTimeout;
            public Coroutine startTimeout;
            public Coroutine stopTimeout;
            public Coroutine exitTimeout;
            public Coroutine tcpConnectionAttemptTimeout;
        }


      

        #region AutoConnect
        private void AutoConnectFirst() {
            Debug.Log("-----------------------------DISCOVER---------------------------");

          

            // starting a repeating call to YawController.Instance().DiscoverDevices(udpPort) with the help of a coroutine - calling continuously because udp packet may be lost
            StartCoroutine(DeviceDiscoveryCoroutine());

            // We receive device in the DidFoundDevice(YawDevice) method
        }

        private IEnumerator DeviceDiscoveryCoroutine() {

            while (state == ControllerState.Initial) {
             
                DiscoverDevices(50010);
             
                yield return new WaitForSeconds(1);
            }
        }
        private IEnumerator DeviceHeartbeat() {
       
            WaitForSeconds wait = new WaitForSeconds(1f);
            yield return wait;
            tcpCLient.BeginSend(new byte[] { CommandIds.GET_ALL_APP_PARAMS });
            while (tcpCLient.Connected) {
                tcpCLient.BeginSend(new byte[] { CommandIds.GET_STATE });
                tcpCLient.BeginSend(new byte[] { CommandIds.GET_TEMPS });
                yield return wait;
            }
               
        }

        // YawControllerDelegate functions

        private void DidFoundDevice(YawDevice device) {
        //    Debug.Log("Did found device: " + device.Name);
            if (YawController.Instance().State == ControllerState.Initial && (device.Status == DeviceStatus.Available || device.Status == DeviceStatus.Unknown)) {
                Debug.Log("-----------------------------CONNECT TO A DEVICE---------------------------");
                YawController.Instance().ConnectToDevice(device, () => {
                    Debug.Log("YAWCONTROLLER: connected");
                }, (error) => { Debug.Log("kapcsolat error"); });
            }
        }

        #endregion
    }

}
