﻿using System;

#if !NET40

using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using AllInOneExample_FullFramework.Models;
using Fluffy.Net;

#endif

namespace AllInOneExample_FullFramework
{
    internal class PacketExample
    {
#if NET40

        public PacketExample Initialize()
        {
            Console.WriteLine("NET 4.0 doesn't support async/await");
            return this;
        }

#else
        private bool _initialized;
        private readonly FluffyServer _server;
        private readonly FluffyClient _client;
        private static Stopwatch _sw;

        public bool ParallelExample { get; set; }

        public PacketExample(FluffyServer server, FluffyClient client)
        {
            _server = server;
            _client = client;
        }

        public PacketExample Initialize()
        {
            if (_initialized)
            {
                return this;
            }

            _initialized = true;

            _server.OnNewConnection += OnNewConnection;
            _client.Connection.PacketHandler.On<MyAwesomeClass>().Do(Awesome);
            _server.PacketHandler.On<MyAwesomeClass>().Do(Awesome);
            return this;
        }

        private static MyAwesomeClass Awesome(MyAwesomeClass awesome)
        {
            awesome.Packets++;
            return awesome;
        }

        private async void OnNewConnection(object sender, ConnectionInfo connection)
        {
            if (ParallelExample)
            {
                await SendMultipleStresstestAsync(connection);
            }
            else
            {
                await SendNonParallel(connection);
            }
        }

        public static async Task SendNonParallel(ConnectionInfo connection)
        {
            var dummy = new MyAwesomeClass
            {
                Id = 1,
                AwesomeString = "AWESOME!!"
            };

            while (true)
            {
                dummy = await connection.Sender.Send<MyAwesomeClass>(dummy);
                Console.WriteLine($"{dummy.Id} \t {dummy.Packets} \t {dummy.AwesomeString}");
            }
        }

        public static async Task SendMultipleStresstestAsync(ConnectionInfo connection)
        {
            int exitCounter = 0;
            var dummy = GenerateDummyData();
            _sw = Stopwatch.StartNew();

            while (true)
            {
                var tList = dummy.Select(x => connection.Sender.Send<MyAwesomeClass>(x)).ToList();
                await Task.WhenAll(tList.Select(x => x.Task));
                for (int i = 0; i < tList.Count; i++)
                {
                    dummy[i] = tList[i].Result;
                }

                if (dummy[0].Packets % 100 == 0)
                {
                    _sw.Stop();
                    var count = dummy.Sum(x => x.Packets);

                    Console.WriteLine($"AVG Delay: {(_sw.Elapsed.TotalMilliseconds / count):##0.00000} ms " +
                                      $"{count}:  ({count * 2 / _sw.Elapsed.TotalMilliseconds} p/ms)");
                    exitCounter++;
                    if (exitCounter >= 1000)
                    {
                        Console.WriteLine($"Test Finished, exiting");
                        break;
                    }
                    _sw.Start();
                }
            }
        }

        private static List<MyAwesomeClass> GenerateDummyData()
        {
            var cid = 0;

            var awesomeList = new List<MyAwesomeClass>();
            for (int i = 0; i < 100; i++)
            {
                awesomeList.Add(new MyAwesomeClass
                {
                    Id = cid++,
                    AwesomeString = "AWESOME!!"
                });
            }

            return awesomeList;
        }

#endif
    }
}