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
                            // IconExtractor/IconUtil for .NET
                            // Copyright (C) 2014 Tsuda Kageyu. All rights reserved.

                            // Redistribution and use in source and binary forms, with or without
                            // modification, are permitted provided that the following conditions
                            // are met:

                            // 1. Redistributions of source code must retain the above copyright
                            //     notice, this list of conditions and the following disclaimer.
                            // 2. Redistributions in binary form must reproduce the above copyright
                            //     notice, this list of conditions and the following disclaimer in the
                            //     documentation and/or other materials provided with the distribution.

                            // THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
                            // "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
                            // TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
                            // PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER
                            // OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
                            // EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
                            // PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
                            // PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
                            // LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
                            // NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
                            // SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
