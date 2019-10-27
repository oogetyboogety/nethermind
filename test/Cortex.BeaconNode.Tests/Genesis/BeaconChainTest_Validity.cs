﻿using System.Linq;
using Cortex.BeaconNode.Configuration;
using Cortex.BeaconNode.Tests.Helpers;
using Cortex.Containers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Shouldly;

namespace Cortex.BeaconNode.Tests.Genesis
{
    [TestClass]
    public class BeaconChainTest_Validity
    {
        public BeaconState CreateValidBeaconState(BeaconChain beaconChain, BeaconChainUtility beaconChainUtility,
            ChainConstants chainConstants, InitialValues initialValues, MiscellaneousParameters miscellaneousParameters, GweiValues gweiValues,
            TimeParameters timeParameters, ulong? eth1TimestampOverride = null)
        {
            var depositCount = miscellaneousParameters.MinimumGenesisActiveValidatorCount;
            (var deposits, _) = TestData.PrepareGenesisDeposits(chainConstants, initialValues, timeParameters, beaconChainUtility, depositCount, gweiValues.MaximumEffectiveBalance, signed: true);
            var eth1BlockHash = new Hash32(Enumerable.Repeat((byte)0x12, 32).ToArray());
            var eth1Timestamp = eth1TimestampOverride ?? miscellaneousParameters.MinimumGenesisTime;
            var state = beaconChain.InitializeBeaconStateFromEth1(eth1BlockHash, eth1Timestamp, deposits);
            return state;
        }

        public void IsValidGenesisState(BeaconChain beaconChain, BeaconState state, bool valid)
        {
            var isValid = beaconChain.IsValidGenesisState(state);
            isValid.ShouldBe(valid);
        }

        [TestMethod]
        public void IsValidGenesisStateTrue()
        {
            // Arrange
            TestConfiguration.GetMinimalConfiguration(
                out var chainConstants,
                out var miscellaneousParameterOptions,
                out var gweiValueOptions,
                out var initialValueOptions,
                out var timeParameterOptions,
                out var stateListLengthOptions,
                out var maxOperationsPerBlockOptions);

            var loggerFactory = new LoggerFactory(new[] {
                new ConsoleLoggerProvider(TestOptionsMonitor.Create(new ConsoleLoggerOptions()))
            });

            var cryptographyService = new CryptographyService();
            var beaconChainUtility = new BeaconChainUtility(miscellaneousParameterOptions, gweiValueOptions, timeParameterOptions, 
                cryptographyService);
            var beaconStateAccessor = new BeaconStateAccessor(miscellaneousParameterOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions,
                cryptographyService, beaconChainUtility);

            var beaconChain = new BeaconChain(loggerFactory.CreateLogger<BeaconChain>(), chainConstants, miscellaneousParameterOptions,
                gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, maxOperationsPerBlockOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor);

            // Act
            var state = CreateValidBeaconState(beaconChain, beaconChainUtility, chainConstants, initialValueOptions.CurrentValue, miscellaneousParameterOptions.CurrentValue, gweiValueOptions.CurrentValue, timeParameterOptions.CurrentValue);

            // Assert
            IsValidGenesisState(beaconChain, state, true);
        }

        [TestMethod]
        public void IsValidGenesisStateFalseInvalidTimestamp()
        {
            // Arrange
            TestConfiguration.GetMinimalConfiguration(
                out var chainConstants,
                out var miscellaneousParameterOptions,
                out var gweiValueOptions,
                out var initialValueOptions,
                out var timeParameterOptions,
                out var stateListLengthOptions,
                out var maxOperationsPerBlockOptions);

            var loggerFactory = new LoggerFactory(new[] {
                new ConsoleLoggerProvider(TestOptionsMonitor.Create(new ConsoleLoggerOptions()))
            });

            var cryptographyService = new CryptographyService();
            var beaconChainUtility = new BeaconChainUtility(miscellaneousParameterOptions, gweiValueOptions, timeParameterOptions,
                cryptographyService);
            var beaconStateAccessor = new BeaconStateAccessor(miscellaneousParameterOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions,
                cryptographyService, beaconChainUtility);

            var beaconChain = new BeaconChain(loggerFactory.CreateLogger<BeaconChain>(), chainConstants, miscellaneousParameterOptions,
                gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, maxOperationsPerBlockOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor);

            // Act
            var state = CreateValidBeaconState(beaconChain, beaconChainUtility, chainConstants, initialValueOptions.CurrentValue, miscellaneousParameterOptions.CurrentValue,
                gweiValueOptions.CurrentValue, timeParameterOptions.CurrentValue, eth1TimestampOverride: (miscellaneousParameterOptions.CurrentValue.MinimumGenesisTime - 3 * chainConstants.SecondsPerDay));

            // Assert
            IsValidGenesisState(beaconChain, state, false);
        }

