using System;
using System.Diagnostics;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Extension4WithVSSDK
{
    public sealed class ErrorListMonitorService
    {
        private static readonly ErrorListMonitorService _instance = new ErrorListMonitorService();
        public static ErrorListMonitorService Instance => _instance;

        private readonly SimpleFileLogger _logger = new SimpleFileLogger();
        private DTE2 _dte;
        private CancellationTokenSource _cts;
        private bool _hadErrorsBefore;
        private bool _isAlertActive;

        private ErrorListMonitorService()
        {
        }

        public void Start()
        {
            if (_cts != null)
                return;

            _logger.Log("ErrorListMonitorService.Start chamado.");
            _cts = new CancellationTokenSource();
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(() => MonitorLoopAsync(_cts.Token));
        }

        private async Task MonitorLoopAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("[ErrorListMonitor] Loop iniciado. Aguardando VS estabilizar...");
            _logger.Log("Loop de monitoramento iniciado.");

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                    if (_dte is null)
                    {
                        _dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                        if (_dte is null)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                            continue;
                        }
                    }

                    if (!_isAlertActive)
                    {
                        bool hasErrors = HasBuildErrors();

                        if (hasErrors && !_hadErrorsBefore)
                        {
                            _hadErrorsBefore = true;
                            _isAlertActive = true;

                            await Task.Delay(2000, cancellationToken);
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                            if (HasBuildErrors())
                            {
                                var window = new ErrorAlertWindow();
                                window.Closed += (_, __) =>
                                {
                                    _isAlertActive = false;
                                };
                                window.Show();
                            }
                            else
                            {
                                _isAlertActive = false;
                            }
                        }
                        else if (!hasErrors)
                        {
                            _hadErrorsBefore = false;
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Loop cancelado.");
            }
            catch (Exception ex)
            {
                _logger.Log($"Falha no loop: {ex}");
            }
        }

        private bool HasBuildErrors()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var errorList = _dte.ToolWindows.ErrorList;
                var items = errorList.ErrorItems;
                int count = items.Count;
                Debug.WriteLine($"[ErrorListMonitor] ErrorItems.Count = {count}");

                for (int i = 1; i <= count; i++)
                {
                    if (items.Item(i).ErrorLevel == vsBuildErrorLevel.vsBuildErrorLevelHigh)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
