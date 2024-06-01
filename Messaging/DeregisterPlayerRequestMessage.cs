using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class DeregisterPlayerRequestMessage
    {
        public string type = "deregister-player-request";
        public string AccessToken { get; set; }
        public string SessionUuid { get; set; }
        public string PlayerUuid { get; set; }
    }
}
