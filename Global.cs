using System;
using System.Collections.Generic;
using System.Text;

namespace S7Test
{
    internal static class Global
    {
        internal static Configuration Configuration = new Configuration();

        internal static S7.Net.Plc Plc;
    }
}
