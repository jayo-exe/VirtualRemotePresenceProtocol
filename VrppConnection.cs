using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VirtualRemotePresenceProtocol
{
    class VrppConnection
    {

        public string Uuid { get; set; }
        public VrppServer server { get; set; }
        public NetworkStream stream { get; set; }
        public VrppUser user { get; set; }
        public VrppPlayer player { get; set; }
        public VrppSession session { get; set; }
        public string AccessToken { get; set; }
        
        private bool awaitingPong = false;
        private bool active = true;

        public async void WaitForMessage()
        {
            var message = "";

            int cursor = 0;
            while (true) {
                var buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, cursor, buffer.Length);
                cursor += bytesRead;
                message += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (bytesRead == 0) { break; } 
            }

            var messageObj = JsonConvert.DeserializeObject(message);
            if(messageObj is Messaging.AuthorizeRequestMessage authRequest)
            {
                handleAuthorizeRequest(authRequest);
            }
            else if (messageObj is Messaging.SessionCreateRequestMessage sessionCreateRequest)
            {
                handleSessionCreateRequest(sessionCreateRequest);
            }
            else if (messageObj is Messaging.SessionCloseRequestMessage sessionCloseRequest)
            {
                handleSessionCloseRequest(sessionCloseRequest);
            }
            else if (messageObj is Messaging.RegisterPlayerRequestMessage registerPlayerRequest)
            {
                handleRegisterPlayerRequest(registerPlayerRequest);
            }
            else if (messageObj is Messaging.DeregisterPlayerRequestMessage deregisterPlayerRequest)
            {
                handleDeregisterPlayerRequest(deregisterPlayerRequest);
            }
            else if (messageObj is Messaging.PlayerUpdateRequestMessage playerUpdateRequest)
            {
                handlePlayerUpdateRequest(playerUpdateRequest);
            }
            else if (messageObj is Messaging.PongMessage pongmsg)
            {
                //pong recieved, client is still connected
                awaitingPong = false;
            }

            if(active == true)
            {
                WaitForMessage();
            }
        }

        public async void SendKeepAlive()
        {
            if(awaitingPong)
            {
                //user has not responded to the last ping, likely disconnected
                handleDisconnect();
                return;
            }
            string pingMessage = "{\"type\":\"ping\"}";
            byte[] outBuffer = Encoding.ASCII.GetBytes(pingMessage);
            await stream.WriteAsync(outBuffer, 0, outBuffer.Length);
            awaitingPong = true;
            await Task.Delay(30000);
            SendKeepAlive();

        }

        public async void handleDisconnect()
        {
            active = false;
            //deregister player from any active session
            if (session != null)
            {
                leaveSession(session.Uuid);
            }
            server.purgeUserConnection(user.Uuid);
        }

        private async void handleAuthorizeRequest(Messaging.AuthorizeRequestMessage authRequest)
        {
            //authorize the user if they aren't already and return a UUID for thier access token
            if(server.getUserConnection(authRequest.Uuid) == null) {
                user = new VrppUser { Uuid = authRequest.Uuid, Username = authRequest.Username };
                AccessToken = Guid.NewGuid().ToString();
                var message = new Messaging.AuthorizeResponseMessage();
                message.AccessToken = this.AccessToken;
                sendToUser(message);
            }
        }

        private async void handleSessionCreateRequest(Messaging.SessionCreateRequestMessage sessionCreateRequest)
        {
            if (sessionCreateRequest.AccessToken != AccessToken)
            {
                return;
            }
            if (session != null)
            {
                return;

            }
            //create a new session if the user isn't currently responsible for one

            var newSession = new VrppSession();
            newSession.HostUser = user;
            newSession.Uuid = Guid.NewGuid().ToString();
            server.sessions.Add(newSession.Uuid, newSession);
            joinSession(newSession.Uuid);

            var responseMessage = new Messaging.RegisterPlayerResponseMessage();
            responseMessage.Uuid = player.Uuid;
            sendToUser(responseMessage);

            var notificationMessage = new Messaging.RegisterPlayerNotificationMessage();
            notificationMessage.player = player;
            sendToOtherPlayers(notificationMessage);
        }

        private async void handleSessionCloseRequest(Messaging.SessionCloseRequestMessage sessionCloseRequest)
        {
            //close a session if the user is currently responsible for one
            if (sessionCloseRequest.AccessToken != AccessToken)
            {
                return;
            }
            closeSession(sessionCloseRequest.SessionUuid);


            var sessMessage = new Messaging.SessionClosedNotificationMessage();
            sessMessage.Uuid = sessionCloseRequest.SessionUuid;
            sendToOtherPlayers(sessMessage);
        }

        private async void handleRegisterPlayerRequest(Messaging.RegisterPlayerRequestMessage registerPlayerRequest)
        {
            //register player with the indicated session as long as they arent part of a session already
            if (registerPlayerRequest.AccessToken != AccessToken)
            {
                return;
            }

            joinSession(registerPlayerRequest.SessionUuid);
            if (session == null || player == null)
            {
                return;
            }

            var responseMessage = new Messaging.RegisterPlayerResponseMessage();
            responseMessage.Uuid = player.Uuid;
            sendToUser(responseMessage);

            var notificationMessage = new Messaging.RegisterPlayerNotificationMessage();
            notificationMessage.player = player;
            sendToOtherPlayers(notificationMessage);
        }

        private async void handleDeregisterPlayerRequest(Messaging.DeregisterPlayerRequestMessage deregisterPlayerRequest)
        {
            //deregister player with the indicated session as long as they are part of a session
            if (deregisterPlayerRequest.AccessToken != AccessToken)
            {
                return;
            }
            if (player == null || deregisterPlayerRequest.PlayerUuid != player.Uuid)
            {
                return;
            }
            leaveSession(deregisterPlayerRequest.SessionUuid);

            var responseMessage = new Messaging.DeregisterPlayerResponseMessage();
            sendToUser(responseMessage);
        }

        private async void handlePlayerUpdateRequest(Messaging.PlayerUpdateRequestMessage playerUpdateRequest)
        {
            //update the players parameters and broadcast to the group
            if (playerUpdateRequest.AccessToken != AccessToken)
            {
                return;
            }
            if (session == null || playerUpdateRequest.SessionUuid != session.Uuid)
            {
                return;
            }

            updatePlayer(playerUpdateRequest.player);

            var updateMessage = new Messaging.PlayerUpdateNotificationMessage();
            updateMessage.player = player;
            sendToOtherPlayers(updateMessage);
        }

        private async void sendToUser(object message)
        {
            byte[] outBuffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));
            await stream.WriteAsync(outBuffer, 0, outBuffer.Length);
        }

        private async void sendToUser(byte[] outBuffer)
        {
            await stream.WriteAsync(outBuffer, 0, outBuffer.Length);
        }

        private async void sendErrorToUser(string errorMessage)
        {
            var message = new Messaging.ErrorResponseMessage();
            message.message = errorMessage;
            sendToUser(message);
        }

        private async void sendToOtherPlayers(object message)
        {
            byte[] outBuffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));
            foreach (VrppPlayer sessionPlayer in session.Players.Values)
            {
                if (player.Uuid == sessionPlayer.Uuid) continue; 
                server.getUserConnection(sessionPlayer.User.Uuid).sendToUser(outBuffer); 
            }
        }

        private async void joinSession(string sessionId)
        {
            if(!server.sessions.ContainsKey(sessionId))
            {
                sendErrorToUser("The reqested session didn't exist");
                return;
            }
            if(session != null)
            {
                sendErrorToUser("You are already in an active session");
                return;
            }
            session = server.sessions[sessionId];
            
            player = new VrppPlayer();
            player.Uuid = Guid.NewGuid().ToString();
            player.User = user;
            session.Players.Add(player.Uuid, player);

            
        }

        private async void leaveSession(string sessionId)
        {
            if (!server.sessions.ContainsKey(sessionId))
            {
                sendErrorToUser("The reqested session didn't exist");
                return;
            }
            if (session == null || session.Uuid != sessionId)
            {
                sendErrorToUser("You are not in the specified session");
                return;
            }
            session.Players.Remove(player.Uuid);
            var deregMessage = new Messaging.DeregisterPlayerNotificationMessage();
            deregMessage.PlayerUuid = player.Uuid;
            sendToOtherPlayers(deregMessage);
            if (session.HostUser.Uuid == user.Uuid)
            {
                closeSession(sessionId);
            }      
        }

        private async void closeSession(string sessionId)
        {
            if (!server.sessions.ContainsKey(sessionId))
            {
                sendErrorToUser("The reqested session didn't exist");
                return;
            }
            if (session == null || session.Uuid != sessionId)
            {
                sendErrorToUser("You are not in the specified session");
                return;
            }
            if (session.HostUser.Uuid != user.Uuid)
            {
                sendErrorToUser("You are not the owner of this session");
                return;
            }

            server.purgeSession(session.Uuid);
        }

        private async void updatePlayer(VrppPlayer newPlayer)
        {
            if(player == null)
            {
                sendErrorToUser("You are not registered as a player in any session");
                return;
            }
            player.BoneNames = newPlayer.BoneNames;
            player.BonePositions = newPlayer.BonePositions;
            player.BoneRotations = newPlayer.BoneRotations;

            player.BlendshapeNames = newPlayer.BlendshapeNames;
            player.BlendshapeValues = newPlayer.BlendshapeValues;

            player.AnimationParameters = newPlayer.AnimationParameters;
            player.AnimationParameterValues = newPlayer.AnimationParameterValues;

        }

    }
}
