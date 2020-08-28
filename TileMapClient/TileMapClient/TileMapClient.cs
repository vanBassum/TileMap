using STDLib.Ethernet;
using STDLib.JBVProtocol;
using STDLib.JBVProtocol.Connections;
using STDLib.Misc;
using STDLib.Serializers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TileMapLib;
using System.Linq;
using TileMapLib.Commands;
using System.ComponentModel;
using System.Threading;

namespace TileMapClient
{


    public class TileMapClient
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
        public event EventHandler<TileServer> OnServerFound;
        public event EventHandler<MapInfo> OnMapInfoRecieved;
        public ThreadedBindingList<TileServer> Servers { get; } = new ThreadedBindingList<TileServer>();
        public ThreadedBindingList<PlayerInfo> Players { get; } = new ThreadedBindingList<PlayerInfo>();
        TileServer SelectedServer { get; set; }
        public TileMapClient()
        {
            connection = new TCPConnection(socket);
            client = new JBVClient(SoftwareID.TileMapClient);
            client.SetConnection(connection);
            client.OnMessageRecieved += Client_OnMessageRecieved;
            client.OnSoftwareIDRecieved += Client_OnSoftwareIDRecieved;

            work = new Task(Work);
            work.Start();
            workTick = new System.Timers.Timer();
            workTick.Interval = 1000;
            workTick.Elapsed += (a, b) => notify.SetBits((UInt32)Events.TimerElapsed);
            workTick.Start();
            socket.OnDisconnected += (a, b) => notify.SetBits((UInt32)Events.Disconnected);
        }

        private void Client_OnSoftwareIDRecieved(object sender, STDLib.JBVProtocol.IO.Frame e)
        {
            lock(Servers)
            {
                TileServer server = Servers.FirstOrDefault(a=>a.ID == e.SID);
                if (server == null)
                    Servers.Add(server = new TileServer() { ID = e.SID });
            }
        }

        private void Client_OnMessageRecieved(object sender, STDLib.JBVProtocol.IO.Frame e)
        {
            object o = serializer.Deserialize(e.PAY);
            if (o is TileBaseCmd cmd)
            {
                cmd.SID = e.SID;
                commands.Enqueue(cmd);
                notify.SetBits((UInt16)Events.CommandRecieved);
            }
        }

        public void ConnectToServer(TileServer server)
        {
            SelectedServer = server;
        }

        PlayerInfo playerinfo;
        public void SendPlayerInfo(PlayerInfo pi)
        {
            playerinfo = pi;
            notify.SetBits((UInt32) Events.UpdatePlayerInfo);
        }

        void ProcessCommand(TileBaseCmd command)
        {
            switch (command)
            {
                case TileCMD_GetServername cmd:

                    lock (Servers)
                    {
                        TileServer server = Servers.FirstOrDefault(a => a.ID == cmd.SID);
                        if (server != null)
                        {
                            if (server.Name != cmd.Servername)
                            {
                                server.Name = cmd.Servername;
                                OnServerFound?.Invoke(this, server);
                            }
                        }
                    }
                    break;

                case TileCMD_GetMapInfo cmd:
                    OnMapInfoRecieved?.Invoke(this, cmd.MapInfo);
                    break;

                case TileCMD_UpdatePlayerInfo cmd:
                    lock(Players)
                    {
                        PlayerInfo pi = Players.FirstOrDefault(p=>p.ID == cmd.Playerinfo.ID);
                        if (pi == null)
                            Players.Add(pi = new PlayerInfo() { ID = cmd.Playerinfo.ID });
                        pi.Populate(cmd.Playerinfo);
                    }
                    break;
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
                            //Find all servers and wait for the user to connect to one.
                            
                            client.RequestSoftwareID(SoftwareID.TileMapServer);

                            lock(Servers)
                            {
                                foreach (TileServer serv in Servers.Where(a => a.Name == null))
                                {
                                    client.SendMessage(serv.ID, serializer.Serialize(new TileCMD_GetServername()));
                                }
                            }
                            
                            if(commands.Any())
                            {
                                TileBaseCmd cmd = commands.Dequeue();
                                ProcessCommand(cmd);
                                if (commands.Any())
                                    notify.SetBits((UInt32)Events.CommandRecieved);
                            }
                            
                            if(SelectedServer != null)
                            {
                                nextState = State.ConnectedToServer;
                            }

                        }
                        break;


                    case State.ConnectedToServer:
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
                            if (events.HasFlag(Events.StateChanged))
                            {
                                //Request the map from the server
                                client.SendMessage(SelectedServer.ID, serializer.Serialize(new TileCMD_GetMapInfo()));
                            }

                            if(events.HasFlag(Events.UpdatePlayerInfo))
                            {
                                playerinfo.ID = client.ID;
                                client.SendMessage(SelectedServer.ID, serializer.Serialize(new TileCMD_UpdatePlayerInfo() { Playerinfo = playerinfo }));
                            }

                            if (commands.Any())
                            {
                                TileBaseCmd cmd = commands.Dequeue();
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
            TimerElapsed            = 1,
            StateChanged            = 2,
            Disconnected            = 4,
            CommandRecieved         = 8,
            SelectedServerChanged   = 16,
            UpdatePlayerInfo        = 32,
        }


        enum State
        {
            Initial,
            Disconnected,
            ConnectedNoLease,
            ConnectedWithLease,
            ConnectedToServer,
        }
    }

    public class TileServer : PropertySensitive
    {
        public UInt16 ID { get { return GetPar<UInt16>(0); } set { SetPar(value); } }
        public string Name { get { return GetPar<string>(null); } set { SetPar(value); } }

        public override string ToString()
        {
            return $"{Name} ({ID})";
        }
    }

}
