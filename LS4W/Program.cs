using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using TsudaKageyu;

namespace LS4W
{
    class Program 
    {
        static void Main(string[] args) 
        {
            List<WindowsProgram> installedPrograms = new List<WindowsProgram>(); 
            string registryKey_AppPaths = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

            //Get executables
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey_AppPaths))
            {
                foreach (string subkeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                    {
                        object rawLocation = subkey.GetValue("");
                        object rawPath = subkey.GetValue("Path");
                        if (rawLocation is null)
                            continue;
                        if (rawPath is null)
                            continue;

                        WindowsProgram app = new WindowsProgram();
                        app.executableLocation = rawLocation.ToString();
                        app.installLocation = rawPath.ToString();
                        
                        var iconB64 = "";
                        if (File.Exists(app.executableLocation))
                        {
                            IconExtractor ie = new IconExtractor(app.executableLocation);
                            if (ie.Count > 0)
                            {
                                Icon icon = ie.GetIcon(0);
                                Icon[] iconVariations = IconUtil.Split(icon);
                                for (int a = 0; a < iconVariations.Count(); a++)
                                {
                                    using (var ms = new MemoryStream())
                                    {    
                                        using (var iconAsBitmap = new Bitmap(iconVariations[a].ToBitmap()))
                                        {
                                            iconAsBitmap.Save(ms, ImageFormat.Png);
                                            var tmpIconB64 = Convert.ToBase64String(ms.GetBuffer());
                                            if (tmpIconB64.Length > iconB64.Length)
                                                iconB64 = tmpIconB64;
                                        }
                                    }
                                }
                            }
                        }
                        app.icon = iconB64;

                        installedPrograms.Add(app);
                    }
                }
            }

            string jsonString = JsonSerializer.Serialize(installedPrograms);
            Console.WriteLine(jsonString);
        }
    }
}
