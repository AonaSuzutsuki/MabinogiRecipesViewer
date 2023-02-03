﻿using System.IO;
using System.IO.Compression;

namespace CookInformationViewer.Models.Updates
{
    public static class Zip
    {
        public static void Extract(string zipPath, string extractDirPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                var outPath = entry.FullName;
                if (outPath.EndsWith("/"))
                {
                    var di = new DirectoryInfo(extractDirPath + @"\" + outPath);
                    if (!di.Exists)
                        di.Create();
                }
                else
                {
                    entry.ExtractToFile(Path.Combine(extractDirPath, entry.FullName), true);
                }
            }
        }
    }
}
