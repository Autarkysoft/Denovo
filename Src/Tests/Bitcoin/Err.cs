// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Tests.Bitcoin
{
    public class Err
    {
        public const string InvalidNetwork = "Given network type is not valid.";
        public const string EndOfStream = "Reached end of stream.";
        public const string OpNotEnoughItems = "Not enough items left in the stack.";
        public const string OpStackItemOverflow = "Stack item count limit exceeded.";
        public const string OpCountOverflow = "Number of OPs in this script exceeds the allowed number.";
        public const string BadRNG = "The provided RNG is broken.";
        internal const string ZeroByteWitness = "Data part of SegWit outputs can not be all zero bytes.";
    }
}
