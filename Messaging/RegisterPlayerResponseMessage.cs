using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class RegisterPlayerResponseMessage
    {
        public string type = "session-create-response";
        public string Uuid { get; set; }
    }
}
