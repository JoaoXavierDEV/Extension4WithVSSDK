using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Extension4WithVSSDK
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid("0D58D8ED-71F3-40D8-BE72-4D5F3B1F5E25")]
    internal sealed class StartupPackage : AsyncPackage
    {
        private static readonly string LogFile =
            Path.Combine(Path.GetTempPath(), "ErrorListMonitor.log");

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            try
            {
                Log("StartupPackage.InitializeAsync chamado");
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                Log("Na UI thread, iniciando monitor...");
                ErrorListMonitorService.Instance.Start();
                Log("Monitor iniciado com sucesso");
            }
            catch (Exception ex)
            {
                Log($"ERRO: {ex}");
            }
        }

        private static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss.fff} | {message}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
