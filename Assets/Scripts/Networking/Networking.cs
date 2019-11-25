using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CustomNetworking.Messages;
using Extensions;

namespace CustomNetworking
{
    public class Networking
    {
        public static bool IsInited { get; private set; }
        public static NetworkingMode NetMode { get; private set; }

        private static TcpListener serverListener;
        private static TcpClient oponentMessagesGateway;
        public static System.Action onOponentConnect;

        private static NetworkStream _serverNetworkStream;
        private static NetworkStream ServerNetworkStream
        {
            get
            {
                return _serverNetworkStream ?? (_serverNetworkStream = oponentMessagesGateway.GetStream());
            }
        }

        private static Thread listenForNewMessagesThread = new Thread(ListenForNewMessages);
        private static Thread oponentWaitingThread = new Thread(() =>
        {
            while (true)
            {
                oponentMessagesGateway = serverListener.AcceptTcpClient(); // Freeze, until opponent will connect

                Debug.LogFormat("Opponent connected".ColorTag(ColorStringTag.Green));
                listenForNewMessagesThread.Start();

                if (onOponentConnect != null)
                {
                    NetworkingController.instance.threadDispatcher.InvokeInMainThread(onOponentConnect);
                }

                serverListener.Stop();
                break;
            }
        });

        public static Queue<BaseMessage> acceptedMessages = new Queue<BaseMessage>();

        public static void Start(string ipAdress, int port, NetworkingMode networkingMode)
        {
            NetMode = networkingMode;
            string ipAndPort = string.Format("{0}:{1}", ipAdress.ToString(), port);

            if (NetMode == NetworkingMode.Server)
            {
                Debug.LogFormat("Try to establish connection to {0}", ipAndPort.ColorTag(ColorStringTag.Yellow));

                //Create ip and port listener
                serverListener = new TcpListener(IPAddress.Parse(ipAdress), port);
                serverListener.Start();

                oponentWaitingThread.Start();
            }
            else
            {
                //Connect to ip:port
                oponentMessagesGateway = new TcpClient();
                oponentMessagesGateway.Connect(IPAddress.Parse(ipAdress), port);

                Debug.LogFormat("Try to connect as client to {0}", ipAndPort.ColorTag(ColorStringTag.Yellow));
                onOponentConnect?.Invoke();

                listenForNewMessagesThread.Start();
            }

            IsInited = true;
            
            Debug.LogFormat("Start as {0}", NetMode.ToString());
        }

        /// <summary>
        /// Launches from other thread
        /// </summary>
        private static void ListenForNewMessages()
        {
            Queue<byte> buffer = new Queue<byte>();
            while (IsInited && oponentMessagesGateway != null)
            {
                if(buffer.Count > 0)
                { buffer.Clear(); }

                while (ServerNetworkStream.DataAvailable)
                {
                    int readByte = ServerNetworkStream.ReadByte();
                    if (readByte != -1)
                    {
                        buffer.Enqueue((byte)readByte);
                    }
                }

                if (buffer.Count > 0)
                {
                    string MessageJSON = Encoding.ASCII.GetString(buffer.ToArray());
                    BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(MessageJSON);

                    switch (baseMessage.messageType)
                    {
                        case MessageType.Position:
                            {
                                PositionMessage posMessage = JsonUtility.FromJson<PositionMessage>(MessageJSON);
                                lock (acceptedMessages)
                                {
                                    acceptedMessages.Enqueue(posMessage);
                                }

                                Debug.LogFormat("Get message:\n\n{0}", MessageJSON);

                                break;
                            }
                    }
                }
            }
        }

        public static void SendMessage<T>(T message) where T : BaseMessage
        {
            if (IsInited && oponentMessagesGateway != null)
            {
                string messageStr = JsonUtility.ToJson(message);
                Debug.LogFormat("Send message:\n\n{0}", messageStr);
                byte[] messageArray = Encoding.ASCII.GetBytes(messageStr);
                oponentMessagesGateway.GetStream().Write(messageArray, 0, messageArray.Length);
            }
            else
            {
                Debug.LogErrorFormat("Can't send message\nIsInited: {0}\nNetworkingMode: {1}", IsInited, NetMode);
            }
        }

        ~Networking()
        {
            listenForNewMessagesThread.Abort();
            oponentWaitingThread.Abort();
            ServerNetworkStream.Dispose();
            oponentMessagesGateway.Dispose();
            serverListener.Stop();
        }
    }
}
