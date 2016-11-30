//------------------------------------------------------------------------------
// <copyright file="devCmdMenu.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LHJ.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class devCmdMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f4a4ef3b-78d7-432a-8fff-999d3ab319e7");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="devCmdMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private devCmdMenu(Package package)
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
        public static devCmdMenu Instance
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
            Instance = new devCmdMenu(package);
        }

        private DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as DTE2;
        }

        private void StartProcess(string workingDirectory, string command, string arguments, string cmdString = "")
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(command, arguments + cmdString);
            processStartInfo.WorkingDirectory = workingDirectory;
            this.ModifyPathVariable(processStartInfo);
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private void ModifyPathVariable(ProcessStartInfo start)
        {
            string str = ".\\node_modules\\.bin;" + start.EnvironmentVariables["PATH"];
            string environmentVariable = Environment.GetEnvironmentVariable("VS140COMNTOOLS");

            if (Directory.Exists(environmentVariable))
            {
                string fullName = Directory.GetParent(environmentVariable).Parent.FullName;
                str = str + ";" + Path.Combine(fullName, "IDE\\Extensions\\Microsoft\\Web Tools\\External");
            }

            start.UseShellExecute = false;
            start.EnvironmentVariables["PATH"] = str;
        }

        public string GetInstallDirectory(IServiceProvider serviceProvider)
        {
            string str = (string)null;
            IVsShell service = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
            if (service != null)
            {
                object pvar = (object)null;
                service.GetProperty(-9007, out pvar);
                if (pvar != null)
                    str = pvar as string;
            }
            return str;
        }

        public string GetFolderPath(DTE2 dte)
        {
            DTE2 dtE2 = dte;
            UIHierarchyItem[] selectedItems = dtE2.ToolWindows.SolutionExplorer.SelectedItems as UIHierarchyItem[];
            if (selectedItems == null || selectedItems.Length <= 0)
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string path = string.Empty;
            UIHierarchyItem uiHierarchyItem = selectedItems[0];
            if (uiHierarchyItem != null)
            {
                if (uiHierarchyItem.Object is Project)
                    path = (uiHierarchyItem.Object as Project).Properties.Item((object)"FullPath").Value.ToString();
                else if (uiHierarchyItem.Object is ProjectItem)
                    path = (uiHierarchyItem.Object as ProjectItem).Properties.Item((object)"FullPath").Value.ToString();
                else if (uiHierarchyItem.Object is Solution)
                {
                    object obj = uiHierarchyItem.Object;
                    path = dtE2.DTE.Solution.FullName;
                }
            }
            return Path.GetDirectoryName(path);
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
            this.StartProcess(this.GetFolderPath(this.GetDTE2()), "cmd.exe", "/k \"" + Path.Combine(this.GetInstallDirectory((IServiceProvider)package), "..\\Tools\\VsDevCmd.bat") + "\"", "");
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "devCmdMenu";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
