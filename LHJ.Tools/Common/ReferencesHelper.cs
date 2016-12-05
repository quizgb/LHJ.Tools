using System.Linq;
using VSLangProj;

namespace LHJ.Tools.Common
{
    public class ReferencesHelper
    {
        /// <summary>
        /// Set Copy Local flag.
        /// </summary>
        public static int SetCopyLocalFlag(EnvDTE.Project project, bool copyLocal = false, bool previewOnly = false)
        {
            var vsProject = project.Object as VSProject;
            int count = 0;

            if (vsProject != null)
            {
                var references = vsProject.References.Cast<Reference>().Where(reference => reference.CopyLocal != copyLocal).ToList();

                if (references.Any())
                    Comm.WriteToDTEOutput(project.DTE, string.Format("Project '{0}':", project.Name));

                foreach (var reference in references.Where(r => !r.IsCore()))
                {
                    count++;
                    if (!previewOnly)
                        reference.CopyLocal = copyLocal;

                    Comm.WriteToDTEOutput(project.DTE, string.Format("\t'{0}': Copy Local: {1} -> {2}", reference.Name, !copyLocal, copyLocal));
                }
            }

            return count;
        }
    }

    public static class ReferencesHelperExtensions
    {
        public static bool IsCore(this Reference reference)
        {
            return reference.Name.Equals("mscorlib");
        }
    }
}
