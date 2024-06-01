using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class AuthorizeRequestMessage
    {
        public string type = "authorize-request";
        public string Uuid { get; set; }
        public string Username { get; set; }
    }
}
