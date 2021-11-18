using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace silverworker_discord
{
    public class ExternalProcess
    {
        public static bool GoPlz(string commandPath, string commandArguments)
        {
            var process = readableProcess(commandPath, commandArguments);
            var outputData = new StringBuilder();
            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                outputData.Append(e.Data);
            });
            var errorData = new StringBuilder();
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                errorData.Append(e.Data);
            });

            try
            {
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
            catch(Exception e)
            {
                var dumpDir = $"fail{DateTime.Now.ToFileTimeUtc()}";
                var outputFilename = $"{dumpDir}/output0.log";
                var errorFilename = $"{dumpDir}/error0.err";
                if(!Directory.Exists(dumpDir))
                {
                    Directory.CreateDirectory(dumpDir);
                }
                else
                {
                    var i = 0;
                    foreach(var file in Directory.GetFiles(dumpDir))
                    {
                        var thisNummatch = Regex.Matches(Path.GetFileNameWithoutExtension(file), "[^\\d](\\d+)\\.err$").LastOrDefault().Value;
                        if(!string.IsNullOrWhiteSpace(thisNummatch))
                        {
                            var thisNumval = 0;
                            if(int.TryParse(thisNummatch, out thisNumval) && thisNumval > i)
                            {
                                i = thisNumval;
                            }
                        }
                    }
                    outputFilename = $"{dumpDir}/output{i}.log";
                    errorFilename = $"{dumpDir}/error{i}.err";
                }
                File.WriteAllText(outputFilename, outputData.ToString());
                File.WriteAllText(errorFilename, errorFilename.ToString());
                return false;
            }
            return true;
        }
        private static Process readableProcess(string commandPath, string commandArguments)
        {
            var pi = new ProcessStartInfo(commandPath, commandArguments);
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            pi.RedirectStandardError = true;
            pi.RedirectStandardOutput = true;
            var process = new Process();
            process.StartInfo = pi;
            return process;
        }
    }
}