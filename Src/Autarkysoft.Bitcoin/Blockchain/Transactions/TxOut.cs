// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    /// <summary>
    /// The output of transactions containing payment amount and <see cref="IPubkeyScript"/>.
    /// Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public class TxOut : IDeserializable
    {
        /// <summary>
        /// Initializes a new and empty instance of <see cref="TxOut"/>.
        /// </summary>
        public TxOut()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TxOut"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="amount">Payment amount in coin's smallest unit (eg. Satoshi).</param>
        /// <param name="pkScript">Public key script</param>
        public TxOut(ulong amount, IPubkeyScript pkScript)
        {
            Amount = amount;
            PubScript = pkScript;
        }



        /// <summary>
        /// Minimum size of a TxOut. Amount(8) + CompactInt(1) + Script(0)
        /// </summary>
        internal int MinSize = sizeof(ulong) + 1;

        private ulong _amount;
        /// <summary>
        /// The amount, in satoshi, to send in this output.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ulong Amount
        {
            get => _amount;
            set
            {
                if (value > Constants.TotalSupply)
                    throw new ArgumentOutOfRangeException(nameof(Amount), "Amount is bigger than total supply.");
                _amount = value;
            }
        }

        private IPubkeyScript _pubScr = new PubkeyScript();
        /// <summary>
        /// The pubkey (locking) script
        /// </summary>
        public IPubkeyScript PubScript
        {
            get => _pubScr;
            set => _pubScr = (value is null) ? new PubkeyScript() : value;
        }


        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter)
        {
            counter.AddUInt64();
            PubScript.AddSerializedSize(counter);
        }

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(Amount);
            PubScript.Serialize(stream);
        }

        /// <summary>
        /// Writes a special <see cref="TxOut"/> format to the given stream that is used for signing transactions
        /// with <see cref="Cryptography.Asymmetric.EllipticCurve.SigHashType.Single"/> type.
        /// <para/> Amount=-1 and script=empty => [8 byte -1] [0x00]
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void SerializeSigHashSingle(FastStream stream)
        {
            stream.Write(new byte[9] { 255, 255, 255, 255, 255, 255, 255, 255, 0 });
        }


        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!stream.TryReadUInt64(out _amount))
            {
                error = Errors.EndOfStream;
                return false;
            }
            if (_amount > Constants.TotalSupply)
            {
                error = Errors.TxAmountOverflow;
                return false;
            }

            if (!PubScript.TryDeserialize(stream, out error))
            {
                return false;
            }

            return true;
        }
    }
}
