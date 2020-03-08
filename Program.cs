using System;
using System.Diagnostics;

namespace S7Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Setup(args);
                
                Open();
                
                if (Global.Configuration.Interactive)
                {
                    Menu();
                }

                Close();
            }
            catch (Exception x)
            {
                Energy.Core.Tilde.WriteException(x, true);
            }
            if (Global.Configuration.Interactive || Debugger.IsAttached)
            {
                Energy.Core.Tilde.Pause();
            }
        }

        private static void Setup(string[] args)
        {
            Energy.Base.Command.Arguments arguments = new Energy.Base.Command.Arguments(args)
                .Parameter("type").Alias("t", "type")
                .Parameter("host").Alias("h", "host")
                .Parameter("port").Alias("p", "port")
                .Parameter("rack").Alias("r", "rack")
                .Parameter("slot").Alias("s", "slot")
                .Switch("interactive").Alias("i", "interactive")
                .Switch("quiet").Alias("q", "quiet")
                .Parse()
                ;
            if (!arguments["type"].IsEmpty)
            {
                Global.Configuration.Type = arguments["type"].Value;
            }
            if (!arguments["host"].IsEmpty)
            {
                Global.Configuration.Host = arguments["host"].Value;
            }
            if (!arguments["port"].IsEmpty)
            {
                Global.Configuration.Port = arguments["port"].Value;
            }
            if (!arguments["rack"].IsEmpty)
            {
                Global.Configuration.Rack = arguments["rack"].Value;
            }
            if (!arguments["slot"].IsEmpty)
            {
                Global.Configuration.Slot = arguments["slot"].Value;
            }
            Global.Configuration.Interactive = !arguments["interactive"].IsEmpty;
            Global.Configuration.Quiet = !arguments["quiet"].IsEmpty;
        }

        private static void Open()
        {
            string type = Global.Configuration.Type ?? Energy.Core.Tilde.Ask("S7 Type", "1500");
            var cpu = S7.Net.CpuType.S71500;
            switch ((type ?? "").ToUpper())
            {
                case "1200":
                    cpu = S7.Net.CpuType.S71200;
                    break;
                case "300":
                    cpu = S7.Net.CpuType.S7300;
                    break;
                case "200":
                    cpu = S7.Net.CpuType.S7200;
                    break;
                case "400":
                    cpu = S7.Net.CpuType.S7400;
                    break;
                case "1500":
                    cpu = S7.Net.CpuType.S71500;
                    break;
                case "LOGO":
                    cpu = S7.Net.CpuType.Logo0BA8;
                    break;
                default:
                    Energy.Core.Tilde.WriteLine("~r~Uknown S7 type, possible values are ~y~200~r~, ~y~300~r~, ~y~400~r~, ~y~1200~r~, ~y~1500~r~, and ~y~LOGO~r~!");
                    return;
            }
            string host = Global.Configuration.Host ?? Energy.Core.Tilde.Ask("S7 Host", "192.168.0.1");
            int port = Energy.Base.Cast.As<int>(Global.Configuration.Port);
            if (0 == port)
            {
                port = Energy.Core.Tilde.Ask<int>("S7 Port", 102);
            }
            short rack = Energy.Base.Cast.As<short>(Global.Configuration.Rack);
            if ("0" != Global.Configuration.Rack && 0 == rack)
            {
                rack = Energy.Core.Tilde.Ask<short>("S7 Rack", 0);
            }
            short slot = Energy.Base.Cast.As<short>(Global.Configuration.Slot);
            if ("0" != Global.Configuration.Slot && 0 == slot)
            {
                slot = Energy.Core.Tilde.Ask<short>("S7 Slot", 1);
            }
            var plc = new S7.Net.Plc(cpu, host, port, rack, slot);
            plc.Open();
            if (!plc.IsConnected)
            {
                Energy.Core.Tilde.WriteLine("~r~Can't connect to PLC");
                return;
            }
            if (!Global.Configuration.Quiet)
            {
                Energy.Core.Tilde.WriteLine("~lc~PLC connection open");
            }
            Global.Plc = plc;
        }

        private static void Close()
        {
            if (Global.Plc != null)
            {
                if (Global.Plc.IsConnected)
                {
                    Global.Plc.Close();
                    if (!Global.Configuration.Quiet)
                    {
                        Energy.Core.Tilde.WriteLine("~lc~PLC connection closed");
                    }
                }
                else
                {
                    if (!Global.Configuration.Quiet)
                    {
                        Energy.Core.Tilde.WriteLine("~lm~PLC not connected");
                    }
                }
            }
        }

        private static void Menu()
        {
            while (true)
            {
                try
                {
                    Energy.Core.Tilde.WriteLine(" ~lg~r~0~ - read data");
                    Energy.Core.Tilde.WriteLine(" ~lg~q~0~ - exit program");
                    var input = Energy.Core.Tilde.Input("~b~Command ~m~:~0~ ", "");
                    input = input.Trim().ToLowerInvariant();
                    switch (input)
                    {
                        default:
                            Energy.Core.Tilde.WriteLine("~r~Unknown command");
                            break;
                        case "":
                            break;
                        case "r":
                            CommandRead();
                            break;
                        case "q":
                            return;
                    }
                }
                catch (Exception x)
                {
                    Energy.Core.Tilde.WriteLine("~r~" + x.Message);
                }
            }
        }

        private static int lastReadBlock = 10;
        private static int lastReadStart = 0;
        private static int lastReadCount = 1;

        private static void CommandRead()
        {
            lastReadBlock = Energy.Core.Tilde.Ask<int>("Data block", lastReadBlock);
            lastReadStart = Energy.Core.Tilde.Ask<int>("Data start", lastReadStart);
            lastReadCount = Energy.Core.Tilde.Ask<int>("Data count", lastReadCount);
            var plc = Global.Plc;
            byte[] data = plc.ReadBytes(S7.Net.DataType.DataBlock, lastReadBlock, lastReadStart, lastReadCount);
            string print = Energy.Base.Hex.Print(data);
            Energy.Core.Tilde.WriteLine(print);
        }
    }
}
