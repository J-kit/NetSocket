﻿using Fluffy.Net.Packets;
using Fluffy.Net.Packets.Modules;
using Fluffy.Net.Packets.Modules.Raw;
using Fluffy.Net.Packets.Modules.Streaming;

using System;
using System.Net.Sockets;

#if NET40
    using system = Fluffy.Compatibility;
#else
using system = System;
#endif

namespace Fluffy.Net
{
    public class ConnectionInfo : IDisposable

    {
        public Guid Identifier { get; }

        public system::EventHandler<ConnectionInfo> OnDisposed;
        public system::EventHandler<ConnectionInfo> OnDisposing;

        public Socket Socket { get; set; }

        public FluffySocket FluffySocket { get; set; }

        internal Receiver Receiver { get; private set; }
        public Sender Sender { get; private set; }

        public PacketRouter PacketHandler { get; private set; }
        public StreamPacketHandler StreamPacketHandler { get; }

        public ConnectionInfo(FluffySocket fluffySocket)
            : this(fluffySocket.Socket, fluffySocket)
        {
        }

        public ConnectionInfo(Socket socket, FluffySocket fluffySocket)
        {
            Identifier = Guid.NewGuid();
            Socket = socket;
            FluffySocket = fluffySocket;

            PacketHandler = new PacketRouter(this);

            Sender = new Sender(this);
            Receiver = new Receiver(socket);

            Receiver.OnReceive += PacketHandler.Handle;
            Receiver.OnDisposing += (sender, receiver) => Dispose();

#if DEBUG

            PacketHandler.RegisterPacket<DummyPacketHandler>();
#endif
            PacketHandler.RegisterPacket<FormattedPacketHandler>();
            StreamPacketHandler = PacketHandler.RegisterPacket<StreamPacketHandler>();

            //PacketHandler
            //    .On<ConnectionInfo>().Do(x => Console.Write($"You are awesome :3"))
            //    .Default(x => Console.Write($"Lol, Default"));
        }

        public void Dispose()
        {
            OnDisposing?.Invoke(this, this);
            Socket?.Dispose();
            // ReSharper disable once DelegateSubtraction
            Receiver.OnReceive -= PacketHandler.Handle;
            OnDisposed?.Invoke(this, this);
        }
    }
}