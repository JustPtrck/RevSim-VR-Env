using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using UnityEngine;

namespace YawVR {


    public interface YawUDPClientDelegate
    {
        void DidRecieveUDPMessage(string message, IPEndPoint remoteEndPoint);
    }


    public class YawUDPClient {

        private int listeningPort;
        private UdpClient udpClient;
        IPEndPoint remoteEndPoint;
        public YawUDPClientDelegate udpDelegate;
        IAsyncResult ar_ = null;

        public YawUDPClient(int listeningPort) {
            try
            {
                this.listeningPort = listeningPort;
                udpClient = new UdpClient(listeningPort);
            }
            catch (Exception err)
            {
                Debug.Log("Error in starting udp listening port: " + err);
            }
        }

        public void SetRemoteEndPoint(IPAddress ipAddress, int port) {
            remoteEndPoint = new IPEndPoint(ipAddress, port);
        }

        public void StartListening()
        {
            try {
                StartListeningToMessages();
            } catch (Exception err) {
                Debug.Log("Error in starting udp listening port: " + err);
            }
        }
        public void StopListening()
        {
            try
            {
                udpClient.Close();
            }
            catch (Exception err) { 
                Debug.Log("Error happened on closing udp listening client" + err); 
            }
        }

        private void StartListeningToMessages()
        {
            ar_ = udpClient.BeginReceive(Receive, new object());
        }
        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, this.listeningPort);
            byte[] bytes = udpClient.EndReceive(ar, ref ipEndPoint);
            string message = Encoding.ASCII.GetString(bytes);
            
            if (!message.Contains("YAW_CALLING")) {
                ActionBus.Instance().Add(() =>
                {
                    //Debug.Log(message);
                    udpDelegate.DidRecieveUDPMessage(message, ipEndPoint);
                });
            }
            StartListeningToMessages();
        }

        public void SendBroadcast(int port, byte[] data)
        {
            if (udpClient == null) return;
            try {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), port);
                udpClient.Send(data, data.Length, ipEndPoint);
            } catch (Exception err) {
                Debug.Log("Error in sending broadcast: " + err);
            }

        }


        public void Send( byte[] data)
        {
            try
            {
                udpClient.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception err)
            {
                Debug.LogError("Error in sending data in YawUDPClient" + err);
            }
        }
    }

}
