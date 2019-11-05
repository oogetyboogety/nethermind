﻿namespace Cortex.BeaconNode.Configuration
{
    public class MaxOperationsPerBlock
    {
        public ulong MaximumAttestations { get; set; }
        public ulong MaximumDeposits { get; set; }
        public ulong MaximumProposerSlashings { get; set; }
    }
}
