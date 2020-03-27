// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Base (abstract) class for all scripts. Inherits from <see cref="IScript"/>.
    /// </summary>
    /// <remarks>
    /// Scripts are categoried into two groups: "scripts" and "witnesses"
    /// 
    /// All scripts have the same structure: [CompactInt][list of operations].
    /// CompactInt is:
    ///   Length of the entire script for "scripts" (SignatureScript, PubkeyScript, RedeemScript)
    ///   Number of operations in the list for "witnesses"
    ///
    /// PushDataOp in scripts is always [DataLength][data].
    /// DataLength that indicates the size of the data to be pushed is of type
    ///   StackInt for "scripts"
    ///   CompactInt for "witnesses"
    /// </remarks>
    public abstract class Script : IScript
    {
        /// <summary>
        /// Initializes this instance with the given maximum length.
        /// </summary>
        /// <param name="maxLen">
        /// Maximum allowed length (for scripts) or item count (for witnesses) according to consensus rules.
        /// </param>
        public Script(int maxLen)
        {
            maxLenOrCount = maxLen;
        }


        /// <summary>
        /// Length of compressed public keys
        /// </summary>
        protected const int CompPubKeyLength = 33;
        /// <summary>
        /// Length of uncompressed public keys
        /// </summary>
        protected const int UncompPubKeyLength = 65;
        /// <summary>
        /// Maximum allowed length (for scripts) or item count (for witnesses) according to consensus rules.
        /// </summary>
        protected readonly int maxLenOrCount;
        private int len;

        /// <summary>
        /// [Default value = false] Returns whether the script instance is of witness type. 
        /// It will affect (de)serialization methods.
        /// </summary>
        public bool IsWitness { get; protected set; } = false;
        /// <inheritdoc/>
        public ScriptType ScriptType { get; set; }
        /// <inheritdoc/>
        public IOperation[] OperationList { get; set; } = new IOperation[0];

        

        /// <inheritdoc/>
        public virtual void Serialize(FastStream stream)
        {
            FastStream temp = new FastStream();
            ToByteArray(temp);
            CompactInt lengthOrCount = new CompactInt(IsWitness ? OperationList.Length : temp.GetSize());

            lengthOrCount.WriteToStream(stream);
            stream.Write(temp);
        }

        /// <inheritdoc/>
        public void ToByteArray(FastStream stream)
        {
            foreach (var op in OperationList)
            {
                if (op is PushDataOp push)
                {
                    push.WriteToStream(stream, IsWitness);
                }
                else if (op is IfElseOpsBase conditional)
                {
                    conditional.WriteToStream(stream);
                }
                else if (op is ReturnOp retOp)
                {
                    retOp.WriteToStream(stream);
                }
                else
                {
                    stream.Write((byte)op.OpValue);
                }
            }
        }

        /// <inheritdoc/>
        public byte[] ToByteArray()
        {
            FastStream stream = new FastStream(100);
            ToByteArray(stream);
            return stream.ToByteArray();
        }


        /// <inheritdoc/>
        public virtual bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt lengthOrCount, out error))
            {
                return false;
            }


            // TODO: change this to check length/count based on consensus rules.
            // and move it inside following condition (IsWitness)
            if (lengthOrCount > int.MaxValue)
            {
                error = (IsWitness ? "Item count" : "Script length") + "is too big.";
                return false;
            }

            if (IsWitness)
            {
                int count = (int)lengthOrCount;

                OperationList = new IOperation[count];
                for (int i = 0; i < count; i++)
                {
                    // TODO: the assumption here is that witness doesn't have anything but PushDataOp, this may be wrong.
                    PushDataOp op = new PushDataOp();
                    if (!op.TryRead(stream, out error, true))
                    {
                        return false;
                    }
                    OperationList[i] = op;
                }
            }
            else
            {
                len = (int)lengthOrCount;

                if (len == 0)
                {
                    OperationList = new IOperation[0];
                    error = null;
                    return true;
                }

                List<IOperation> opList = new List<IOperation>();
                int offset = 0;
                while (offset < len)
                {
                    if (!TryRead(stream, opList, ref offset, out error))
                    {
                        return false;
                    }
                }

                // Make sure the correct number of bytes were read as part of the script. No more, no less.
                if (offset != len)
                {
                    error = "Invalid stack format.";
                    return false;
                }
                OperationList = opList.ToArray();
            }

            error = null;
            return true;
        }


        private bool IsPushOp(byte b)
        {
            return b >= 0 && b <= (byte)OP._16 && b != (byte)OP.Reserved;
        }

        /// <summary>
        /// Reads a single <see cref="IOperation"/> from the given the given stream and adds the result to the given list.
        /// Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream of bytes to use</param>
        /// <param name="opList">The list to add the result to</param>
        /// <param name="offset">This value will be incremented based on how many bytes were read</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        protected bool TryRead(FastStreamReader stream, List<IOperation> opList, ref int offset, out string error)
        {
            if (!stream.TryPeekByte(out byte firstByte))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (IsPushOp(firstByte))
            {
                PushDataOp op = new PushDataOp();
                int start = stream.GetCurrentIndex();
                if (!op.TryRead(stream, out error, IsWitness))
                {
                    return false;
                }
                opList.Add(op);
                offset += stream.GetCurrentIndex() - start;
            }
            else if (firstByte == (byte)OP.RETURN)
            {
                ReturnOp op = new ReturnOp();
                int start = stream.GetCurrentIndex();
                if (!op.TryRead(stream, len, out error))
                {
                    return false;
                }
                opList.Add(op);
                offset += stream.GetCurrentIndex() - start;
            }
            else
            {
                _ = stream.TryReadByte(out firstByte);
                offset++;
                switch ((OP)firstByte)
                {
                    // Invalid OPs:
                    case OP.VerIf:
                    case OP.VerNotIf:
                        error = $"Invalid OP was found: {GetOpName((OP)firstByte)}";
                        return false;

                    // Disabled OPs:
                    case OP.CAT:
                    case OP.SubStr:
                    case OP.LEFT:
                    case OP.RIGHT:
                    case OP.INVERT:
                    case OP.AND:
                    case OP.OR:
                    case OP.XOR:
                    case OP.MUL2:
                    case OP.DIV2:
                    case OP.MUL:
                    case OP.DIV:
                    case OP.MOD:
                    case OP.LSHIFT:
                    case OP.RSHIFT:
                        error = $"Disabled OP was found: {GetOpName((OP)firstByte)}";
                        return false;

                    // Special case of IFs:
                    case OP.IF:
                        IFOp ifop = new IFOp(IsWitness);

                        // Peek at next byte to check if it is OP_EndIf or OP_Else
                        if (!stream.TryPeekByte(out byte nextB))
                        {
                            error = Err.EndOfStream;
                            return false;
                        }

                        List<IOperation> ifOps = new List<IOperation>();
                        while (offset < len && nextB != (byte)OP.EndIf && nextB != (byte)OP.ELSE)
                        {
                            if (!TryRead(stream, ifOps, ref offset, out error))
                            {
                                return false;
                            }
                            if (!stream.TryPeekByte(out nextB))
                            {
                                error = Err.EndOfStream;
                                return false;
                            }
                        }
                        ifop.mainOps = ifOps.ToArray();

                        if (offset < len)
                        {
                            if (!stream.TryReadByte(out byte currentB))
                            {
                                error = Err.EndOfStream;
                                return false;
                            }
                            offset++;

                            if (currentB == (byte)OP.ELSE)
                            {
                                if (!stream.TryPeekByte(out nextB))
                                {
                                    error = Err.EndOfStream;
                                    return false;
                                }

                                List<IOperation> elseOps = new List<IOperation>();
                                while (offset < len && nextB != (byte)OP.EndIf)
                                {
                                    if (!TryRead(stream, elseOps, ref offset, out error))
                                    {
                                        return false;
                                    }
                                    if (!stream.TryPeekByte(out nextB))
                                    {
                                        error = Err.EndOfStream;
                                        return false;
                                    }
                                }
                                ifop.elseOps = elseOps.ToArray();

                                _ = stream.TryReadByte(out currentB);
                            }

                            if (currentB != (byte)OP.EndIf)
                            {
                                error = "Mandatory OP_EndIf is missing.";
                                return false;
                            }
                        }
                        else
                        {
                            error = Err.EndOfStream + " Mandatory OP_EndIf is missing.";
                            return false;
                        }

                        opList.Add(ifop);
                        break;


                    case OP.NotIf:
                        NotIfOp notifOp = new NotIfOp(IsWitness);
                        // Peek at next byte to check if it is OP_EndIf or OP_Else
                        if (!stream.TryPeekByte(out nextB))
                        {
                            error = Err.EndOfStream;
                            return false;
                        }

                        ifOps = new List<IOperation>();
                        while (offset < len && nextB != (byte)OP.EndIf && nextB != (byte)OP.ELSE)
                        {
                            if (!TryRead(stream, ifOps, ref offset, out error))
                            {
                                return false;
                            }
                            if (!stream.TryPeekByte(out nextB))
                            {
                                error = Err.EndOfStream;
                                return false;
                            }
                        }
                        notifOp.mainOps = ifOps.ToArray();

                        if (offset < len)
                        {
                            if (!stream.TryReadByte(out byte currentB))
                            {
                                error = Err.EndOfStream;
                                return false;
                            }
                            offset++;

                            if (currentB == (byte)OP.ELSE)
                            {
                                if (!stream.TryPeekByte(out nextB))
                                {
                                    error = Err.EndOfStream;
                                    return false;
                                }

                                List<IOperation> elseOps = new List<IOperation>();
                                while (offset < len && nextB != (byte)OP.EndIf)
                                {
                                    if (!TryRead(stream, elseOps, ref offset, out error))
                                    {
                                        return false;
                                    }
                                    if (!stream.TryPeekByte(out nextB))
                                    {
                                        error = Err.EndOfStream;
                                        return false;
                                    }
                                }
                                notifOp.elseOps = elseOps.ToArray();

                                _ = stream.TryReadByte(out currentB);
                            }

                            if (currentB != (byte)OP.EndIf)
                            {
                                error = "Mandatory OP_EndIf is missing.";
                                return false;
                            }
                        }
                        else
                        {
                            error = Err.EndOfStream + " Mandatory OP_EndIf is missing.";
                            return false;
                        }

                        opList.Add(notifOp);
                        break;

                    case OP.ELSE:
                        error = "OP_ELSE found without prior OP_IF or OP_NOTIF.";
                        return false;
                    case OP.EndIf:
                        error = "OP_EndIf found without prior OP_IF or OP_NOTIF.";
                        return false;

                    // From OP_0 to OP_16 except OP_Reserved is already handled.

                    case OP.Reserved:
                        opList.Add(new ReservedOp());
                        break;

                    case OP.NOP:
                        opList.Add(new NOPOp());
                        break;
                    case OP.VER:
                        opList.Add(new VEROp());
                        break;

                    // OP.IF and OP.NotIf moved to top
                    // OP.VerIf and OP.VerNotIf moved to top (Invalid tx)
                    // OP.ELSE and OP.EndIf moved to top

                    case OP.VERIFY:
                        opList.Add(new VerifyOp());
                        break;

                    // OP.RETURN is already handled

                    case OP.ToAltStack:
                        opList.Add(new ToAltStackOp());
                        break;
                    case OP.FromAltStack:
                        opList.Add(new FromAltStackOp());
                        break;
                    case OP.DROP2:
                        opList.Add(new DROP2Op());
                        break;
                    case OP.DUP2:
                        opList.Add(new DUP2Op());
                        break;
                    case OP.DUP3:
                        opList.Add(new DUP3Op());
                        break;
                    case OP.OVER2:
                        opList.Add(new OVER2Op());
                        break;
                    case OP.ROT2:
                        opList.Add(new ROT2Op());
                        break;
                    case OP.SWAP2:
                        opList.Add(new SWAP2Op());
                        break;
                    case OP.IfDup:
                        opList.Add(new IfDupOp());
                        break;
                    case OP.DEPTH:
                        opList.Add(new DEPTHOp());
                        break;
                    case OP.DROP:
                        opList.Add(new DROPOp());
                        break;
                    case OP.DUP:
                        opList.Add(new DUPOp());
                        break;
                    case OP.NIP:
                        opList.Add(new NIPOp());
                        break;
                    case OP.OVER:
                        opList.Add(new OVEROp());
                        break;
                    case OP.PICK:
                        opList.Add(new PICKOp());
                        break;
                    case OP.ROLL:
                        opList.Add(new ROLLOp());
                        break;
                    case OP.ROT:
                        opList.Add(new ROTOp());
                        break;
                    case OP.SWAP:
                        opList.Add(new SWAPOp());
                        break;
                    case OP.TUCK:
                        opList.Add(new TUCKOp());
                        break;

                    // OP_ (CAT SubStr LEFT RIGHT SIZE INVERT AND OR XOR) are moved to top

                    case OP.SIZE:
                        opList.Add(new SizeOp());
                        break;

                    case OP.EQUAL:
                        opList.Add(new EqualOp());
                        break;
                    case OP.EqualVerify:
                        opList.Add(new EqualVerifyOp());
                        break;
                    case OP.Reserved1:
                        opList.Add(new Reserved1Op());
                        break;
                    case OP.Reserved2:
                        opList.Add(new Reserved2Op());
                        break;
                    case OP.ADD1:
                        opList.Add(new ADD1Op());
                        break;
                    case OP.SUB1:
                        opList.Add(new SUB1Op());
                        break;

                    // OP.MUL2 and OP.DIV2 are moved to top (disabled op).

                    case OP.NEGATE:
                        opList.Add(new NEGATEOp());
                        break;
                    case OP.ABS:
                        opList.Add(new ABSOp());
                        break;
                    case OP.NOT:
                        opList.Add(new NOTOp());
                        break;
                    case OP.NotEqual0:
                        opList.Add(new NotEqual0Op());
                        break;
                    case OP.ADD:
                        opList.Add(new AddOp());
                        break;
                    case OP.SUB:
                        opList.Add(new SUBOp());
                        break;

                    // OP_ (MUL DIV MOD LSHIFT RSHIFT) are moved to top (disabled op).

                    case OP.BoolAnd:
                        opList.Add(new BoolAndOp());
                        break;
                    case OP.BoolOr:
                        opList.Add(new BoolOrOp());
                        break;
                    case OP.NumEqual:
                        opList.Add(new NumEqualOp());
                        break;
                    case OP.NumEqualVerify:
                        opList.Add(new NumEqualVerifyOp());
                        break;
                    case OP.NumNotEqual:
                        opList.Add(new NumNotEqualOp());
                        break;
                    case OP.LessThan:
                        opList.Add(new LessThanOp());
                        break;
                    case OP.GreaterThan:
                        opList.Add(new GreaterThanOp());
                        break;
                    case OP.LessThanOrEqual:
                        opList.Add(new LessThanOrEqualOp());
                        break;
                    case OP.GreaterThanOrEqual:
                        opList.Add(new GreaterThanOrEqualOp());
                        break;
                    case OP.MIN:
                        opList.Add(new MINOp());
                        break;
                    case OP.MAX:
                        opList.Add(new MAXOp());
                        break;
                    case OP.WITHIN:
                        opList.Add(new WITHINOp());
                        break;

                    case OP.RIPEMD160:
                        opList.Add(new RipeMd160Op());
                        break;
                    case OP.SHA1:
                        opList.Add(new Sha1Op());
                        break;
                    case OP.SHA256:
                        opList.Add(new Sha256Op());
                        break;
                    case OP.HASH160:
                        opList.Add(new Hash160Op());
                        break;
                    case OP.HASH256:
                        opList.Add(new Hash256Op());
                        break;
                    // TODO: Add following case
                    //case OP.CodeSeparator:
                    //    break;
                    case OP.CheckSig:
                        opList.Add(new CheckSigOp());
                        break;
                    case OP.CheckSigVerify:
                        opList.Add(new CheckSigVerifyOp());
                        break;
                    case OP.CheckMultiSig:
                        opList.Add(new CheckMultiSigOp());
                        break;
                    case OP.CheckMultiSigVerify:
                        opList.Add(new CheckMultiSigVerifyOp());
                        break;
                    case OP.NOP1:
                        opList.Add(new NOP1Op());
                        break;
                    case OP.CheckLocktimeVerify:
                        opList.Add(new CheckLocktimeVerifyOp());
                        break;
                    case OP.CheckSequenceVerify:
                        opList.Add(new CheckSequenceVerifyOp());
                        break;
                    case OP.NOP4:
                        opList.Add(new NOP4Op());
                        break;
                    case OP.NOP5:
                        opList.Add(new NOP5Op());
                        break;
                    case OP.NOP6:
                        opList.Add(new NOP6Op());
                        break;
                    case OP.NOP7:
                        opList.Add(new NOP7Op());
                        break;
                    case OP.NOP8:
                        opList.Add(new NOP8Op());
                        break;
                    case OP.NOP9:
                        opList.Add(new NOP9Op());
                        break;
                    case OP.NOP10:
                        opList.Add(new NOP10Op());
                        break;

                    default:
                        error = "Undefined OP code";
                        return false;
                }
            }

            error = null;
            return true;
        }


        private string GetOpName(OP val)
        {
            return $"OP_{val.ToString()}";
        }

    }
}
