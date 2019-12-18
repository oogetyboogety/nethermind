﻿//  Copyright (c) 2018 Demerzel Solutions Limited
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.BeaconNode.Storage;
using Nethermind.Core2.Containers;
using Nethermind.Core2.Crypto;
using Nethermind.Core2.Types;
using BeaconBlock = Nethermind.BeaconNode.Containers.BeaconBlock;
using BeaconState = Nethermind.BeaconNode.Containers.BeaconState;

namespace Nethermind.BeaconNode
{
    public class BeaconNodeFacade : IBeaconNodeApi
    {
        private readonly ClientVersion _clientVersion;
        private readonly ForkChoice _forkChoice;
        private readonly IStoreProvider _storeProvider;
        private readonly ValidatorAssignments _validatorAssignments;
        private readonly BlockProducer _blockProducer;

        public BeaconNodeFacade(            
            ClientVersion clientVersion,
            ForkChoice forkChoice, 
            IStoreProvider storeProvider,
            ValidatorAssignments validatorAssignments,
            BlockProducer blockProducer)
        {
            _clientVersion = clientVersion;
            _forkChoice = forkChoice;
            _storeProvider = storeProvider;
            _validatorAssignments = validatorAssignments;
            _blockProducer = blockProducer;
        }
        
        public async Task<string> GetNodeVersionAsync()
        {
            return await Task.Run(() => _clientVersion.Description);
        }

        public async Task<ulong> GetGenesisTimeAsync()
        {
            BeaconState state = await GetHeadStateAsync();
            return state.GenesisTime;
        }

        public Task<bool> GetIsSyncingAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<Fork> GetNodeForkAsync()
        {
            BeaconState state = await GetHeadStateAsync();
            return state.Fork;
        }

        public async IAsyncEnumerable<ValidatorDuty> ValidatorDutiesAsync(IEnumerable<BlsPublicKey> validatorPublicKeys, Epoch epoch)
        {
            // TODO: Rather than check one by one (each of which loops through potentially all slots for the epoch), optimise by either checking multiple, or better possibly caching or pre-calculating
            foreach (BlsPublicKey validatorPublicKey in validatorPublicKeys)
            {
                ValidatorDuty validatorDuty = await _validatorAssignments.GetValidatorDutyAsync(validatorPublicKey, epoch);
                yield return validatorDuty;
            }
        }
        
        public async Task<BeaconBlock> NewBlockAsync(Slot slot, BlsSignature randaoReveal)
        {
            return await _blockProducer.NewBlockAsync(slot, randaoReveal);
        }
        
        private async Task<BeaconState> GetHeadStateAsync()
        {
            if (!_storeProvider.TryGetStore(out IStore? retrievedStore))
            {
                throw new Exception("Beacon chain is currently syncing or waiting for genesis.");
            }

            IStore store = retrievedStore!;
            Hash32 head = await _forkChoice.GetHeadAsync(store);
            if (!store.TryGetBlockState(head, out BeaconState? state))
            {
                throw new Exception($"Beacon chain is currently syncing, head state {head} not found.");
            }

            return state!;
        }

    }
}