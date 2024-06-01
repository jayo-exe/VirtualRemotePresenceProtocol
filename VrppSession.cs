using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol
{
    //Represents a session, which contains players to allow grouped communication
    class VrppSession
    {
        public string Uuid { get; set; }
        public VrppUser HostUser { get; set; }
        public Dictionary<string, VrppPlayer> Players { get; set; }
    }
}
