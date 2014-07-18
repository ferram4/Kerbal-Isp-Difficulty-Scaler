/*
 * InstallChecker, originally by Majiir
 * Released into the public domain using a CC0 Public Domain Dedication: http://creativecommons.org/publicdomain/zero/1.0/
 */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalIspDifficultyScaler
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallChecker : MonoBehaviour
    {
        protected void Start()
        {
            var assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name).Where(a => a.url != "KerbalIspDifficultyScaler/Plugins");
            if (assemblies.Any())
            {
                var badPaths = assemblies.Select(a => a.path).Select(p => Uri.UnescapeDataString(new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath)).MakeRelativeUri(new Uri(p)).ToString().Replace('/', Path.DirectorySeparatorChar)));
                PopupDialog.SpawnPopupDialog("Incorrect KIDS Installation", "KIDS has been installed incorrectly and will not function properly. All KIDS files should be located in KSP/GameData/KerbalIspDifficultyScaler. Do not move any files from inside the KIDS folder.\n\nIncorrect path(s):\n" + String.Join("\n", badPaths.ToArray()), "OK", false, HighLogic.Skin);
            }
        }
    }
    
}
