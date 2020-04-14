// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;

namespace Tests
{
    /// <summary>
    /// These are randomly generated keys. Prv1, Pub1,... Prv2, Pub2,... are from the same key pair
    /// </summary>
    public static class KeyHelper
    {
        internal static PrivateKey Prv1 => new PrivateKey("L28Peud5cQcijrtMthAdUS8FynpM8PKZtnoUZb1VAio9WxKoebHt");
        internal static PublicKey Pub1
        {
            get
            {
                PublicKey.TryRead(Pub1UnCompBytes, out PublicKey result);
                return result;
            }
        }
        internal static string Pub1CompHex => "030b3ad1cea48c61bdcff356675d92010290cdc2e04e1c9e68b6a01d3cec746c17";
        internal static string Pub1UnCompHex => "040b3ad1cea48c61bdcff356675d92010290cdc2e04e1c9e68b6a01d3cec746c17b95aedf5242b50b5c82147697351941032602332d5cc81531eec98a9b8f9c7cd";
        internal static byte[] Pub1CompBytes => Helper.HexToBytes(Pub1CompHex);
        internal static byte[] Pub1UnCompBytes => Helper.HexToBytes(Pub1UnCompHex);
        internal static string Pub1CompHashHex => "2145a147db08e8defb15dbfcb9968971e98b0128";
        internal static string Pub1UnCompHashHex => "b716d8e4a05af9161dcaf0d62ce87475c863ef72";
        internal static byte[] Pub1CompHash => Helper.HexToBytes(Pub1CompHashHex);
        internal static byte[] Pub1UnCompHash => Helper.HexToBytes(Pub1UnCompHashHex);
        internal static string Pub1CompAddr => "142viJrTYHA4TzryiEiuQkYk4Ay5TfpzqW";
        internal static string Pub1UnCompAddr => "1Hh62ZRFLpWT92EKpCbjFc1UgrGDNtfx4i";
        internal static string Pub1BechAddr => "bc1qy9z6z37mpr5da7c4m07tn95fw85ckqfg28wxzd";
        internal static string Pub1BechAddrHex => "2145a147db08e8defb15dbfcb9968971e98b0128";
        internal static string Pub1BechAddrHexUncomp => "b716d8e4a05af9161dcaf0d62ce87475c863ef72";
        internal static string Pub1NestedSegwit => "39vipRmsscHCg3sT7FHfqSUmCoNZroCygq";
        internal static string Pub1NestedSegwitHex => "5a588d0320647b22b6b70ad345dbfbc488380cf0";
        internal static string Pub1NestedSegwitHexUncomp => "958fccf9de503092f161d7432890574aef62a965";



        internal static PrivateKey Prv2 => new PrivateKey("KxWSVSkSv3gGs2AmCF3qRCc6MqAikTL3n4wwMJjsfQikMU61ZQkL");
        internal static PublicKey Pub2
        {
            get
            {
                PublicKey.TryRead(Pub2UnCompBytes, out PublicKey result);
                return result;
            }
        }
        internal static string Pub2CompHex => "036c9e91206e3e3618f45f60a92a2a48670beb46d8d39b69290eec467b521ae591";
        internal static string Pub2UnCompHex => "046c9e91206e3e3618f45f60a92a2a48670beb46d8d39b69290eec467b521ae591059e4f371c885229be97b0b23e8ebab6e603465fb3618b05697d6225142656e5";
        internal static byte[] Pub2CompBytes => Helper.HexToBytes(Pub2CompHex);
        internal static byte[] Pub2UnCompBytes => Helper.HexToBytes(Pub2UnCompHex);
        internal static byte[] Pub2CompHash => Helper.HexToBytes("8f634c80a4e9c9619d4856e94de014c538fadaa3");
        internal static byte[] Pub2UnCompHash => Helper.HexToBytes("95c2a85c042ae21e167df5f3382eaa256dd42ee7");
        internal static string Pub2CompAddr => "1E5AaqVBxLbbAokPA9VpjZNsWtH1hbBfcS";
        internal static string Pub2UnCompAddr => "1Eersdkb2p2jPj4kZ2cEQihgHPr57WWqrC";
        internal static string Pub2BechAddr => "bc1q3a35eq9ya8ykr82g2m55mcq5c5u04k4rvdeav8";
        internal static string Pub2NestedSegwit => "3HuNV2HoYAdUspE7utaXQoTgHvR5Fk33f2";
        internal static string Pub2NestedSegwitHex => "b1d82aac8ec0d0f0841547bdfbce08a80826bc96";
    }
}
