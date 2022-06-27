using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeDLSharp;

namespace Shakalik
{
    public class YouTubeDLWrapper
    {
        internal static async Task DownloadVideo(string chatId,string urlVideo)
        {

            Process process = new Process();
            process.StartInfo.FileName = "youtube-dl";
            process.StartInfo.Arguments = $"-o \"{ProjectDir.basePath}\" \"{urlVideo}\"";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.Start();
        }   
    }
}

