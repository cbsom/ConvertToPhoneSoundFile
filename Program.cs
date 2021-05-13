using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ConvertToPhoneSoundFile.Properties;
using Serilog;

namespace ConvertToPhoneSoundFile
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static void StartupInit()
        {
            //Setup logger            
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"LogFiles\log.txt");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath, rollOnFileSizeLimit: true, fileSizeLimitBytes: 1048576)
                .CreateLogger();
        }

        public static string GetSoundFilePath(string fileName)
        {
            return Path.Combine(Settings.Default.SaveInDirectory,
                $"{Path.GetFileNameWithoutExtension(fileName)}.wav");
        }

        public static bool DoSoundFileConversion(string uploadedFile)
        {
            string soundFilePath = GetSoundFilePath(uploadedFile);
            const int timeout = 10000;
            using var p = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg.exe",
                    Arguments =
                        $"-i \"{uploadedFile}\" {Settings.Default.ConvertOptions} \"{soundFilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            using (var outputWaitHandle = new AutoResetEvent(false))
            {
                using var errorWaitHandle = new AutoResetEvent(false);
                p.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        Log.Information($"Converting \"{soundFilePath}\": {e.Data}");
                    }
                };
                p.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        Log.Error($"Converting \"{soundFilePath}\": {e.Data}");
                    }
                };

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                if (p.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    Log.Information($"Successfully converted {uploadedFile} to {soundFilePath}");
                    return true;
                }
            }

            Log.Information($"TIMEOUT - Failed to convert {uploadedFile} to {soundFilePath}");

            return false;
        }
    }
}