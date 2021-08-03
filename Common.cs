using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace setup_common
{
	public static class Common
	{
        public const string NAME = "Max 4ds Tools";
        public const string VERSION = "0.8.0";

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(Process process, int buffer = 1024) {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }

        public static bool IsMaxRunning(string exePath) {
            var procs = Process.GetProcessesByName("3dsmax");

            foreach (var proc in procs) {
                string name = "";
                try {
                    name = GetMainModuleFileName(proc);
                }
                catch (Exception) { }

                Debug.WriteLine(name);
                if (name == exePath) return true;
            }
            return false;
        }
    }
}
