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
        private readonly Jems4Api _jems4Api;
        private static readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();
        private static readonly AutoResetEvent _fileEvent = new AutoResetEvent(false);

        public Program(Constants constants, FileOperations fileOperations, MacAPI macApi, JabilApi jabilApi, Jems4Api jems4Api)
        {
            _constants = constants;
            _fileOperations = fileOperations;
            _macApi = macApi;
            _jabilApi = jabilApi;
            _jems4Api = jems4Api;
        }

        public static void Main(string[] args)
        {
            HttpClient httpClient = new HttpClient();
            Constants constants = new Constants();
            FileOperations fileOperations = new FileOperations();

            MacAPI macAPI = new MacAPI(httpClient);
            JabilApi jabilApi = new JabilApi(httpClient);
            Jems4Api jesm4Api = new Jems4Api(constants);

            Program program = new Program(constants, fileOperations, macAPI, jabilApi, jesm4Api);

            MonitorLatestFile(constants, jesm4Api, program);

        }

        private static void MonitorLatestFile(Constants _constants, Jems4Api _jems4Api, Program program)
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
                        program.Run(_constants, latestFile, _jems4Api);
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

        private async void Run(Constants _constants, string pathWatch, Jems4Api _jems4Api)
        {
            Console.WriteLine($"Iniciando o processamento do arquivo: {pathWatch}");

            OkToStartResponse okToStart = new OkToStartResponse();

            string userServiceAccount = _constants.UserName;
            string PasswordServiceAccount = _constants.Password;


            if (_constants.LegacyMes == "False")
            {
                await _jems4Api.AdSignin(userServiceAccount, PasswordServiceAccount);
            }

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

                    //if (string.IsNullOrEmpty(log.Mac))
                    //{
                    //    Console.ForegroundColor = ConsoleColor.Yellow;
                    //    Console.WriteLine("Mac não informado");
                    //    Console.WriteLine($"Serial: {log.Serial}");
                    //    Console.ResetColor();
                    //}

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
                    if (_constants.LegacyMes == "False")
                    {
                        Console.WriteLine("Chamando o Jabil API, parameters sent");
                        Console.WriteLine($"Assembly: {_constants.Assembly}, Serial: {log.Serial}");

                        List<GetWipInformationBySerialNumberResponse> wipInformation = await _jems4Api.GetWipInformationBySerialNumber("Manaus", "SAMSUNG", _constants.Assembly, log.Serial);
                        okToStart = await _jems4Api.OkToStart(wipInformation.FirstOrDefault().WipId, _constants.Resource);


                        if (okToStart.OkToStart == true)
                        {
                            StartWipRequest startWipRequest = new StartWipRequest();
                            StartWipResponse startWipResponse = new StartWipResponse();
                            CompleteWipRequest completeWipRequest = new CompleteWipRequest();
                            CompleteWipResponse completeWipResponse = new CompleteWipResponse();

                            startWipRequest.SerialNumber = log.Serial;
                            startWipRequest.WipId = wipInformation.FirstOrDefault().WipId;
                            startWipRequest.StartDateTime = DateTime.UtcNow;
                            startWipRequest.ShouldSkipValidation = true;
                            startWipRequest.IsSingleWipMode = true;

                            completeWipRequest.WipId = wipInformation.FirstOrDefault().WipId;
                            completeWipRequest.EndDateTime = DateTime.UtcNow;
                            completeWipRequest.IsSingleWipMode = true;

                            startWipResponse = await _jems4Api.StartWip(startWipRequest);
                            completeWipResponse = await _jems4Api.CompleteWip(completeWipRequest);
                        }
                    }



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
