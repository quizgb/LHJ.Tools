//------------------------------------------------------------------------------
// <copyright file="PasteRefMenu.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace LHJ.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PasteRefMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0ee4e64b-a559-4da8-9aa9-9156352cc3ef");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasteRefMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private PasteRefMenu(Package package)
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
        public static PasteRefMenu Instance
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
            Instance = new PasteRefMenu(package);
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
            try
            {
                string text = Clipboard.GetText();

                if (!text.StartsWithAny("copyref:file:", "copyref:gac:", "copyref:proj:"))
                {
                    return;
                }

                Project selectedProject = Global.Dte.GetSelectedProject();

                if (selectedProject == null)
                {
                    return;
                }

                try
                {
                    string str1 = text;
                    char[] chArray = new char[1] { '\n' };

                    foreach (string str2 in str1.Split(chArray))
                    {
                        try
                        {
                            if (str2.StartsWith("copyref:file:"))
                            {
                                string reference = str2.Substring("copyref:file:".Length).Trim();
                                selectedProject.AddReference(reference);
                            }
                            else if (str2.StartsWith("copyref:gac:"))
                            {
                                string reference = str2.Substring("copyref:gac:".Length).Trim();
                                selectedProject.AddReference(reference);
                            }
                            else if (str2.StartsWith("copyref:proj:"))
                            {
                                Project projectByFile = Global.Dte.GetProjectByFile(str2.Substring("copyref:proj:".Length).Trim());

                                try
                                {
                                    selectedProject.AddReference(projectByFile, true);
                                }
                                catch (Exception ex)
                                {
                                    VsShellUtilities.ShowMessageBox(this.ServiceProvider, ex.Message, (string)null, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
        }
    }
}
