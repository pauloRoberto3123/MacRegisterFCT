using System;
using System.Collections.Generic;
using System.IO;
namespace MacRegister.Service


{
    public class FileOperations
    {
        public FctLog GetLogInfo(string pathFile)
        {

            FctLog log = new FctLog();
            var lastLogLines = PreProcessLogFile(pathFile);

            foreach (string line in lastLogLines)
            {
                if (line.Contains("P/N : "))
                {
                    log.Serial = line.Split(':')[1].Trim();
                    //log.Serial = "BR01BA9223728AEA8FSD80089";
                }

                if (line.Contains("JIG : "))
                {
                    log.Jig = "-" + "0" + line.Split(':')[1].Trim();
                }
                if (line.Contains("// ETHERNET ID :"))
                {
                    log.Mac = line.Split(':')[1].Trim();
                }
                if (line.Contains("DATE : ") && line.Contains("TIME : "))
                {

                    string datePart = line.Split(new[] { "DATE : " }, StringSplitOptions.None)[1].Split(new[] { " TIME : " }, StringSplitOptions.None)[0].Trim();
                    string timePart = line.Split(new[] { "TIME : " }, StringSplitOptions.None)[1].Trim();


                    string dateTimeString = $"{datePart} {timePart}";

                    //Console.WriteLine($"Concatenated DateTime String: {dateTimeString}");


                    log.DateStart = dateTimeString;
                }

                if (line.Contains("TEST-TIME : "))
                {

                    log.DateEnd = line.Split(new[] { "TEST-TIME : " }, StringSplitOptions.None)[1].Trim();

                }
                if (line.Contains("PROGRAM : "))
                {
                    string programInfo = line.Split(new[] { ':' }, 2)[1].Trim();
                    string searchTerm = "Runtime";
                    int startIndex = programInfo.IndexOf(searchTerm);

                    if (startIndex != -1)
                    {
                        log.PgmVersion = programInfo.Substring(startIndex);
                    }
                }
                if (line.Contains("FAILITEM :"))
                {
                    log.FailureMessage = line.Split(':')[1].Trim();
                }

                if (line.Contains("RESULT : "))
                {
                    string result = line.Split(':')[1].Trim();
                    if (result.Trim().ToUpper() == "PASS")
                    {
                        log.TestResult = "P";
                    }

                    else
                    {
                        log.TestResult = "F";
                    }
                }
            }
            return log;

        }

        public List<string> PreProcessLogFile(string pathFile)
        {
            var lastLogLinesList = new List<string>();

            // Read all lines from the file
            var wholeLog = File.ReadAllText(pathFile);
            string[] splitLogs = wholeLog.Split(new[] { "#INIT" }, StringSplitOptions.None);
            string[] lastLogLines = splitLogs[splitLogs.Length - 1].Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var log in lastLogLines)
            {
                lastLogLinesList.Add(log);
            }

            return lastLogLinesList;
        }

        public void MoveFileToSccess(string pathFile, string pathSuccess)
        {
            // Cria a pasta com a data se não existir
            CreateDirectoryForDate(pathSuccess);

            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            string fileName = Path.GetFileName(pathFile);
            string timestamp = DateTime.Now.ToString("_yyyyMMddHHmmssfff");
            string destFile = Path.Combine(pathSuccess, dateFolder, timestamp + fileName);

            File.Move(pathFile, destFile);
            return;
        }

        public void MoveFileToError(string pathFile, string pathError)
        {

            // Cria a pasta com a data se não existir
            CreateDirectoryForDate(pathError);

            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            string fileName = Path.GetFileName(pathFile);
            string timestamp = DateTime.Now.ToString("_yyyyMMddHHmmssfff");
            string destFile = Path.Combine(pathError, dateFolder, timestamp + fileName);

            File.Move(pathFile, destFile);
            return;
        }

        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public void CreateDirectoryForDate(string basePath)
        {
            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            string fullPath = Path.Combine(basePath, dateFolder);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
        public bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        public bool IsRetest(string path)
        {
            var lines = File.ReadLines(path);
            foreach (var line in lines)
            {
                if (line.Contains("ETHERNET_ID [ALREADY WRITTEN]"))
                {
                    return true;
                }
            }
            return false;


        }
    }
}
