using System;
using System.Linq;
using EnvDTE;

namespace LHJ.Tools
{
    public class Comm
    {
        private const string CWindowPaneName = "[LHJ.Tools]Dll LocalCopy Set False";

        public static void WriteToDTEOutput(DTE dte, string text)
        {
            var window = dte.Application.Windows.Item(Constants.vsWindowKindOutput);
            var outputWindow = (OutputWindow)window.Object;
            var owp = GetOutputWindowPane(outputWindow);
            owp.OutputString(text + "\n");
            owp.Activate();
        }

        private static OutputWindowPane GetOutputWindowPane(OutputWindow outputWindow)
        {
            foreach (var outputWindowPane in outputWindow.OutputWindowPanes.Cast<OutputWindowPane>()
                                .Where(outputWindowPane => outputWindowPane.Name == CWindowPaneName))
                return outputWindowPane;

            return outputWindow.OutputWindowPanes.Add(CWindowPaneName);
        }
    }
}
