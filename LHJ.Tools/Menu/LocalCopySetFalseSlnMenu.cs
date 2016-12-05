//------------------------------------------------------------------------------
// <copyright file="LocalCopySetFalseSlnMenu.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using LHJ.Tools.Common;

namespace LHJ.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LocalCopySetFalseSlnMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d0aa557a-b8c5-4289-8fe9-12b79f45d788");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCopySetFalseSlnMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private LocalCopySetFalseSlnMenu(Package package)
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
        public static LocalCopySetFalseSlnMenu Instance
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
            Instance = new LocalCopySetFalseSlnMenu(package);
        }

        private void SetCopyLocalForSolution()
        {
            var projects = SolutionHelper.GetProjects(Global.Dte);
            int changeCount = 0;

            foreach (var project in projects)
            {
                changeCount += ReferencesHelper.SetCopyLocalFlag(project);
                SaveProjectIfNeeded(project);
            }

            LogChangesToOutput(changeCount);
        }

        private void LogChangesToOutput(int changeCount)
        {
            var msg = changeCount > 0
                          ? string.Format(CultureInfo.CurrentCulture,
                                          "Copy Local set to {0} in {1} references{2}.", false, changeCount,
                                          false ? " (Preview)" : string.Empty)
                          : "No Copy Local references found to set.";

            Comm.WriteToDTEOutput(Global.Dte, msg);
        }

        private void SaveProjectIfNeeded(Project project)
        {
            if (!project.IsDirty) return;

            project.Save();
            Comm.WriteToDTEOutput(Global.Dte, string.Format("'{0}' saved.", project.Name));
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
            this.SetCopyLocalForSolution();
        }
    }
}
