using Hermes.Repositories;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Hermes.Services;

public class SfcSenderService
{
    private readonly SfcService _sfcService;
    private readonly ConcurrentQueue<string> _pendingFiles = new();
    private readonly FolderWatcherService _folderWatcherService;
    private CancellationTokenSource _cancellationTokenSource;

    public SfcSenderService(SfcService sfcService, FolderWatcherService folderWatcherService)
    {
        this._sfcService = sfcService;
        this._folderWatcherService = folderWatcherService;
        this._folderWatcherService.FileCreated += this.OnFileCreated;
    }

    private void OnFileCreated(object? sender, string fullPath)
    {
        this._pendingFiles.Enqueue(fullPath);
    }

    public void Start()
    {
        this._folderWatcherService.Start();
        this._cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => this.ProcessFilesAsync(this._cancellationTokenSource.Token));
        this._logger.Info("Watch logfiles started");
    }

    private async void ProcessFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            this._session.SetIdleState();
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!this._session.IsStopped && this._pendingFiles.TryDequeue(out var fullPath))
                {
                    this._session.SetProcessingState();
                    await this.ProcessFileAsync(fullPath);
                }
                else
                {
                    await Task.Delay(Settings.Instance.WatchLogfilesDelayMilliseconds, cancellationToken);
                }
            }
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            this._logger.Error("Watch logfiles stopped");
        }
        catch (Exception e)
        {
            this._logger.Error(e.Message);
        }
        finally
        {
            this._cancellationTokenSource?.Dispose();
        }
    }

    public void Stop()
    {
        this._folderWatcherService.Stop();
    }

    public void SendToSfc(string fullPath)
    {
        //TODO
        throw new NotImplementedException();
    }
}