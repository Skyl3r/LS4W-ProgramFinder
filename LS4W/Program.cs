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
                        installedPrograms.Add(app);
                    }
                }
            }

            foreach (WindowsProgram app in installedPrograms)
            {
                if (!File.Exists(app.executableLocation))
                    continue;

                var versionInfo = FileVersionInfo.GetVersionInfo(app.executableLocation);
                app.applicationName = versionInfo.FileDescription;
                app.publisherName = versionInfo.CompanyName;
                app.iconPaths = new List<string>();

                IconExtractor ie = new IconExtractor(app.executableLocation);
                if (ie.Count == 0)
                    continue;
                Icon icon = ie.GetIcon(0);
                Icon[] iconVariations = IconUtil.Split(icon);


                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string iconPath = assemblyPath + @"\icons\";
                if (!Directory.Exists(iconPath))
                {
                    Directory.CreateDirectory(iconPath);
                }

                for (int a = 0; a < iconVariations.Count(); a++)
                {
                    string iconExportName = Path.GetFileName(app.executableLocation) + "_" + a + ".png";
                    Bitmap iconAsBitmap = iconVariations[a].ToBitmap();
                    iconAsBitmap.Save(iconPath + iconExportName, ImageFormat.Png);
                    app.iconPaths.Add(iconPath + iconExportName);
                }
            }



            string jsonString = JsonSerializer.Serialize(installedPrograms);
            Console.WriteLine(jsonString);
        }
    }
}