        [TestMethod]
        public void IsValidGenesisStateTrueMoreBalance()
        {
            // Arrange
            TestConfiguration.GetMinimalConfiguration(
                out var chainConstants,
                out var miscellaneousParameterOptions,
                out var gweiValueOptions,
                out var initialValueOptions,
                out var timeParameterOptions,
                out var stateListLengthOptions,
                out var maxOperationsPerBlockOptions);

            var loggerFactory = new LoggerFactory(new[] {
                new ConsoleLoggerProvider(TestOptionsMonitor.Create(new ConsoleLoggerOptions()))
            });

            var cryptographyService = new CryptographyService();
            var beaconChainUtility = new BeaconChainUtility(miscellaneousParameterOptions, gweiValueOptions, timeParameterOptions,
                cryptographyService);
            var beaconStateAccessor = new BeaconStateAccessor(miscellaneousParameterOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions,
                cryptographyService, beaconChainUtility);

            var beaconChain = new BeaconChain(loggerFactory.CreateLogger<BeaconChain>(), chainConstants, miscellaneousParameterOptions,
                gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, maxOperationsPerBlockOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor);

            // Act
            var state = CreateValidBeaconState(beaconChain, beaconChainUtility, chainConstants, initialValueOptions.CurrentValue, miscellaneousParameterOptions.CurrentValue,
                gweiValueOptions.CurrentValue, timeParameterOptions.CurrentValue);
            state.Validators[0].SetEffectiveBalance(gweiValueOptions.CurrentValue.MaximumEffectiveBalance + (Gwei)1);

            // Assert
            IsValidGenesisState(beaconChain, state, true);
        }

        [TestMethod]
        public void IsValidGenesisStateTrueOneMoreValidator()
        {
            // Arrange
            TestConfiguration.GetMinimalConfiguration(
                out var chainConstants,
                out var miscellaneousParameterOptions,
                out var gweiValueOptions,
                out var initialValueOptions,
                out var timeParameterOptions,
                out var stateListLengthOptions,
                out var maxOperationsPerBlockOptions);

            var loggerFactory = new LoggerFactory(new[] {
                new ConsoleLoggerProvider(TestOptionsMonitor.Create(new ConsoleLoggerOptions()))
            });

            var cryptographyService = new CryptographyService();
            var beaconChainUtility = new BeaconChainUtility(miscellaneousParameterOptions, gweiValueOptions, timeParameterOptions,
                cryptographyService);
            var beaconStateAccessor = new BeaconStateAccessor(miscellaneousParameterOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions,
                cryptographyService, beaconChainUtility);

            var beaconChain = new BeaconChain(loggerFactory.CreateLogger<BeaconChain>(), chainConstants, miscellaneousParameterOptions,
                gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, maxOperationsPerBlockOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor);

            var depositCount = miscellaneousParameterOptions.CurrentValue.MinimumGenesisActiveValidatorCount + 1;
            (var deposits, _) = TestData.PrepareGenesisDeposits(chainConstants, initialValueOptions.CurrentValue, timeParameterOptions.CurrentValue, beaconChainUtility, depositCount, gweiValueOptions.CurrentValue.MaximumEffectiveBalance, signed: true);
            var eth1BlockHash = new Hash32(Enumerable.Repeat((byte)0x12, 32).ToArray());
            var eth1Timestamp = miscellaneousParameterOptions.CurrentValue.MinimumGenesisTime;

            // Act
            var state = beaconChain.InitializeBeaconStateFromEth1(eth1BlockHash, eth1Timestamp, deposits);

            // Assert
            IsValidGenesisState(beaconChain, state, true);
        }

        [TestMethod]
        public void IsValidGenesisStateFalseNotEnoughValidators()
        {
            // Arrange
            TestConfiguration.GetMinimalConfiguration(
                out var chainConstants,
                out var miscellaneousParameterOptions,
                out var gweiValueOptions,
                out var initialValueOptions,
                out var timeParameterOptions,
                out var stateListLengthOptions,
                out var maxOperationsPerBlockOptions);

            var loggerFactory = new LoggerFactory(new[] {
                new ConsoleLoggerProvider(TestOptionsMonitor.Create(new ConsoleLoggerOptions()))
            });

            var cryptographyService = new CryptographyService();
            var beaconChainUtility = new BeaconChainUtility(miscellaneousParameterOptions, gweiValueOptions, timeParameterOptions,
                cryptographyService);
            var beaconStateAccessor = new BeaconStateAccessor(miscellaneousParameterOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions,
                cryptographyService, beaconChainUtility);

            var beaconChain = new BeaconChain(loggerFactory.CreateLogger<BeaconChain>(), chainConstants, miscellaneousParameterOptions,
                gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, maxOperationsPerBlockOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor);

            var depositCount = miscellaneousParameterOptions.CurrentValue.MinimumGenesisActiveValidatorCount - 1;
            (var deposits, _) = TestData.PrepareGenesisDeposits(chainConstants, initialValueOptions.CurrentValue, timeParameterOptions.CurrentValue, beaconChainUtility, depositCount, gweiValueOptions.CurrentValue.MaximumEffectiveBalance, signed: true);
            var eth1BlockHash = new Hash32(Enumerable.Repeat((byte)0x12, 32).ToArray());
            var eth1Timestamp = miscellaneousParameterOptions.CurrentValue.MinimumGenesisTime;

            // Act
            var state = beaconChain.InitializeBeaconStateFromEth1(eth1BlockHash, eth1Timestamp, deposits);

            // Assert
            IsValidGenesisState(beaconChain, state, false);
        }
    }
}
