using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VSLangProj;

internal static class Global
{
    public static DTE dte;

    public static DTE Dte
    {
        get
        {
            return Global.dte = Global.dte ?? Package.GetGlobalService(typeof(DTE)) as DTE;
        }
    }

    public static Project GetSelectedProject(this DTE dte)
    {
        Project project = (Project)null;
        try
        {
            IVsProject selectedVsProject = Global.GetSelectedVsProject();
            if (selectedVsProject != null)
                project = dte.GetProjectByFile(selectedVsProject.FileName());
        }
        catch
        {
        }
        return project;
    }

    public static IVsProject GetSelectedVsProject()
    {
        uint pitemid = uint.MaxValue;
        IVsMonitorSelection globalService = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
        if (globalService == null)
            return (IVsProject)null;
        IVsMultiItemSelect ppMIS = (IVsMultiItemSelect)null;
        IntPtr ppHier = IntPtr.Zero;
        IntPtr ppSC = IntPtr.Zero;
        try
        {
            if (ErrorHandler.Failed(globalService.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC)) || ppHier == IntPtr.Zero || (int)pitemid == -1)
                return (IVsProject)null;
            return Marshal.GetObjectForIUnknown(ppHier) as IVsProject;
        }
        finally
        {
            if (ppSC != IntPtr.Zero)
                Marshal.Release(ppSC);
            if (ppHier != IntPtr.Zero)
                Marshal.Release(ppHier);
        }
    }

    public static bool StartsWithAny(this string text, params string[] patterns)
    {
        return ((IEnumerable<string>)patterns).Any<string>((Func<string, bool>)(x => text.StartsWith(x)));
    }

    public static Project GetProjectByFile(this DTE dte, string projectFile)
    {
        try
        {
            foreach (Project project in dte.Solution.Projects)
            {
                if (project.FileName == projectFile)
                    return project;
            }
        }
        catch
        {
        }
        return (Project)null;
    }

    public static Project AddReference(this Project proj, string reference)
    {
        try
        {
            // ISSUE: reference to a compiler-generated method
            (proj.Object as VSProject).References.Add(reference);
        }
        catch
        {
        }
        return (Project)null;
    }

    public static Project AddReference(this Project proj, Project reference, bool throwOnError = false)
    {
        try
        {
            // ISSUE: reference to a compiler-generated method
            (proj.Object as VSProject).References.AddProject(reference);
        }
        catch
        {
            if (throwOnError)
                throw;
        }
        return (Project)null;
    }

    public static IEnumerable<Reference> References(this Project proj)
    {
        return (proj.Object as VSProject).References.Cast<Reference>();
    }

    public static bool IsIn<T>(this T obj, IEnumerable<T> collection)
    {
        return collection.Contains<T>(obj);
    }

    public static Project FindByFileName(this IEnumerable<Project> projects, string file)
    {
        return projects.FirstOrDefault<Project>((Func<Project, bool>)(x => x.FileName == file));
    }

    public static string FileName(this IVsProject project)
    {
        string pbstrMkDocument = (string)null;
        project.GetMkDocument(4294967294U, out pbstrMkDocument);
        return pbstrMkDocument;
    }
}