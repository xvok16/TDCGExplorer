using System;
using TAHdecrypt;

namespace TSOConfigDump
{
    static class Program
    {
        static void Main(string[] args)
        {
            TSOConfig config = new TSOConfig();
            config.Dump();
        }
    }
}