﻿using Fluffy.IO.Buffer;

namespace Fluffy.Net.Packets.Modules.Raw
{
    public abstract class BasePacketHandler
    {
        public abstract byte OpCode { get; }

        internal ConnectionInfo Connection { get; set; }

        public abstract void Handle(LinkedStream stream);
    }
}