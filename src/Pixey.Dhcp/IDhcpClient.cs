using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixey.Dhcp
{
    public interface IDhcpClient
    {
        Task<IReadOnlyList<IDhcpPacketView>> DiscoverDhcpServers(DhcpDiscoveryParameters parameters, CancellationToken ct);
    }
}