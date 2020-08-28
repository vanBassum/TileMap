using STDLib.Ethernet;
using STDLib.JBVProtocol;
using STDLib.JBVProtocol.Connections;
using STDLib.Misc;
using STDLib.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TileMapLib;
using TileMapLib.Commands;

namespace TileMapServer
{
    public class TileMapServer
    {
        TcpSocketClient socket = new TcpSocketClient();
        TCPConnection connection;
        JBVClient client;
        ThreadedBindingList<PlayerInfo> tileClients = new ThreadedBindingList<PlayerInfo>();
        Task work;
        TaskEvents notify = new TaskEvents();
        System.Timers.Timer workTick;
        Queue<TileBaseCmd> commands = new Queue<TileBaseCmd>();
        JSON serializer = new JSON();

        public TileMapServer()
        {
            Settings.Load();

            connection = new TCPConnection(socket);
            client = new JBVClient(SoftwareID.TileMapServer);
            client.SetConnection(connection);
            client.OnMessageRecieved += Client_OnMessageRecieved;

            work = new Task(Work);
            work.Start();
            workTick = new System.Timers.Timer();
            workTick.Interval = 1000;
            workTick.Elapsed += (a, b) => notify.SetBits((UInt32)Events.TimerElapsed);
            workTick.Start();
            socket.OnDisconnected += (a, b) => notify.SetBits((UInt32)Events.Disconnected);
        }

        void ProcessCommand(TileBaseCmd command)
        {

            switch (command)
            {
                case TileCMD_GetServername cmd:
                    cmd.Servername = Settings.Servername;
                    client.SendMessage(cmd.SID, serializer.Serialize(cmd));
                    break;

                case TileCMD_GetMapInfo cmd:
                    cmd.MapInfo = Settings.MapInfo;
                    client.SendMessage(cmd.SID, serializer.Serialize(cmd));
                    break;

                case TileCMD_UpdatePlayerInfo cmd:
                    lock(tileClients)
                    {
                        PlayerInfo pi = tileClients.FirstOrDefault(p => p.ID == cmd.SID);
                        if (pi == null)
                            tileClients.Add(pi = new PlayerInfo() { ID = cmd.SID });
                        pi.Populate(cmd.Playerinfo);
                    }
                    break;
            }


        }

        private void Client_OnMessageRecieved(object sender, STDLib.JBVProtocol.IO.Frame e)
        {
            object o = serializer.Deserialize(e.PAY);
            if(o is TileBaseCmd cmd)
            {
                cmd.SID = e.SID;
                commands.Enqueue(cmd);
                notify.SetBits((UInt16)Events.CommandRecieved);
            }    
        }

        void Work()
        {
            State nextState = State.Initial;
            State actState = State.Initial;
            State prevState = State.Initial;

            while (true)
            {
                Events events = (Events)notify.WaitOne();

                switch (actState)
                {
                    case State.Initial:
                        if (socket.IsConnected)
                        {
                            nextState = State.ConnectedNoLease;
                        }
                        else if (socket.IsConnecting)
                        {

                        }
                        else
                        {
                            socket.ConnectAsync(Settings.ConnectionServer);
                        }
                        break;

                    case State.ConnectedNoLease:

                        if (events.HasFlag(Events.Disconnected))
                        {
                            nextState = State.Disconnected;
                        }
                        else if (client.HasLease)
                        {
                            nextState = State.ConnectedWithLease;
                        }

                        break;

                    case State.ConnectedWithLease:
                        if (events.HasFlag(Events.Disconnected))
                        {
                            nextState = State.Disconnected;
                        }
                        else if (!client.HasLease)
                        {
                            nextState = State.ConnectedNoLease;
                        }
                        else
                        {
                            //We are connected and have a lease.


                            foreach(PlayerInfo tx in tileClients)
                            {
                                foreach(PlayerInfo rx in tileClients.Where(p=>p != tx))
                                {
                                    client.SendMessage(rx.ID, serializer.Serialize(new TileCMD_UpdatePlayerInfo() { Playerinfo = tx }));
                                }
                            }



                            TileBaseCmd cmd;
                            if(commands.TryDequeue(out cmd))
                            {
                                ProcessCommand(cmd);
                                if (commands.Any())
                                    notify.SetBits((UInt32)Events.CommandRecieved);
                            }

                        }
                        break;
                }


                if (actState != nextState)
                {
                    notify.SetBits((UInt32)Events.StateChanged);
                    prevState = actState;
                    actState = nextState;
                    Console.WriteLine($"{this.GetType().Name}: State change '{prevState}' => {actState}");
                }
            }
        }


        



        [Flags]
        enum Events
        {
            TimerElapsed    = 1,
            StateChanged    = 2,
            Disconnected    = 4,
            CommandRecieved = 8,
        }


        enum State
        {
            Initial,
            Disconnected,
            ConnectedNoLease,
            ConnectedWithLease,
        }
    }

}
