using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class DeregisterPlayerResponseMessage
    {
        public string type = "deregister-player-response";
        public string response = "ok";
    }
}
