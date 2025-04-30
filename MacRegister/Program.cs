using MacRegister.Dtos;
using MacRegister.InfraStructure;
using MacRegister.Model.Jems4Api;
using MacRegister.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace MacRegister
{
    internal class Program
    {

        private readonly FileOperations _fileOperations;
        private readonly Constants _constants;
        private readonly MacAPI _macApi;
        private readonly JabilApi _jabilApi;

        public Program(Constants constants, FileOperations fileOperations, MacAPI macApi, JabilApi jabilApi)
        {
            _constants = constants;
            _fileOperations = fileOperations;
            _macApi = macApi;
            _jabilApi = jabilApi;
        }

        public static void Main(string[] args)
        {
            HttpClient httpClient = new HttpClient();
            Constants constants = new Constants();
            FileOperations fileOperations = new FileOperations();

            MacAPI macAPI = new MacAPI(httpClient);
            JabilApi jabilApi = new JabilApi(httpClient);

            Program program = new Program(constants, fileOperations, macAPI, jabilApi);

            MonitorLatestFile(constants, program);

        }

        private static void MonitorLatestFile(Constants _constants, Program program)
        {
            string lastProcessedFile = null;
            DateTime lastProcessedTime = DateTime.MinValue;

            while (true)
            {
                var latestFile = GetMostRecentlyModifiedFile(_constants.PathLog);

                if (!string.IsNullOrEmpty(latestFile))
                {
                    DateTime modifiedTime = File.GetLastWriteTime(latestFile);

                    // Process only if the timestamp is different from the last tracked one
                    if (modifiedTime != lastProcessedTime)
                    {
                        program.Run(_constants, latestFile);
                        lastProcessedFile = latestFile;
                        lastProcessedTime = modifiedTime;
                    }
                }

                Thread.Sleep(5000); // Check every 5 seconds
            }
        }
        private static string GetMostRecentlyModifiedFile(string directory)
        {
            var files = Directory.GetFiles(directory);
            return files.Length > 0
                ? files.OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault()
                : null;
        }

        private void Run(Constants _constants, string pathWatch)
        {
            Console.WriteLine($"Iniciando o processamento do arquivo: {pathWatch}");

            Thread.Sleep(2000);
            string filePath = pathWatch;
            if (!File.Exists(filePath))
            {
                return;
            }
            try
            {
                FctLog log = _fileOperations.GetLogInfo(filePath);
                bool isRetest = _fileOperations.IsRetest(filePath);


                MesApiRequest mesApiRequest = new MesApiRequest
                {
                    Serial = log.Serial,
                    Customer = "SAMSUNG",
                    Division = "SAMSUNG",
                    Step = _constants.Step,
                    Equipment = _constants.Equipment + log.Jig,
                    TestResult = log.TestResult,
                    FailureLabel = log.FailureMessage,
                };

                MacInsertRequest macInsertRequest = new MacInsertRequest
                {
                    Serial = log.Serial,
                    Mac = log.Mac

                };

                if ((
                    //string.IsNullOrEmpty(log.Mac) && !isRetest && log.TestResult == "P" ||
                    string.IsNullOrEmpty(log.Serial) ||
                    string.IsNullOrEmpty(log.PgmVersion)


                    ))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n************ CAMPOS OBRIGATÓRIOS AUSENTES ************\n");
                    Console.ResetColor();

                    if (string.IsNullOrEmpty(log.Serial))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Serial não informado");
                        Console.WriteLine($"Serial: {log.Serial}");
                        Console.ResetColor();
                    }

                    if (string.IsNullOrEmpty(log.Mac) && _constants.WillGetMac == "True")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Mac não informado");
                        Console.WriteLine($"Serial: {log.Serial}");
                        Console.ResetColor();
                    }

                    if (string.IsNullOrEmpty(log.PgmVersion))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Versão do PGM não informada");
                        Console.WriteLine($"Serial: {log.Serial}");
                        Console.ResetColor();
                    }

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n-----------------------------------------\n");
                    Console.ResetColor();

                    //_fileOperations.MoveFileToError(filePath, _constants.PathProcessed);
                    return;

                }

                string resultPGM = _constants.PGM;
                if (_constants.WillGetMac == "True")
                {
                    resultPGM = _macApi.GetPgmVersion(log.Serial).ToUpper();
                }

                if (resultPGM == "FAIL")
                {
                    // Título em vermelho para indicar falha
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n************ PGM NÃO CONFIGURADO NO MES************\n");
                    Console.ResetColor();

                    // Informações do Log em caso de falha
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Serial: {log.Serial}");
                    Console.ResetColor();

                    // Separador em cinza escuro
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n-----------------------------------------\n");
                    Console.ResetColor();
                    //_fileOperations.MoveFileToError(filePath, _constants.PathProcessed);
                    return;
                }


                if (resultPGM != log.PgmVersion.ToUpper())
                {
                    // Título em vermelho para indicar falha
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n************ PGM CONFIGURADO INCORRETAMENTE ************\n");
                    Console.ResetColor();

                    // Informações do Log em caso de falha
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Versão Script Local: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{log.PgmVersion}");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Versão Script Configurada no Mes: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{resultPGM}");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Serial: {log.Serial}");
                    Console.ResetColor();

                    // Separador em cinza escuro
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n-----------------------------------------\n");
                    Console.ResetColor();

                    //_fileOperations.MoveFileToError(filePath, _constants.PathProcessed);

                    return;
                }

                if (log.TestResult == "P")
                {
                    var resultMesApi = _jabilApi.SendTestMes(mesApiRequest);

                    if (!isRetest && _constants.WillGetMac == "True")
                    {
                        var resultMacApi = _macApi.InsertMacLog(macInsertRequest);


                        // Título em verde
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n************ MAC REGISTRADO NA BASE DE DADOS /LOG REPORTADO PARA O MES ************\n");
                        Console.ResetColor();

                        // Informações do Log
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Serial: {log.Serial}");
                        Console.WriteLine($"Mac: {log.Mac}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Versão Script Local: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{log.PgmVersion}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Versão Script Configurada no Mes: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{resultPGM}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Resultado Placa: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{log.TestResult}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Resultado API Mes: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("OK");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Resultado API Mac: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("OK");
                        Console.ResetColor();

                        // Separador em cinza escuro
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("\n-----------------------------------------\n");
                        Console.ResetColor();
                        //_fileOperations.MoveFileToSccess(filePath, _constants.PathProcessed);
                        return;

                    }
                    else
                    {

                        // Título em verde
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n************LOG REPORTADO PARA O MES ************\n");
                        Console.ResetColor();

                        // Informações do Log
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Serial: {log.Serial}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Versão Script Local: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{log.PgmVersion}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Versão Script Configurada no Mes: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{resultPGM}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Resultado Placa: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{log.TestResult}");
                        Console.ResetColor();

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"Resultado API Mes: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("OK");
                        Console.ResetColor();

                        // Separador em cinza escuro
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("\n-----------------------------------------\n");
                        Console.ResetColor();
                        //_fileOperations.MoveFileToSccess(filePath, _constants.PathProcessed);
                        return;

                    }
                }

                else
                {
                    var resultMesApi = _jabilApi.SendTestMes(mesApiRequest);
                    //var resultMesApi = "SUCESS";



                    // Título em vermelho para indicar falha
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n************ PLACA NÃO APROVADA ************\n");
                    Console.ResetColor();

                    // Informações do Log em caso de falha
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Serial: {log.Serial}");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Resultado: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{log.TestResult}");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Resultado API Mes: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{resultMesApi}");
                    Console.ResetColor();

                    // Separador em cinza escuro
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n-----------------------------------------\n");
                    Console.ResetColor();
                    //_fileOperations.MoveFileToSccess(filePath, _constants.PathProcessed);
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.ResetColor();
                //_fileOperations.MoveFileToError(filePath, _constants.PathError);
                return;

            }
        }
    }
}
