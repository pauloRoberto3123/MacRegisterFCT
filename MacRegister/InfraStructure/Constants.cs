using MacRegister.Service;
using System.Configuration;
using System.IO;

namespace MacRegister.InfraStructure
{
    public class Constants
    {
        FileOperations _fileOperations = new FileOperations();
        public string TokenApi;

        public static string BaseUrlMac
        {
            get
            {
                return ConfigurationManager.AppSettings["BaseUrlMac"];
            }
        }
        public static string BaseUrlJems4
        {
            get
            {
                return ConfigurationManager.AppSettings["BaseUrlJems4"];
            }
        }
        public string LegacyMes
        {
            get
            {
                return ConfigurationManager.AppSettings["LegacyMes"];
            }
        }
        public string Resource
        {
            get
            {
                return ConfigurationManager.AppSettings["Resource"];
            }
        }

        public string Assembly
        {
            get
            {
                return ConfigurationManager.AppSettings["Assembly"];
            }
        }

        public static string BaseUrlMes
        {
            get
            {
                return ConfigurationManager.AppSettings["BaseUrlMes"];
            }
        }

        public string PathLog
        {

            get
            {
                return ConfigurationManager.AppSettings["BasePathLog"];
            }
        }
        public string Equipment
        {

            get
            {
                return ConfigurationManager.AppSettings["Equipment"];
            }
        }
        public string Step
        {

            get
            {
                return ConfigurationManager.AppSettings["Step"];
            }
        }

        public string PathProcessed
        {

            get
            {
                string basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string pathProcessed = basePath + @"\temp\Success";
                _fileOperations.CreateDirectory(pathProcessed);
                return pathProcessed;

            }
        }

        public string PathError
        {
            get
            {
                string basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string pathProcessed = basePath + @"\temp\Error";
                _fileOperations.CreateDirectory(pathProcessed);
                return pathProcessed;

            }
        }

    }
}
