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
using Microsoft.Extensions.Logging;

namespace Nethermind.HonestValidator
{
    internal static class Log
    {
        // Event IDs: ABxx (based on Theory of Reply Codes)
        
        // Event ID Type:
        // 6bxx debug - general
        // 7bxx debug - test
        // 1bxx info - preliminary
        // 2bxx info - completion
        // 3bxx info - intermediate
        // 8bxx info - finalization
        // 4bxx warning
        // 5bxx error
        // 9bxx critical
        
        // Event ID Category:
        // a0xx core service, worker, configuration, peering
        // a1xx beacon chain, incl. state transition
        // a2xx fork choice
        // a3xx deposit contract, Eth1, genesis
        // a4xx honest validator, API
        // a5xx custody game
        // a6xx shard data chains
        // a9xx miscellaneous / other
        
        // 1bxx preliminary

        public static readonly Action<ILogger, string, string, int, Exception?> HonestValidatorWorkerExecuteStarted =
            LoggerMessage.Define<string, string, int>(LogLevel.Information,
                new EventId(1400, nameof(HonestValidatorWorkerExecuteStarted)),
                "{ProductTokenVersion} honest validator started; {Environment} environment [{ThreadId}]");
        
        // 5bxx error
        
        public static readonly Action<ILogger, Exception?> HonestValidatorWorkerLoopError =
            LoggerMessage.Define(LogLevel.Error,
                new EventId(5400, nameof(HonestValidatorWorkerLoopError)),
                "Unexpected error caught in honest validator worker, loop continuing.");

        // 8bxx finalization

        // 9bxx critical

        public static readonly Action<ILogger, Exception?> HonestValidatorWorkerCriticalError =
            LoggerMessage.Define(LogLevel.Critical,
                new EventId(9400, nameof(HonestValidatorWorkerCriticalError)),
                "Critical unhandled error in honest validator worker. Worker cannot continue.");

    }
}