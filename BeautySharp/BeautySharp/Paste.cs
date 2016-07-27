//------------------------------------------------------------------------------
// <copyright file="Paste.cs" company="ionix">
//     Copyright (c) ionix.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Win32;

namespace BeautySharp
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Paste
    {
        private static string _token = "";
        private static readonly string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/BeautySharp.ini";

        private const string TokenSuffix = "{TOKEN}";

        /* URL BASES */
        private const string UrlPaste = "http://www.ioncodes.com/BeautySharp/create.php?token={TOKEN}";
        private const string UrlCreateToken = "http://www.ioncodes.com/BeautySharp/createtoken.php";

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("807af95f-fdbe-4ee1-acbe-713a905d840b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Paste"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private Paste(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Paste Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
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

            Instance = new Paste(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Let's do the work!

            if (_token != "")
            {
                //METHOD: CREATE PASTE
                var request = (HttpWebRequest)WebRequest.Create(UrlPaste.Replace(TokenSuffix, _token));
                request.Method = "POST";
                request.ContentType = "multipart/form-data";
                NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(string.Empty);
                outgoingQueryString.Add("source", "Console.WriteLine(\"Hello World.\");");
                string postdata = outgoingQueryString.ToString();
                byte[] data = Encoding.ASCII.GetBytes(postdata);
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                MessageBox.Show(responseString);
            }
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
            MessageBox.Show(responseString);
            switch (responseString)
            {
                case "Wrong request.":
                    throw new Exception("You found a bug! Feed me senpai!");
                case "DB Error.":
                    MessageBox.Show("Server not working currently.");
                    return "";
            }
            MessageBox.Show(responseString);
            return responseString; // return token
        }
    }
}
