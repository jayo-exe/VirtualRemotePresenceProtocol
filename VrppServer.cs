using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace VirtualRemotePresenceProtocol
{
    class VrppServer
    {
        public TcpListener listener { get; set; }
        public Dictionary<string, VrppConnection> connections { get; set; }
        public Dictionary<string, VrppSession> sessions { get; set; }
        public bool active = false;

        public VrppServer(int port)
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ipEndpoint);
        }

        public void StartServer()
        {
            listener.Start();
            active = true;
            listenLoop();
        }

        private async void listenLoop()
        {
            if(!active)
            {
                return;
            }

            TcpClient handler = await listener.AcceptTcpClientAsync();

            VrppConnection connection = new VrppConnection();
            connection.Uuid = Guid.NewGuid().ToString();
            connection.server = this;
            connection.stream = handler.GetStream();
            connections.Add(connection.Uuid, connection);
            connection.WaitForMessage();
            connection.SendKeepAlive();
        }
        
        public async void purgeSession(string sessionId)
        {
            sessions.Remove(sessionId);
        }

        public async void purgeUserConnection(string userId)
        {
            foreach(KeyValuePair<string,VrppConnection> connection in connections)
            {
                if(connection.Value.user.Uuid == userId)
                {
                    connections[connection.Key].stream.Close();
                    connections[connection.Key].stream.Dispose();
                    connections.Remove(connection.Key);
                    return;
                }
            }
        }

        public VrppConnection getUserConnection(string userId)
        {
            foreach (KeyValuePair<string, VrppConnection> connection in connections)
            {
                if (connection.Value.user.Uuid == userId)
                {
                    return connections[connection.Key];
                }
            }
            return null;
        }
    }
}
