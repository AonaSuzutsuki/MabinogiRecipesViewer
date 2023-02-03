﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateLib.Update;

namespace Updater.Models
{
    public class UpdateInfo
    {
        public int Pid { get; }

        public string FileName { get; }

        public UpdateClient Client { get; }

        public string ExtractDirectoryPath { get; }

        public UpdateInfo(int pid, string fileName, UpdateClient updateClient, string extractDirectoryPath)
        {
            Pid = pid;
            FileName = fileName;
            Client = updateClient;
            ExtractDirectoryPath = extractDirectoryPath;
        }
    }
}
