using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Win32;

namespace Tester
{
    class Program
    {
        private static string _token = "";
        private static readonly string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BeautySharp.ini";

        private const string TokenSuffix = "{TOKEN}";

        /* URL BASES */
        private const string UrlPaste = "http://www.ioncodes.com/BeautySharp/create.php?token={TOKEN}";
        private const string UrlCreateToken = "http://www.ioncodes.com/BeautySharp/createtoken.php";

        static void Main(string[] args)
        {
            File.Delete(Path);
            if (FileValidation())
            {
                _token = File.ReadAllText(Path);
            }
            else
            {
                _token = CreateToken();
                File.WriteAllText(Path, _token);
            }

            Console.Read();
        }

        private static string CreateToken()
        {
            const string location = @"SOFTWARE\Microsoft\Cryptography";
            const string name = "MachineGuid";
            string guid;

            using (RegistryKey localMachineX64View =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
                {
                    if (rk == null)
                        throw new KeyNotFoundException(
                            $"Key Not Found: {location}");

                    object machineGuid = rk.GetValue(name);
                    if (machineGuid == null)
                        throw new IndexOutOfRangeException(
                            $"Index Not Found: {name}");

                    guid = machineGuid.ToString();
                }
            }

            return RegisterToken(guid);
        }

        private static bool FileValidation()
        {
            return File.Exists(Path); // improved later
        }

        private static string RegisterToken(string tempToken)
        {
            var request = (HttpWebRequest)WebRequest.Create(UrlCreateToken);
            request.Method = "POST";
            request.ContentType = "multipart/form-data";
            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(string.Empty);
            outgoingQueryString.Add("id", tempToken);
            string postdata = outgoingQueryString.ToString();
            byte[] data = Encoding.ASCII.GetBytes(postdata);
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            Console.WriteLine(responseString);
            switch (responseString)
            {
                case "Wrong request.":
                    throw new Exception("You found a bug! Feed me senpai!");
                case "DB Error.":
                    Console.WriteLine("Server not working currently.");
                    return "";
            }
            Console.WriteLine(responseString);
            return responseString; // return token
        }
    }
}
