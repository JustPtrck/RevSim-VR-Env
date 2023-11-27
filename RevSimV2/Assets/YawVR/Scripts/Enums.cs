namespace YawVR {
    /// <summary>
    /// SDK Connection type
    /// </summary>
    public enum ConnectType {
        CONNECT_FIRST_FOUND_DEVICE,
        DEBUG_CONNECT_TO_IP,
        NO_AUTO_CONNECT
    }

    /// <summary>
    /// YawDevice's status
    /// </summary>
    public enum DeviceStatus
    {
        Available, Reserved, Unknown
    }

    public enum Result
    {
        Success, Error
    }


    /// <summary>
    /// The controller's inner state
    /// </summary>
    public enum ControllerState
    {
        Initial, Connecting, Connected, Starting, Started, Stopping, Disconnecting
    } 
}
