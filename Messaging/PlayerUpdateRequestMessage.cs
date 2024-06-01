using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class PlayerUpdateRequestMessage
    {
        public string type = "update-player-request";
        public string AccessToken { get; set; }
        public string SessionUuid { get; set; }
        public VrppPlayer player { get; set; }
    }
}
