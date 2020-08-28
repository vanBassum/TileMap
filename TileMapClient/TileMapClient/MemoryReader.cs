using FRMLib;
using STDLib.Misc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TileMapLib;

namespace TileMapClient
{
    public class MemoryReader
    { 
        MemoryManager memoryManager = new MemoryManager();
        public event EventHandler<PlayerInfo> OnPlayerInfoUpdate;
        Task work;
        TaskEvents notify = new TaskEvents();
        System.Timers.Timer workTick;
        Map map;

        public MemoryReader(Map map)
        {
            this.map = map;
            work = new Task(Work);
            work.Start();
            workTick = new System.Timers.Timer();
            workTick.Interval = 250;
            workTick.Elapsed += (a, b) => notify.SetBits((UInt32)Events.TimerElapsed);
            workTick.Start();
        }

        ~MemoryReader()
        {
            Dispose();
        }

        public void Dispose()
        {
            memoryManager.detach();
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
                        nextState = State.Detached;
                        break;

                    case State.Detached:
                        memoryManager.attach(map.ProcessName);
                        if (memoryManager.IsAttached())
                            nextState = State.Attached;
                        break;

                    case State.Attached:
                        if (!memoryManager.IsAttached())
                            nextState = State.Detached;
                        PlayerInfo Playerinfo = new PlayerInfo();
                        Playerinfo.DirXPos = GetFloat(map.AddressDirX);
                        Playerinfo.DirYPos = GetFloat(map.AddressDirY);
                        Playerinfo.LocXPos = GetFloat(map.AddressLocX);
                        Playerinfo.LocYPos = GetFloat(map.AddressLocY);
                        Playerinfo.LastUpdate = DateTime.Now;
                        OnPlayerInfoUpdate?.Invoke(this, Playerinfo);
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


        public float GetFloat(string memPath)
        {
            string[] split = memPath.TrimEnd(']').Split(']');


            IntPtr ptr = IntPtr.Zero;

            for(int i=0; i<split.Length; i++)
            {
                UInt64 add;
                if (UInt64.TryParse(split[i], System.Globalization.NumberStyles.HexNumber, null, out add))
                {
                    if (i == split.Length - 1)
                        return memoryManager.Read_Float((IntPtr)(add + (UInt64)ptr));
                    else
                        ptr = memoryManager.Read_Address((IntPtr)(add + (UInt64)ptr));
                }
                else
                {
                    ptr = memoryManager.GetProcessModuleBase(split[i]);
                }
            }

            return 0;
        }


        [Flags]
        enum Events
        {
            TimerElapsed = 1,
            StateChanged = 2,
        }


        enum State
        {
            Initial,
            Attached,
            Detached,
        }
    }


}
