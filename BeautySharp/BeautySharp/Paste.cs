//------------------------------------------------------------------------------
// <copyright file="Paste.cs" company="ionix">
//     Copyright (c) ionix.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using EnvDTE;
using Microsoft.Win32;

namespace BeautySharp
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Paste
    {
        public DTE _dte;
        
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

            OleMenuCommandService commandService =
                this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            _dte = (DTE)ServiceProvider.GetService(typeof(DTE));

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
        public static Paste Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
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

            if (Variables._token == "") return;
            // METHOD: CREATE PASTE
            if (_dte.ActiveDocument != null)
            {
                string source = Functions.GetDocumentText(_dte.ActiveDocument);
                if (source != "")
                {
                    string postData = "source=" + Uri.EscapeDataString(source);
                    Clipboard.SetText(Functions.WebPost(Variables.UrlPaste.Replace(Variables.TokenSuffix, Variables._token), postData));

                    Functions.Notify("Your paste has been published! Link is now in your clipboard!", CommandSet); // Toast
                }
                else
                {
                    MessageBox.Show("Source cannot be empty!");
                }
            }
            else
            {
                MessageBox.Show("No valid file opened!");
            }
        }
    }
}
