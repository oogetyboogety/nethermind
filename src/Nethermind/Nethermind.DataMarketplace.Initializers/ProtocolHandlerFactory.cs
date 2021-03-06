//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using Nethermind.DataMarketplace.Core.Domain;
using Nethermind.DataMarketplace.Core.Services;
using Nethermind.DataMarketplace.Subprotocols;
using Nethermind.Logging;
using Nethermind.Network;
using Nethermind.Network.P2P;

namespace Nethermind.DataMarketplace.Initializers
{
    public class ProtocolHandlerFactory : IProtocolHandlerFactory
    {
        private readonly INdmSubprotocolFactory _subprotocolFactory;
        private readonly IProtocolValidator _protocolValidator;
        private readonly IEthRequestService _ethRequestService;
        private readonly ILogger _logger;

        public ProtocolHandlerFactory(INdmSubprotocolFactory subprotocolFactory, IProtocolValidator protocolValidator,
            IEthRequestService ethRequestService, ILogManager logManager)
        {
            _subprotocolFactory = subprotocolFactory;
            _protocolValidator = protocolValidator;
            _ethRequestService = ethRequestService;
            _logger = logManager.GetClassLogger();
        }
        
        public IProtocolHandler Create(ISession session)
        {
            var handler = _subprotocolFactory.Create(session);
            handler.ProtocolInitialized += (sender, args) =>
            {
                var ndmEventArgs = (NdmProtocolInitializedEventArgs) args;
                _protocolValidator.DisconnectOnInvalid(Protocol.Ndm, session, ndmEventArgs);
                if (_logger.IsTrace) _logger.Trace($"NDM version {handler.ProtocolVersion}: {session.RemoteNodeId}, host: {session.Node.Host}");
                if (string.IsNullOrWhiteSpace(_ethRequestService.FaucetHost) ||
                    !session.Node.Host.Contains(_ethRequestService.FaucetHost))
                {
                    return;
                }

                _ethRequestService.UpdateFaucet(handler as INdmPeer);
            };

            return handler;
        }
    }
}