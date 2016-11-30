//------------------------------------------------------------------------------
// <copyright file="OpenContainingFolder.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
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
    internal sealed class OpenContainingFolder
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("430d0ca9-4380-4932-b829-b2752996f62a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenContainingFolder"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenContainingFolder(Package package)
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
        public static OpenContainingFolder Instance
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
            Instance = new OpenContainingFolder(package);
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
            DTE2 service = this.ServiceProvider.GetService(typeof(SDTE)) as DTE2;
            Solution solution = service.Solution;

            if (service.SelectedItems.Count <= 0)
            {
                return;
            }

            Folders folders = new Folders();

            foreach (SelectedItem selectedItem in service.SelectedItems)
            {
                if (solution != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "\"" + new FileInfo(solution.Properties.Item((object)"PATH").Value.ToString()).Directory.FullName + "\""); 
                }
         
                if (selectedItem.Project != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "\"" + folders.GetProjectPath(selectedItem.Project) + "\"");
                }

                if (selectedItem.ProjectItem != null)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "\"" + folders.GetProjectItemPath(selectedItem.ProjectItem) + "\"");
                }
            }
        }
    }
}
