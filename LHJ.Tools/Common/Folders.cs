using EnvDTE;
using System.IO;

namespace LHJ.Tools
{
    internal sealed class Folders
    {
        public string GetProjectPath(Project proj)
        {
            if (this.HasProperty(proj.Properties, "FullPath"))
                return new FileInfo(proj.Properties.Item((object)"FullPath").Value.ToString()).Directory.FullName;
            if (this.HasProperty(proj.Properties, "ProjectFile"))
                return new FileInfo(proj.Properties.Item((object)"ProjectFile").Value.ToString()).Directory.FullName;
            return string.Empty;
        }

        public string GetProjectItemPath(ProjectItem item)
        {
            if (!this.HasProperty(item.Properties, "FullPath"))
                return string.Empty;
            Property property = item.Properties.Item((object)"FullPath");
            if (property == null)
                return string.Empty;
            return new FileInfo(property.Value.ToString()).Directory.FullName;
        }

        public string GetOutputPath(Project proj)
        {
            string projectPath = this.GetProjectPath(proj);
            if (string.IsNullOrWhiteSpace(projectPath) || proj.ConfigurationManager.ActiveConfiguration.Properties == null)
                return string.Empty;
            EnvDTE.Properties properties = proj.ConfigurationManager.ActiveConfiguration.Properties;
            if (!this.HasProperty(properties, "OutputPath"))
                return string.Empty;
            string path2 = properties.Item((object)"OutputPath").Value.ToString();
            return Path.Combine(projectPath, path2);
        }

        private bool HasProperty(EnvDTE.Properties properties, string propertyName)
        {
            if (properties != null)
            {
                foreach (Property property in properties)
                {
                    if (property != null && property.Name == propertyName)
                        return true;
                }
            }
            return false;
        }
    }
}
