﻿namespace Cortex.Containers
{
    public class BeaconBlock
    {
        public BeaconBlock(Hash32 genesisStateRoot)
        {
            Slot = new Slot(0);
            ParentRoot = Hash32.Zero;
            StateRoot = genesisStateRoot;
            Body = new BeaconBlockBody();
            Signature = new BlsSignature();
        }

        public BeaconBlock(Slot slot, Hash32 parentRoot, Hash32 stateRoot, BeaconBlockBody body, BlsSignature signature)
        {
            Slot = slot;
            ParentRoot = parentRoot;
            StateRoot = stateRoot;
            Body = body;
            Signature = signature;
            //Body = new BeaconBlockBody(randaoReveal,
            //    new Eth1Data(Hash32.Zero, 0),
            //    new Bytes32(), Array.Empty<Deposit>());
        }

        public BeaconBlockBody Body { get; }
        public Hash32 ParentRoot { get; }
        public BlsSignature Signature { get; private set; }
        public Slot Slot { get; private set; }
        public Hash32 StateRoot { get; }

        public void SetSignature(BlsSignature signature)
        {
            Signature = signature;
        }

        public void SetSlot(Slot slot)
        {
            Slot = slot;
        }
    }
}
