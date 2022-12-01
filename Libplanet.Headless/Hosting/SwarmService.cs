using Libplanet.Action;
using Libplanet.Net;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

namespace Libplanet.Headless.Hosting;

public class SwarmService<T> : BackgroundService, IDisposable
    where T : IAction, new()
{
    private readonly Swarm<T> _swarm;
    private readonly BoundPeer[] _peers;
    private readonly AsyncManualResetEvent? _readyForServices;

    public SwarmService(
        Swarm<T> swarm, BoundPeer[] peers, AsyncManualResetEvent? readyForServices = null)
    {
        _swarm = swarm;
        _peers = peers;
        _readyForServices = readyForServices;
    }

    private string getPeerString(BoundPeer peer)
    {
        var pubKey = peer.PublicKey.ToString();
        var hostAndPort = peer.ToString().Split('/')[1];
        var host = hostAndPort.Split(':')[0];
        var port = hostAndPort.Split(':')[1];
        return $"peerString: {pubKey},{host},{port}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_readyForServices is {})
        {
            await _readyForServices.WaitAsync(stoppingToken).ConfigureAwait(false);
        }

        _ = _swarm.WaitForRunningAsync().ContinueWith(_ =>
        {
            var peer = _swarm.AsPeer;
            var result = getPeerString(peer);
            Console.WriteLine(result);
        });
        await _swarm.AddPeersAsync(_peers, default, cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.PreloadAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
        await _swarm.StartAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
    }
}
