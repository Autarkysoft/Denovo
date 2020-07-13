// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods that a P2P message reply manager implements.
    /// </summary>
    public interface IReplyManager
    {
        /// <summary>
        /// Builds and returns a version message
        /// </summary>
        /// <returns>A version message</returns>
        Message GetVersionMsg();

        /// <summary>
        /// Builds and returns the appropriate response to the given <see cref="Message"/>.
        /// In case there is no response null should be returned.
        /// </summary>
        /// <param name="msg"><see cref="Message"/> to reply to</param>
        /// <returns>Response <see cref="Message"/> or null</returns>
        Message[] GetReply(Message msg);
    }
}
