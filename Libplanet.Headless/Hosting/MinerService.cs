using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Crypto;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

namespace Libplanet.Headless.Hosting;

public class MinerService<T> : BackgroundService, IDisposable
    where T : IAction, new()
{
    private readonly BlockChain<T> _blockChain;

    private readonly PrivateKey _privateKey;

    private readonly AsyncManualResetEvent? _readyForServices;

    public MinerService(
        BlockChain<T> blockChain,
        PrivateKey minerPrivateKey,
        AsyncManualResetEvent? readyForServices = null)
    {
        _blockChain = blockChain;
        _privateKey = minerPrivateKey;
        _readyForServices = readyForServices;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_readyForServices is {})
        {
            await _readyForServices.WaitAsync(stoppingToken).ConfigureAwait(false);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await _blockChain.MineBlock(_privateKey).ConfigureAwait(false);
            stoppingToken.ThrowIfCancellationRequested();
        }
    }
}
