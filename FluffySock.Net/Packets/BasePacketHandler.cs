﻿using Fluffy.Extensions;
using Fluffy.IO.Buffer;

using System;
using System.IO;

namespace Fluffy.Net.Packets
{
    public enum Packet : byte
    {
        TestPacket = 1,
        FormattedPacket
    }

    public abstract class BasePacket
    {
        public abstract byte OpCode { get; }

        internal ConnectionInfo Connection { get; set; }

        public abstract void Handle(LinkedStream stream);
    }

    public class DummyPacket : BasePacket
    {
        public override byte OpCode => (int)Packet.TestPacket;

        public override void Handle(LinkedStream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }
    }

    public class FormattedPacket : BasePacket
    {
        public override byte OpCode => (int)Packet.FormattedPacket;

        public override void Handle(LinkedStream stream)
        {
            var result = stream.Deserialize();
            Connection.TypedPacketHandler.Handle(result);
        }
    }
}