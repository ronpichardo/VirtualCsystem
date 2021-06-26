using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.WebScripting;
using Crestron.SimplSharpPro.UC;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.Fusion;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace VirtualControlBeta
{
    public class ControlSystem : CrestronControlSystem
    {
        HttpCwsServer _cwsServer;
        UcEngine mtrDevice;
        Xpanel testXpanel;
        FusionRoom fusion;
        public Config.ConfigSettings conf;
        //SshClient sshClient;
        //ShellStream shellStream;

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 50;

                // Would only be relevant to for VC4, hardware procs just return int of program slot(slot 1 returns 1)
                conf = new Config.ConfigSettings();
                conf.RoomId = InitialParametersClass.RoomId;
                conf.RoomName = InitialParametersClass.RoomName;
                conf.ShureIp = "0.0.0.0";

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                _cwsServer = new HttpCwsServer("");
                //_cwsServer.ReceivedRequestEvent += _cwsServer_ReceivedRequestEvent;
                var settingRoute = new HttpCwsRoute("settings");
                settingRoute.RouteHandler = new ConfigSettingsReqHandler(conf.RoomId, conf.RoomName, conf.ShureIp);
                _cwsServer.AddRoute(settingRoute);
                _cwsServer.Register();
                // This one returns blank on MC4, but im not using 3-series noise so don't need this path
                //ErrorLog.Error("ApplicationDirectory: {0}", Directory.GetApplicationDirectory());
                // This one returns simpl/app01 -> relevant since i'd like to host/serve files from this path
                ErrorLog.Error("ApplicationRootDirectory: {0}", Directory.GetApplicationRootDirectory());

                mtrDevice = new UcEngine(0x03, this);
                mtrDevice.Description = "Just a MSTeams Kit";

                mtrDevice.ExtenderAudioReservedSigs.Use();
                mtrDevice.ExtenderEthernetReservedSigs.Use();

                mtrDevice.SigChange += MtrDevice_SigChange; ;

                //Reserved sig handler initialization
                mtrDevice.ExtenderAudioReservedSigs.DeviceExtenderSigChange += ExtenderAudioReservedSigs_DeviceExtenderSigChange; ;
                mtrDevice.ExtenderEthernetReservedSigs.DeviceExtenderSigChange += ExtenderEthernetReservedSigs_DeviceExtenderSigChange;
                mtrDevice.Register();

                testXpanel = new Xpanel(0x04, this);
                testXpanel.SigChange += TestXpanel_SigChange;
                testXpanel.Description = "XPanel for testing VC4";
                testXpanel.Register();

                fusion = new FusionRoom(0x08, this, "2MVC4Test", "0000");
                fusion.Description = "VirtualControlSharp Test";
                fusion.Register();

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void ExtenderEthernetReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void ExtenderAudioReservedSigs_DeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void MtrDevice_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void TestXpanel_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    if (args.Sig.BoolValue)
                    {
                        switch (args.Sig.Number)
                        {
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                                currentDevice.BooleanInput[args.Sig.Number].BoolValue = !currentDevice.BooleanOutput[args.Sig.Number].BoolValue;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void _cwsServer_ReceivedRequestEvent(object sender, HttpCwsRequestEventArgs args)
        {
            if (args.Context.Request.HttpMethod == "PUT")
            {
                args.Context.Response.StatusCode = 200;
                args.Context.Response.StatusDescription = "OK";
                args.Context.Response.Write("{\"status\": \"No post/puts have yet been implemented\"}", true);
            }
            else
            {
                args.Context.Response.StatusCode = 200;
                args.Context.Response.StatusDescription = "OK";
                args.Context.Response.ContentType = "application/json";
                JObject resObj = new JObject
                {
                    {
                        "data", new JObject
                        {
                            { "roomid", conf.RoomId },
                            { "roomname", conf.RoomName }
                        }
                    }
                };
                string jsonResponse = JsonConvert.SerializeObject(resObj);
                args.Context.Response.Write(jsonResponse, true);
            }
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void _ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void _ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    if (_cwsServer != null)
                    {
                        _cwsServer.Unregister();
                        _cwsServer.Dispose();
                    }                    
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void _ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    if (_cwsServer != null)
                    {
                        _cwsServer.Unregister();
                        _cwsServer.Dispose();
                    }
                    break;
            }

        }
    }
}