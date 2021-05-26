// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// OP code values used in <see cref="IScript"/>s as <see cref="Scripts.Operations.IOperation"/>s.
    /// </summary>
    /// <remarks>
    /// https://en.bitcoin.it/wiki/Script 
    /// https://github.com/bitcoin/bitcoin/blob/a822a0e4f6317f98cde6f0d5abe952b4e8992ac9/src/script/interpreter.h
    /// https://github.com/bitcoin/bitcoin/blob/a822a0e4f6317f98cde6f0d5abe952b4e8992ac9/src/script/interpreter.cpp
    /// </remarks>
    public enum OP : byte
    {
        # region  Constants 

        /// <summary>
        /// Push an emtpy array of bytes onto the stack. Also known as OP_FALSE.
        /// </summary>
        _0 = 0x00,

        // * From 0x01 to 0x4b don't have names. They indicate size of the following data to be pushed.

        /// <summary>
        /// Next 1 byte indicates size of the data to be pushed.
        /// </summary>
        PushData1 = 0x4c,
        /// <summary>
        /// Next 2 bytes indicate size of the data to be pushed.
        /// </summary>
        PushData2 = 0x4d,
        /// <summary>
        /// Next 4 bytes indicate size of the data to be pushed.
        /// </summary>
        PushData4 = 0x4e,
        /// <summary>
        /// Push number -1 to the stack.
        /// </summary>
        Negative1 = 0x4f,
        /// <summary>
        /// Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch.
        /// </summary>
        Reserved = 0x50,
        /// <summary>
        /// Push number 1 to the stack. Also known as OP_TRUE.
        /// </summary>
        _1 = 0x51,
        /// <summary>
        /// Push number 2 to the stack.
        /// </summary>
        _2 = 0x52,
        /// <summary>
        /// Push number 3 to the stack.
        /// </summary>
        _3 = 0x53,
        /// <summary>
        /// Push number 4 to the stack.
        /// </summary>
        _4 = 0x54,
        /// <summary>
        /// Push number 5 to the stack.
        /// </summary>
        _5 = 0x55,
        /// <summary>
        /// Push number 6 to the stack.
        /// </summary>
        _6 = 0x56,
        /// <summary>
        /// Push number 7 to the stack.
        /// </summary>
        _7 = 0x57,
        /// <summary>
        /// Push number 8 to the stack.
        /// </summary>
        _8 = 0x58,
        /// <summary>
        /// Push number 9 to the stack.
        /// </summary>
        _9 = 0x59,
        /// <summary>
        /// Push number 10 to the stack.
        /// </summary>
        _10 = 0x5a,
        /// <summary>
        /// Push number 11 to the stack.
        /// </summary>
        _11 = 0x5b,
        /// <summary>
        /// Push number 12 to the stack.
        /// </summary>
        _12 = 0x5c,
        /// <summary>
        /// Push number 13 to the stack.
        /// </summary>
        _13 = 0x5d,
        /// <summary>
        /// Push number 14 to the stack.
        /// </summary>
        _14 = 0x5e,
        /// <summary>
        /// Push number 15 to the stack.
        /// </summary>
        _15 = 0x5f,
        /// <summary>
        /// Push number 16 to the stack.
        /// </summary>
        _16 = 0x60,

        #endregion



        #region Flow control

        /// <summary>
        /// Does nothing!
        /// </summary>
        NOP = 0x61,
        /// <summary>
        /// Transaction is invalid unless occuring in an unexecuted OP_IF branch.
        /// </summary>
        VER = 0x62,
        /// <summary>
        /// Marks the beginning of a conditional statement. Removes the top stack item, if the value is True,
        /// the statements are executed, otherwise the statements under optional <see cref="ELSE"/> are executed.
        /// <para/>Format (must end with <see cref="EndIf"/>): [expression] if [statements] [else [statements]]* endif
        /// </summary>
        IF = 0x63,
        /// <summary>
        /// Marks the beginning of a conditional statement. Removes the top stack item, if the value is False, 
        /// the statements are executed, otherwise the statements under optional <see cref="ELSE"/> are executed.
        /// <para/>Format (must end with <see cref="EndIf"/>): [expression] notif [statements] [else [statements]]* endif
        /// </summary>
        NotIf = 0x64,
        /// <summary>
        /// Transaction is invalid even when occuring in an unexecuted OP_IF branch
        /// </summary>
        VerIf = 0x65,
        /// <summary>
        /// Transaction is invalid even when occuring in an unexecuted OP_IF branch
        /// </summary>
        VerNotIf = 0x66,
        /// <summary>
        /// Marks the "else" part of the conditional statement. Can only exist after an <see cref="IF"/> or <see cref="NotIf"/>.
        /// The statements will only be executed if the preceding statements weren't executed.
        /// <para/>Format: [expression] if [statements] [else [statements]]* endif
        /// </summary>
        ELSE = 0x67,
        /// <summary>
        /// Marks end of a conditional block. All blocks must end or the script is invalid. 
        /// It also can't exist without a prior <see cref="IF"/> or <see cref="NotIf"/>.
        /// <para/>Format: [expression] if [statements] [else [statements]]* endif
        /// </summary>
        EndIf = 0x68,
        /// <summary>
        /// Removes the top stack item and fails if it was False.
        /// </summary>
        VERIFY = 0x69,
        /// <summary>
        /// Creates an unspendable output. Used for attaching extra data to transactions. 
        /// </summary>
        RETURN = 0x6a,

        #endregion



        #region Stack

        /// <summary>
        /// Removes one item from stack and puts it on top of alt-stack.
        /// </summary>
        ToAltStack = 0x6b,
        /// <summary>
        /// Removes one item from alt-stack and puts it on top of stack.
        /// </summary>
        FromAltStack = 0x6c,
        /// <summary>
        /// Removes the top two stack items.
        /// </summary>
        DROP2 = 0x6d,
        /// <summary>
        /// Duplicates the top 2 stack items.
        /// </summary>
        DUP2 = 0x6e,
        /// <summary>
        /// Duplicates the top 3 stack items.
        /// </summary>
        DUP3 = 0x6f,
        /// <summary>
        /// Copies the pair of items two spaces back in the stack to the front.
        /// <para/> Example: x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2
        /// </summary>
        OVER2 = 0x70,
        /// <summary>
        /// The fifth and sixth items back are moved to the top of the stack.
        /// <para/> Example: x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2
        /// </summary>
        ROT2 = 0x71,
        /// <summary>
        /// Swaps the top two pairs of items.
        /// <para/> Example: x1 x2 x3 x4 -> x3 x4 x1 x2
        /// </summary>
        SWAP2 = 0x72,
        /// <summary>
        /// Duplicates top stack item if its value is not 0.
        /// </summary>
        IfDup = 0x73,
        /// <summary>
        /// Puts the number of stack items onto the stack.
        /// </summary>
        DEPTH = 0x74,
        /// <summary>
        /// Removes the top stack item.
        /// </summary>
        DROP = 0x75,
        /// <summary>
        /// Duplicates the top stack item.
        /// </summary>
        DUP = 0x76,
        /// <summary>
        /// Removes the second item from top of stack.
        /// <para/> Example: x1 x2 -> x2
        /// </summary>
        NIP = 0x77,
        /// <summary>
        /// Copies the second item from top of the stack to the top.
        /// <para/> Example: x1 x2 -> x1 x2 x1
        /// </summary>
        OVER = 0x78,
        /// <summary>
        /// The item n back in the stack is "copied" to the top.
        /// <para/> Example: xn ... x2 x1 x0 -> xn ... x2 x1 x0 xn
        /// </summary>
        PICK = 0x79,
        /// <summary>
        /// The item n back in the stack is "moved" to the top.
        /// <para/> Example: xn x(n-1) ... x2 x1 x0 n -> x(n-1) ... x2 x1 x0 xn
        /// </summary>
        ROLL = 0x7a,
        /// <summary>
        /// The top three items on the stack are rotated to the left by one place.
        /// <para/> Example: x1 x2 x3 -> x2 x3 x1
        /// </summary>
        ROT = 0x7b,
        /// <summary>
        /// The top two items on the stack are swapped.
        /// <para/> Example: x1 x2 -> x2 x1
        /// </summary>
        SWAP = 0x7c,
        /// <summary>
        /// The item at the top of the stack is copied and inserted before the second-to-top item.
        /// <para/> Example: x1 x2 -> x2 x1 x2
        /// </summary>
        TUCK = 0x7d,

        #endregion



        #region Splice

        /// <summary>
        /// [Disabled] Concatenates two strings.
        /// </summary>
        CAT = 0x7e,
        /// <summary>
        /// [Disabled] Returns a section of a string.
        /// </summary>
        SubStr = 0x7f,
        /// <summary>
        /// [Disabled] Keeps only characters left of the specified point in a string.
        /// </summary>
        LEFT = 0x80,
        /// <summary>
        /// [Disabled] Keeps only characters right of the specified point in a string.
        /// </summary>
        RIGHT = 0x81,
        /// <summary>
        /// Pushes the string length of the top element of the stack (without popping it).
        /// </summary>
        SIZE = 0x82,

        #endregion



        #region Bitwise logic

        /// <summary>
        /// [Disabled] Flips all of the bits in the input.
        /// </summary>
        INVERT = 0x83,
        /// <summary>
        /// [Disabled] Boolean and between each bit in the inputs.
        /// </summary>
        AND = 0x84,
        /// <summary>
        /// [Disabled] Boolean or between each bit in the inputs.
        /// </summary>
        OR = 0x85,
        /// <summary>
        /// [Disabled] Boolean exclusive or between each bit in the inputs. 
        /// </summary>
        XOR = 0x86,
        /// <summary>
        /// Pops two top stack items, compares them and pushes the equality result onto the stack. 
        /// 1 if the inputs are exactly equal, 0 otherwise.
        /// </summary>
        EQUAL = 0x87,
        /// <summary>
        /// Runs <see cref="EQUAL"/> then <see cref="VERIFY"/> respectively.
        /// </summary>
        EqualVerify = 0x88,
        /// <summary>
        /// Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch
        /// </summary>
        Reserved1 = 0x89,
        /// <summary>
        /// Reserved OP code. Transaction is invalid unless occuring in an unexecuted OP_IF branch
        /// </summary>
        Reserved2 = 0x8a,

        #endregion



        #region Arithmetic

        /// <summary>
        /// 1 is added to the top stack item converted to an integer.
        /// </summary>
        ADD1 = 0x8b,
        /// <summary>
        /// 1 is subtracted from the top stack item converted to an integer.
        /// </summary>
        SUB1 = 0x8c,
        /// <summary>
        /// [Disabled] The top stack item converted to an integer is multiplied by 2.
        /// </summary>
        MUL2 = 0x8d,
        /// <summary>
        /// [Disabled] The top stack item converted to an integer is divided by 2.
        /// </summary>
        DIV2 = 0x8e,
        /// <summary>
        /// The sign of the top stack item converted to an integer is flipped.
        /// </summary>
        NEGATE = 0x8f,
        /// <summary>
        /// The top stack item converted to an integer is made positive.
        /// </summary>
        ABS = 0x90,
        /// <summary>
        /// If the top stack item converted to an integer is 0 or 1, it is flipped. Otherwise the output will be 0.
        /// </summary>
        NOT = 0x91,
        /// <summary>
        /// Returns 0 if the top stack item converted to an integer is 0. 1 otherwise.
        /// </summary>
        NotEqual0 = 0x92,
        /// <summary>
        /// Adds two top stack items converted to integers (a b -> [a+b]).
        /// </summary>
        ADD = 0x93,
        /// <summary>
        /// Subtracts two top stack items converted to integers (a b -> [a-b]).
        /// </summary>
        SUB = 0x94,
        /// <summary>
        /// [Disabled] a is multiplied by b (a b -> [a*b]).
        /// </summary>
        MUL = 0x95,
        /// <summary>
        /// [Disabled] a is divided by b (a b -> [a/b]).
        /// </summary>
        DIV = 0x96,
        /// <summary>
        /// [Disabled] Returns the remainder after dividing a by b. 
        /// </summary>
        MOD = 0x97,
        /// <summary>
        /// [Disabled] Shifts a left b bits, preserving sign.
        /// </summary>
        LSHIFT = 0x98,
        /// <summary>
        /// [Disabled] Shifts a right b bits, preserving sign.
        /// </summary>
        RSHIFT = 0x99,
        /// <summary>
        /// If both a and b are not 0, the output is 1. Otherwise 0.
        /// </summary>
        BoolAnd = 0x9a,
        /// <summary>
        /// If a or b is not 0, the output is 1. Otherwise 0.
        /// </summary>
        BoolOr = 0x9b,
        /// <summary>
        /// Returns 1 if the numbers are equal, 0 otherwise.
        /// </summary>
        NumEqual = 0x9c,
        /// <summary>
        /// Runs <see cref="NumEqual"/> then <see cref="VERIFY"/>.
        /// </summary>
        NumEqualVerify = 0x9d,
        /// <summary>
        /// Returns 1 if the numbers are not equal, 0 otherwise.
        /// </summary>
        NumNotEqual = 0x9e,
        /// <summary>
        /// Returns 1 if a is less than b, 0 otherwise.
        /// </summary>
        LessThan = 0x9f,
        /// <summary>
        /// Returns 1 if a is greater than b, 0 otherwise.
        /// </summary>
        GreaterThan = 0xa0,
        /// <summary>
        /// Returns 1 if a is less than or equal to b, 0 otherwise.
        /// </summary>
        LessThanOrEqual = 0xa1,
        /// <summary>
        /// Returns 1 if a is greater than or equal to b, 0 otherwise.
        /// </summary>
        GreaterThanOrEqual = 0xa2,
        /// <summary>
        /// Returns the smaller of a and b.
        /// </summary>
        MIN = 0xa3,
        /// <summary>
        /// Returns the larger of a and b.
        /// </summary>
        MAX = 0xa4,
        /// <summary>
        /// Top 3 stack items are removed and converted to integers (x, min, max). 
        /// If (min &#60;&#61; x &#60; max) push 1 to the stack, otherwise push 0.
        /// </summary>
        WITHIN = 0xa5,

        #endregion



        #region Cryptography

        /// <summary>
        /// The top stack item is removed and the RIPEMD-160 hash result is pushed to the stack.
        /// </summary>
        RIPEMD160 = 0xa6,
        /// <summary>
        /// The top stack item is removed and the SHA-1 hash result is pushed to the stack.
        /// </summary>
        SHA1 = 0xa7,
        /// <summary>
        /// The top stack item is removed and the SHA256 hash result is pushed to the stack.
        /// </summary>
        SHA256 = 0xa8,
        /// <summary>
        /// The top stack item is removed and the RIPEMD-160 of SHA-256 hash result is pushed to the stack.
        /// </summary>
        HASH160 = 0xa9,
        /// <summary>
        /// The top stack item is removed and the double SHA-256 hash result is pushed to the stack.
        /// </summary>
        HASH256 = 0xaa,
        /// <summary>
        /// All of the signature checking words will only match signatures to the data after the most 
        /// recently-executed OP_CODESEPARATOR.
        /// </summary>
        CodeSeparator = 0xab,
        /// <summary>
        /// Removes two top stack items and use them as an ECDSA signature and a public key respectively 
        /// in the signature verification process. The result of the verification is pushed to the stack as true/false.
        /// </summary>
        CheckSig = 0xac,
        /// <summary>
        /// Runs <see cref="CheckSig"/> first then <see cref="VERIFY"/>.
        /// </summary>
        CheckSigVerify = 0xad,
        /// <summary>
        /// Removes multiple items from the stack and use them as many ECDSA signatures and public keys
        /// in multiple signature verification process. The final result of the verifications is pushed to the stack as true/false.
        /// <para/> Due to a bug in bitcoin-core implementation of scripts, one extra unused value is 
        /// removed from the stack in the end.
        /// </summary>
        CheckMultiSig = 0xae,
        /// <summary>
        /// Runs <see cref="CheckMultiSig"/> then <see cref="VERIFY"/>.
        /// </summary>
        CheckMultiSigVerify = 0xaf,

        #endregion


        #region New/future OP codes

        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP1 = 0xb0,
        /// <summary>
        /// Marks transaction as invalid if the top stack item is greater than the transaction's nLockTime field.
        /// Previously known as NOP2.
        /// </summary>
        CheckLocktimeVerify = 0xb1,
        /// <summary>
        /// Marks transaction as invalid if the relative lock time of the input is not equal to or longer than 
        /// the value of the top stack item.
        /// Previously known as NOP3
        /// </summary>
        CheckSequenceVerify = 0xb2,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP4 = 0xb3,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP5 = 0xb4,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP6 = 0xb5,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP7 = 0xb6,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP8 = 0xb7,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP9 = 0xb8,
        /// <summary>
        /// OP is ignored.
        /// </summary>
        NOP10 = 0xb9,

        #endregion

        /// <summary>
        /// Removes 3 items from the stack as a signature, a number and a public key and performs the signature
        /// verification process defined by BIP-342
        /// </summary>
        CheckSigAdd = 0xba,

        // Note: 0xbb to 0xff are not defined.

    }
}
