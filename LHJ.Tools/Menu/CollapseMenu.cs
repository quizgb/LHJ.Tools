//------------------------------------------------------------------------------
// <copyright file="CollapseMenu.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LHJ.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CollapseMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e8bb2998-d8b2-4da4-aa6e-b8cf41f33cfc");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollapseMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CollapseMenu(Package package)
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
                OleMenuCommand menuItem = new OleMenuCommand(this.CollapseSelectedNodes, menuCommandID);
                commandService.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += new EventHandler(this.BeforeQueryStatus);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CollapseMenu Instance
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
            Instance = new CollapseMenu(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)(this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService).FindCommand(new CommandID(CommandSet, 256));
            command.Visible = false;
            command.Enabled = false;
            IVsUIHierarchyWindow explorerToolWindow = this.GetSolutionExplorerToolWindow();
            if (explorerToolWindow == null)
                return;
            command.Visible = !this.IsSolutionExplorerEmpty();
            if (!command.Visible || !this.ContainsAtleastOneCollapsibleNode(this.GetSelectedNodes(explorerToolWindow)))
                return;
            command.Enabled = true;
        }

        private void CollapseSelectedNodes(object sender, EventArgs e)
        {
            this.Collapse();
        }

        private void Collapse()
        {
            IVsUIHierarchyWindow explorerToolWindow = this.GetSolutionExplorerToolWindow();
            if (explorerToolWindow == null)
                return;
            VSITEMSELECTION[] selectedNodes = this.GetSelectedNodes(explorerToolWindow);
            bool flag1 = this.ContainsSolutionNode(selectedNodes);
            bool flag2 = false;
            VSITEMSELECTION loneProjectWithoutSolution = new VSITEMSELECTION()
            {
                itemid = 4294967294,
                pHier = (IVsHierarchy)null
            };
            if (!flag1)
                flag2 = this.ContainsLoneProjectWithoutSolution(selectedNodes, out loneProjectWithoutSolution);
            if (flag1)
            {
                IVsHierarchy service = (IVsSolution)this.ServiceProvider.GetService(typeof(SVsSolution)) as IVsHierarchy;
                if (service == null)
                    return;
                this.CollapseHierarchyItems(explorerToolWindow, service, 4294967294U, true);
            }
            else if (flag2)
            {
                if (loneProjectWithoutSolution.pHier == null || (int)loneProjectWithoutSolution.itemid == -1)
                    return;
                this.CollapseHierarchyItems(explorerToolWindow, loneProjectWithoutSolution.pHier, loneProjectWithoutSolution.itemid, true);
            }
            else
            {
                foreach (VSITEMSELECTION vsitemselection in selectedNodes)
                    this.CollapseHierarchyItems(explorerToolWindow, vsitemselection.pHier, vsitemselection.itemid, false);
            }
        }

        private bool ContainsLoneProjectWithoutSolution(VSITEMSELECTION[] selectedItems, out VSITEMSELECTION loneProjectWithoutSolution)
        {
            ErrorUtilities.VerifyThrowArgumentNull((object)selectedItems, "selectedItems");
            bool flag = false;
            loneProjectWithoutSolution = new VSITEMSELECTION()
            {
                itemid = uint.MaxValue,
                pHier = (IVsHierarchy)null
            };
            foreach (VSITEMSELECTION selectedItem in selectedItems)
            {
                if (this.IsLoneProjectWithoutSolution(selectedItem))
                {
                    flag = true;
                    loneProjectWithoutSolution = selectedItem;
                    break;
                }
            }
            return flag;
        }

        private bool IsLoneProjectWithoutSolution(VSITEMSELECTION selectedItem)
        {
            bool flag = false;
            if ((int)selectedItem.itemid == -2 && selectedItem.pHier != null)
            {
                IVsHierarchy service = (IVsSolution)this.ServiceProvider.GetService(typeof(SVsSolution)) as IVsHierarchy;
                object pvar;
                if (service != null && this.GetNumberOfProjectUnderTheSolution() == 1 && (selectedItem.pHier.GetProperty(selectedItem.itemid, -2032, out pvar) == 0 && object.ReferenceEquals(pvar, (object)service)))
                    flag = this.IsSolutionNodeHidden();
            }
            return flag;
        }

        private bool IsSolutionNodeHidden()
        {
            bool flag = true;
            object pvar;
            if (((IVsSolution)this.ServiceProvider.GetService(typeof(SVsSolution))).GetProperty(-8017, out pvar) == 0 && pvar is bool)
                flag = (bool)pvar;
            return flag;
        }

        private void CollapseHierarchyItems(IVsUIHierarchyWindow toolWindow, IVsHierarchy hierarchy, uint itemid, bool hierIsSolution)
        {
            ErrorUtilities.VerifyThrowArgumentNull((object)toolWindow, "toolWindow");
            ErrorUtilities.VerifyThrowArgumentNull((object)hierarchy, "hierarchy");
            Guid guid = typeof(IVsHierarchy).GUID;
            IntPtr ppHierarchyNested;
            uint pitemidNested;
            if (hierarchy.GetNestedHierarchy(itemid, ref guid, out ppHierarchyNested, out pitemidNested) == 0 && IntPtr.Zero != ppHierarchyNested)
            {
                IVsHierarchy objectForIunknown = Marshal.GetObjectForIUnknown(ppHierarchyNested) as IVsHierarchy;
                Marshal.Release(ppHierarchyNested);
                if (objectForIunknown == null)
                    return;
                this.CollapseHierarchyItems(toolWindow, objectForIunknown, pitemidNested, false);
            }
            else
            {
                if (!hierIsSolution)
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(toolWindow.ExpandItem(hierarchy as IVsUIHierarchy, itemid, EXPANDFLAGS.EXPF_CollapseFolder));
                object pvar;
                if (hierarchy.GetProperty(itemid, -2041, out pvar) != 0)
                    return;
                for (uint itemId = this.GetItemId(pvar); (int)itemId != -1; itemId = this.GetItemId(pvar))
                {
                    this.CollapseHierarchyItems(toolWindow, hierarchy, itemId, false);
                    int property = hierarchy.GetProperty(itemId, -2042, out pvar);
                    if (property != 0)
                    {
                        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(property);
                        break;
                    }
                }
            }
        }

        private bool ContainsSolutionNode(VSITEMSELECTION[] selectedNodes)
        {
            ErrorUtilities.VerifyThrowArgumentNull((object)selectedNodes, "selectedNodes");
            bool flag = false;
            if ((IVsSolution)this.ServiceProvider.GetService(typeof(SVsSolution)) is IVsHierarchy)
            {
                foreach (VSITEMSELECTION selectedNode in selectedNodes)
                {
                    if (this.IsSolutionNode(selectedNode))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        private bool IsSolutionNode(VSITEMSELECTION selectedNode)
        {
            IVsHierarchy service = (IVsSolution)this.ServiceProvider.GetService(typeof(SVsSolution)) as IVsHierarchy;
            return selectedNode.pHier == null ? (int)selectedNode.itemid == -2 : object.ReferenceEquals((object)selectedNode.pHier, (object)service);
        }

        private VSITEMSELECTION[] GetSelectedNodes(IVsUIHierarchyWindow toolWindow)
        {
            ErrorUtilities.VerifyThrowArgumentNull((object)toolWindow, "toolWindow");
            uint pitemid = uint.MaxValue;
            IntPtr ppHier = IntPtr.Zero;
            VSITEMSELECTION[] vsitemselectionArray = (VSITEMSELECTION[])null;
            try
            {
                IVsMultiItemSelect ppMIS;
                int currentSelection = toolWindow.GetCurrentSelection(out ppHier, out pitemid, out ppMIS);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(currentSelection);
                if (currentSelection == 0)
                {
                    if ((int)pitemid == -3 && ppMIS != null)
                    {
                        uint pcItems;
                        int pfSingleHierarchy;
                        int selectionInfo = ppMIS.GetSelectionInfo(out pcItems, out pfSingleHierarchy);
                        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(selectionInfo);
                        if (pcItems > 0U)
                        {
                            if (selectionInfo == 0)
                            {
                                VSITEMSELECTION[] rgItemSel = new VSITEMSELECTION[pcItems];
                                for (int index = 0; (long)index < (long)pcItems; ++index)
                                {
                                    rgItemSel[index].itemid = uint.MaxValue;
                                    rgItemSel[index].pHier = (IVsHierarchy)null;
                                }
                                int selectedItems = ppMIS.GetSelectedItems(0U, pcItems, rgItemSel);
                                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(selectedItems);
                                if (selectedItems == 0)
                                    vsitemselectionArray = rgItemSel;
                            }
                        }
                    }
                    else if ((int)pitemid != -3)
                    {
                        if (ppHier != IntPtr.Zero)
                        {
                            IVsHierarchy objectForIunknown = Marshal.GetObjectForIUnknown(ppHier) as IVsHierarchy;
                            vsitemselectionArray = new VSITEMSELECTION[1]
                            {
                new VSITEMSELECTION()
                {
                  itemid = pitemid,
                  pHier = objectForIunknown
                }
                            };
                        }
                    }
                }
            }
            finally
            {
                if (ppHier != IntPtr.Zero)
                    Marshal.Release(ppHier);
            }
            return vsitemselectionArray ?? new VSITEMSELECTION[0];
        }

        private bool ContainsAtleastOneCollapsibleNode(VSITEMSELECTION[] selectedItems)
        {
            ErrorUtilities.VerifyThrowArgumentNull((object)selectedItems, "selectedItems");
            bool flag = false;
            foreach (VSITEMSELECTION selectedItem in selectedItems)
            {
                if (this.IsCollapsible(selectedItem))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        private bool IsCollapsible(VSITEMSELECTION selectedItem)
        {
            bool flag = false;
            if (this.IsSolutionNode(selectedItem))
                flag = true;
            else if (selectedItem.pHier != null)
            {
                object pvar;
                if (selectedItem.pHier.GetProperty(selectedItem.itemid, -1001, out pvar) == 0)
                    flag = (int)this.GetItemId(pvar) != -1;
                if (!flag && selectedItem.pHier.GetProperty(selectedItem.itemid, -2041, out pvar) == 0)
                    flag = (int)this.GetItemId(pvar) != -1;
            }
            return flag;
        }

        private IVsUIHierarchyWindow GetSolutionExplorerToolWindow()
        {
            IVsUIHierarchyWindow uiHierarchyWindow = (IVsUIHierarchyWindow)null;
            IVsUIShell service = this.ServiceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (service != null)
            {
                uint grfFTW = 0;
                Guid rguidPersistenceSlot = new Guid("{3AE79031-E1BC-11D0-8F78-00A0C9110057}");
                IVsWindowFrame ppWindowFrame = (IVsWindowFrame)null;
                service.FindToolWindow(grfFTW, ref rguidPersistenceSlot, out ppWindowFrame);
                if (ppWindowFrame != null)
                {
                    object pvar = (object)null;
                    ppWindowFrame.GetProperty(-3001, out pvar);
                    if (pvar != null)
                        uiHierarchyWindow = pvar as IVsUIHierarchyWindow;
                }
            }
            return uiHierarchyWindow;
        }

        private bool IsSolutionExplorerEmpty()
        {
            bool flag = true;
            IVsUIHierarchyWindow explorerToolWindow = this.GetSolutionExplorerToolWindow();
            if (explorerToolWindow != null)
            {
                uint pitemid = uint.MaxValue;
                IVsMultiItemSelect ppMIS = (IVsMultiItemSelect)null;
                IntPtr ppHier = IntPtr.Zero;
                try
                {
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(explorerToolWindow.GetCurrentSelection(out ppHier, out pitemid, out ppMIS));
                    flag = (int)pitemid != -3 && !(ppHier != IntPtr.Zero);
                }
                finally
                {
                    if (ppHier != IntPtr.Zero)
                        Marshal.Release(ppHier);
                }
                if (flag)
                    flag = this.GetNumberOfProjectUnderTheSolution() <= 0;
            }
            return flag;
        }

        private int GetNumberOfProjectUnderTheSolution()
        {
            int num = -1;
            IVsHierarchy service = (IVsSolution)this.ServiceProvider.GetService(typeof(SVsSolution)) as IVsHierarchy;
            if (service != null)
            {
                object pvar;
                service.GetProperty(4294967294U, -2027, out pvar);
                num = (pvar as Solution).Count;
            }
            return num;
        }

        private bool ConvertToBool(object pvar, bool defaultValue)
        {
            bool flag = defaultValue;
            if (pvar is bool)
                flag = (bool)pvar;
            else if (pvar is int)
                flag = (int)pvar == 1;
            return flag;
        }

        private uint GetItemId(object pvar)
        {
            if (pvar == null)
                return uint.MaxValue;
            if (pvar is int)
                return (uint)(int)pvar;
            if (pvar is uint)
                return (uint)pvar;
            if (pvar is short)
                return (uint)(short)pvar;
            if (pvar is ushort)
                return (uint)(ushort)pvar;
            if (pvar is long)
                return (uint)(long)pvar;
            return uint.MaxValue;
        }
    }
}
