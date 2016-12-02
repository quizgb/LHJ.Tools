//------------------------------------------------------------------------------
// <copyright file="CopyRefMenu.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VSLangProj;

namespace LHJ.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CopyRefMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a25befdb-b45e-448c-831b-3945825a979c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRefMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CopyRefMenu(Package package)
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
        public static CopyRefMenu Instance
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
            Instance = new CopyRefMenu(package);
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
            string[] selectedRefs = Global.Dte.SelectedItems.Cast<SelectedItem>().Select<SelectedItem, string>((Func<SelectedItem, string>)(x => x.Name)).ToArray<string>();
            Project selectedProject = Global.Dte.GetSelectedProject();

            if (selectedProject == null)
            {
                return;
            }

            try
            {
                Clipboard.SetText(string.Join("\n", selectedProject.References().Where<Reference>((Func<Reference, bool>)(x => x.Name.IsIn<string>((IEnumerable<string>)selectedRefs))).Select<Reference, string>((Func<Reference, string>)(x =>
                {
                    string description = x.Description;

                    if (string.IsNullOrEmpty(description))
                    {
                        return "copyref:proj:" + x.ContainingProject.FileName;
                    }

                    if (Path.IsPathRooted(description))
                    {
                        return "copyref:file:" + x.Path;
                    }

                    return "copyref:gac:" + Path.GetFileNameWithoutExtension(description);
                })).ToArray<string>()));
            }
            catch(Exception ee)
            {
            }
        }
    }
}
