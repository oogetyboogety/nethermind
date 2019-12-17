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

using Nethermind.Core.Extensions;
using Nethermind.DataMarketplace.Core.Domain;
using Nethermind.DataMarketplace.Subprotocols.Messages;
using Nethermind.Network;

namespace Nethermind.DataMarketplace.Subprotocols.Serializers
{
    public class InvalidDataMessageSerializer : IMessageSerializer<InvalidDataMessage>
    {
        public byte[] Serialize(InvalidDataMessage headerDataMessage)
            => Nethermind.Core.Encoding.Rlp.Encode(
                Nethermind.Core.Encoding.Rlp.Encode(headerDataMessage.DepositId),
                Nethermind.Core.Encoding.Rlp.Encode((int) headerDataMessage.Reason)).Bytes;

        public InvalidDataMessage Deserialize(byte[] bytes)
        {
            var context = bytes.AsRlpStream();
            context.ReadSequenceLength();
            var depositId = context.DecodeKeccak();
            var reason = (InvalidDataReason) context.DecodeInt();

            return new InvalidDataMessage(depositId, reason);
        }
    }
}