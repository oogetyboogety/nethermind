/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using Nethermind.Abi;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.DataMarketplace.Core.Configs;
using Nethermind.DataMarketplace.Core.Domain;
using Nethermind.DataMarketplace.Core.Services.Models;
using Nethermind.Dirichlet.Numerics;
using Nethermind.Facade;
using Nethermind.Logging;
using Nethermind.Wallet;

namespace Nethermind.DataMarketplace.Core.Services
{
    public class DepositService : IDepositService
    {
        private readonly IAbiEncoder _abiEncoder;
        private readonly IBlockchainBridge _blockchainBridge;
        private readonly IWallet _wallet;
        private readonly ILogger _logger;
        private Address _consumerAddress;
        private readonly Address _contractAddress;

        public DepositService(IBlockchainBridge blockchainBridge, IAbiEncoder abiEncoder, IWallet wallet,
            INdmConfig ndmConfig, ILogManager logManager)
        {
            _blockchainBridge = blockchainBridge ?? throw new ArgumentNullException(nameof(blockchainBridge));
            _abiEncoder = abiEncoder ?? throw new ArgumentNullException(nameof(abiEncoder));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            if (ndmConfig is null)
            {
                throw new ArgumentNullException(nameof(ndmConfig));
            }

            _logger = logManager?.GetClassLogger() ?? throw new ArgumentNullException(nameof(logManager));
            _consumerAddress = string.IsNullOrWhiteSpace(ndmConfig.ConsumerAddress)
                ? Address.Zero
                : new Address(ndmConfig.ConsumerAddress);
            _contractAddress = string.IsNullOrWhiteSpace(ndmConfig.ContractAddress)
                ? Address.Zero
                : new Address(ndmConfig.ContractAddress);
        }

        public UInt256 ReadDepositBalance(Keccak depositId)
        {
            var txData = _abiEncoder.Encode(AbiEncodingStyle.IncludeSignature, ContractData.DepositBalanceAbiSig, depositId.Bytes);
            Transaction transaction = new Transaction();
            transaction.Value = 0;
            transaction.Data = txData;
            transaction.To = _contractAddress;
            transaction.SenderAddress = _consumerAddress;
            transaction.GasLimit = 100000;
            transaction.GasPrice = 0.GWei();
            transaction.Nonce = (UInt256) _blockchainBridge.GetNonce(_consumerAddress);
            _wallet.Sign(transaction, _blockchainBridge.GetNetworkId());
            BlockchainBridge.CallOutput callOutput = _blockchainBridge.Call(_blockchainBridge.Head, transaction);
            return (callOutput.OutputData ?? new byte[] {0}).ToUInt256();
        }

        public void ChangeConsumerAddress(Address address)
        {
            _consumerAddress = address;
        }

        public void ValidateContractAddress(Address contractAddress)
        {
            if (contractAddress != _contractAddress)
            {
                throw new InvalidDataException($"Contract address {contractAddress} is different than configured {_contractAddress}");
            }

            if (_blockchainBridge.GetCode(contractAddress).Length == 0)
            {
                throw new InvalidDataException($"No contract code at address {contractAddress}.");
            }
            
            if (!Bytes.AreEqual(_blockchainBridge.GetCode(contractAddress), Bytes.FromHexString(ContractData.DeployedCode)))
            {
                throw new InvalidDataException($"Code at address {contractAddress} is different than expected.");
            }
        }

        public Keccak MakeDeposit(Address onBehalfOf, Deposit deposit)
        {
            var txData = _abiEncoder.Encode(AbiEncodingStyle.IncludeSignature, ContractData.DepositAbiSig, deposit.Id.Bytes, deposit.Units, deposit.ExpiryTime);
            Transaction transaction = new Transaction();
            transaction.Value = deposit.Value;
            transaction.Data = txData;
            transaction.To = _contractAddress;
            transaction.SenderAddress = onBehalfOf;
            transaction.GasLimit = 70000; // check  
            transaction.GasPrice = 20.GWei();
            transaction.Nonce = (UInt256) _blockchainBridge.GetNonce(onBehalfOf);
            _wallet.Sign(transaction, _blockchainBridge.GetNetworkId());
            return _blockchainBridge.SendTransaction(transaction);
        }

        public uint VerifyDeposit(Keccak depositId)
        {
            var txData = _abiEncoder.Encode(AbiEncodingStyle.IncludeSignature, ContractData.VerifyDepositAbiSig, depositId.Bytes);
            Transaction transaction = new Transaction();
            transaction.Value = 0;
            transaction.Data = txData;
            transaction.To = _contractAddress;
            transaction.SenderAddress = _consumerAddress;
            transaction.GasLimit = 100000;
            transaction.GasPrice = 0.GWei();
            transaction.Nonce = (UInt256) _blockchainBridge.GetNonce(_consumerAddress);
            BlockchainBridge.CallOutput callOutput = _blockchainBridge.Call(_blockchainBridge.Head, transaction);
            return (callOutput.OutputData ?? new byte[] {0}).ToUInt32();
        }
    }
}