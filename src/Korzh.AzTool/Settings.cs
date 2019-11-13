using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Korzh.AzTool
{
    class Settings
    {
        static Settings()
        {
            string homePath = Environment.OSVersion.Platform == PlatformID.Win32NT
                                ? Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%")
                                : Environment.GetEnvironmentVariable("HOME");

            GlobalConfigFilePath = Path.Combine(homePath, ".korzh", "aztool.config");

            LocalConnectionString = "UseDevelopmentStorage=true;";
        }


        public static readonly string GlobalConfigFilePath;

        public static readonly string LocalConnectionString;
    }
}
