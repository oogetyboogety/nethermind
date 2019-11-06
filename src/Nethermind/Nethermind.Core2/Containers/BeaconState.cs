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

using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;

namespace Nethermind.Core2.Containers
{
    public class BeaconState
    {
        public ulong GenesisTime { get; set; }
        public Slot Slot { get; set; }
        public Fork Fork { get; set; }
        public BeaconBlockHeader LatestBlockHeader { get; set; }
        public Sha256[] BlockRoots { get; set; }
        public Sha256[] StateRoots { get; set; }
        public Sha256[] HistoricalRoots { get; set; }
        public Eth1Data Eth1Data { get; set; }
        public Eth1Data EthDataVotes { get; set; }
        public ulong Eth1DepositIndex { get; set; }
        public Validator[] Validators { get; set; }
        public Gwei[] Balances { get; set; }
        public Sha256 RandaoMixes { get; set; }
        public Gwei[] Slashings { get; set; }
        public PendingAttestation[] PreviousEpochAttestations { get; set; }
        public PendingAttestation[] CurrentEpochAttestations { get; set; }
        public byte[] JustificationBits { get; set; }
        public Checkpoint PreviousJustifiedCheckpoint { get; set; }
        public Checkpoint CurrentJustifiedCheckpoint { get; set; }
        public Checkpoint FinalizedCheckpoint { get; set; }

        public static int SszLength(BeaconState container)
        {
            throw new System.NotImplementedException();
        }
    }
}