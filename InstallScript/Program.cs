using System;
using System.IO;

namespace InstallScript
{
    class Program
    {
        static void Main(string[] args)
        {
            string invoker_dir = Directory.GetCurrentDirectory();

            string target_dir = "C:/Program Files/ACTelemetry";

            if (!Directory.Exists(target_dir))
            {
                Directory.CreateDirectory(target_dir);
            }

            if (File.Exists(target_dir + "/Backend.dll"))
            {
                File.Delete(target_dir + "/Backend.dll");
            }

            File.Move(invoker_dir + "/Backend.dll", target_dir + "/Backend.dll");
        }
    }
}
