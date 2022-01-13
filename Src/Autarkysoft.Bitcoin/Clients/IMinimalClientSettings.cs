// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Defines methods and properties of a minimal client and is used by all <see cref="Node"/> instances.
    /// Inherits from <see cref="IClientSettings"/>.
    /// </summary>
    public interface IMinimalClientSettings : IClientSettings
    {
    }
}
