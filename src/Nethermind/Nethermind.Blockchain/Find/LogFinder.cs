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

using System;
using System.Collections.Generic;
using System.Linq;
using Nethermind.Blockchain.Filters;
using Nethermind.Blockchain.Receipts;
using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.Blockchain.Find
{
    public class LogFinder : ILogFinder
    {
        private readonly IReceiptStorage _receiptStorage;
        private readonly IBlockFinder _blockFinder;
        private const long PendingBlockNumber = long.MaxValue;

        public LogFinder(IBlockFinder blockFinder, IReceiptStorage receiptStorage)
        {
            _receiptStorage = receiptStorage;
            _blockFinder = blockFinder;
        }
        
        public FilterLog[] FindLogs(LogFilter filter)
        {
            var currentBlock = _blockFinder.GetBlock(filter.ToBlock);
            var fromBlock = _blockFinder.GetBlock(filter.FromBlock);
            List<FilterLog> results = new List<FilterLog>();
            
            while (currentBlock.Number >= fromBlock.Number)
            {
                if (filter.Matches(currentBlock.Bloom))
                {
                    var receipts = GetReceiptsFromBlock(currentBlock);
                    long logIndex = 0;
                    foreach (var receipt in receipts)
                    {
                        if (filter.Matches(receipt.Bloom))
                        {
                            foreach (var log in receipt.Logs)
                            {
                                if (filter.Accepts(log))
                                {
                                    results.Add(new FilterLog(logIndex, receipt, log));
                                }

                                logIndex++;
                            }
                        }
                        else
                        {
                            logIndex += receipt.Logs.Length;
                        }
                    }
                }
                
                if (!TryGetParentBlock(currentBlock, out currentBlock))
                {
                    break;
                }
            }

            return results.ToArray();
        }

        private IEnumerable<TxReceipt> GetReceiptsFromBlock(Block block) =>
            block.Body.Transactions.Select(transaction => _receiptStorage.Find(transaction.Hash));
        
        private bool TryGetParentBlock(Block currentBlock, out Block parentBlock)
        {
            if (currentBlock.IsGenesis)
            {
                parentBlock = null;
                return false;
            }
            else
            {
                parentBlock = _blockFinder.FindBlock(currentBlock.ParentHash);
                return true;                
            }
        }
    }
}