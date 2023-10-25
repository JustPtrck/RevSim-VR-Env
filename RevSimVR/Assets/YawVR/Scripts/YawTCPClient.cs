using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;

namespace YawVR {

    public interface YawTCPClientDelegate
    {
        void DidRecieveTCPMessage(byte[] data);
        void DidLostServerConnection();
    }

    public class YawTCPClient
    {
        private TcpClient tcpClient;
        public YawTCPClientDelegate tcpDelegate;
        private bool connected = false;
        public bool Connected { get { return connected; } }

        Action onConnectionSuccess;
        Action<string> onConnectionError;

        private IAsyncResult ar;

        Thread connectionThread;

        public void Initialize(string ip, int port, Action onConnectionSuccess, Action<string> onConnectionError)
        {
            Debug.Log("TCP client started connecting");
            if (tcpClient != null) {
                if (tcpClient.Connected) {
                    CloseConnection();
                }

            }
            this.onConnectionSuccess = onConnectionSuccess;
            this.onConnectionError = onConnectionError;
            connected = false;
            connectionThread = new Thread(() => Connection(ip, port));
            connectionThread.Start();
        }

        private void Connection(string ip, int port) {
            try
            {
                tcpClient = new TcpClient();
                IPAddress ipAddress = IPAddress.Parse(ip);
                tcpClient.Connect(ip, port);
                ActionBus.Instance().Add(() =>
                {

                    if (tcpClient.Connected)
                    {
                        Debug.Log("Connected to: " + ip + " " + port);
                        connected = true;
                        if (onConnectionSuccess != null) {
                            onConnectionSuccess();
                            onConnectionError = null;
                            onConnectionSuccess = null;
                        }
                    } else {
                        if (onConnectionError != null)
                        {
                            onConnectionError("Unable to connect to tcp server");
                            onConnectionError = null;
                            onConnectionSuccess = null;
                        }
                    }
                    onConnectionError = null;
                    onConnectionSuccess = null;
                });
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                ActionBus.Instance().Add(() =>
                {
                    if (onConnectionError != null ) {
                        onConnectionError("Unable to connect to tcp server");
                        onConnectionError = null;
                        onConnectionSuccess = null;
                    }
                });
            }
        }

        public void StopConnecting() {
            if (connectionThread != null && connectionThread.IsAlive) {
                connectionThread.Abort();
            }
            connectionThread = null;
        }

        public void BeginRead()
        {
            var buffer = new byte[4096];
            var ns = tcpClient.GetStream();
            ns.BeginRead(buffer, 0, buffer.Length, EndRead, buffer);
        }

        public void EndRead(IAsyncResult result)
        {
            var buffer = (byte[])result.AsyncState;
            var ns = tcpClient.GetStream();
            var bytesAvailable = ns.EndRead(result);
            byte[] data = new byte[bytesAvailable];
            Array.Copy(buffer, data, bytesAvailable);
            if (data.Length != 0) {
                ActionBus.Instance().Add(() =>
                {
                    tcpDelegate.DidRecieveTCPMessage(data);
                });
                BeginRead();
            } else {
                ActionBus.Instance().Add(() =>
                {
                    CloseConnection();
                    tcpDelegate.DidLostServerConnection();
                });
            }
        }

        public void BeginSend(byte[] data)
        {
            var ns = tcpClient.GetStream();
            ns.BeginWrite(data, 0, data.Length, EndSend, data);
        }

        public void EndSend(IAsyncResult result)
        {
            var bytes = (byte[])result.AsyncState;
         
        }

        public void CloseConnection()
        {
            connected = false;
            if (tcpClient == null || !tcpClient.Connected) return;
            try
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
            catch (Exception err)
            {
                Debug.Log("Error happened on closing tcp client" + err);
            }

        }
    }
}



