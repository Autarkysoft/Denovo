// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Protocol Version and User Agent
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0014.mediawiki
    /// </summary>
    public class BIP0014
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BIP0014"/> using given parameters.
        /// </summary>
        /// <remarks>
        /// Note that we consider both client name and version to be mandatory and comment is the only optional part.
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="name">Client name.</param>
        /// <param name="ver">
        /// Client version, if null then version will be set to 0.0, recommended form is Major.Minor.Revision.
        /// </param>
        /// <param name="comment">
        /// [Default value = null]
        /// Additioanl comment (it is best to separate each piece of information with a semicolon).
        /// </param>
        public BIP0014(string name, Version ver, string comment = null)
        {
            ClientName = name;
            ClientVersion = ver;
            Comment = comment;
        }



        private const string ReservedChars = "/:()";
        private const byte Separator = (byte)'/';

        private string _name;
        /// <summary>
        /// Name of the client
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        public string ClientName
        {
            get => _name;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(ClientName), "Client name can not be null or empty.");
                if (value.Any(c => ReservedChars.Contains(c)))
                    throw new FormatException($"Client name can not contain reserverd characters ({ReservedChars})");

                _name = value;
            }
        }

        private Version _ver;
        /// <summary>
        /// Version of the client
        /// </summary>
        public Version ClientVersion
        {
            get => _ver;
            set => _ver = value ?? new Version(0, 0);
        }

        private string _cmt;
        /// <summary>
        /// Additional optional comments
        /// </summary>
        /// <exception cref="FormatException"/>
        public string Comment
        {
            get => _cmt;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Any(c => ReservedChars.Contains(c)))
                    throw new FormatException($"Comment can not contain reserverd characters ({ReservedChars})");

                _cmt = value;
            }
        }



        /// <summary>
        /// Returns byte array representation of this instance.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        /// <summary>
        /// Returns byte array representation of multiple instances of <see cref="BIP0014"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="bips"><see cref="BIP0014"/> instances to use.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArrayMulti(BIP0014[] bips)
        {
            if (bips == null || bips.Any(b => b == null))
                throw new ArgumentNullException(nameof(bips), "Input can not be null!");


            // 20 is the approximate byte size of each BIP-14 object
            FastStream stream = new FastStream(bips.Length * 20);
            stream.Write(Separator);
            foreach (var item in bips)
            {
                stream.Write(item.GetBytes());
                stream.Write(Separator);
            }
            return stream.ToByteArray();
        }

        /// <summary>
        /// Convert the given string to <see cref="BIP0014"/> instances (can contain multiple).
        /// The return value indicates success.
        /// </summary>
        /// <param name="bip14EncodedString">String containing the value(s) to convert.</param>
        /// <param name="result">If the conversion succeeds, this will contain results. Otherwise it will be null.</param>
        /// <returns>True if Conversion succeeds, flase otherwise.</returns>
        public static bool TryParse(string bip14EncodedString, out BIP0014[] result)
        {
            if (string.IsNullOrWhiteSpace(bip14EncodedString) ||
                bip14EncodedString[0] != (char)Separator || bip14EncodedString[^1] != (char)Separator)
            {
                result = null;
                return false;
            }

            string[] parts = bip14EncodedString.Split(new char[] { (char)Separator }, StringSplitOptions.RemoveEmptyEntries);

            result = new BIP0014[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                int sepIndex = parts[i].IndexOf(':');
                // -1 means doesn't have ':' and 0 means it doesn't have client name
                if (sepIndex < 1)
                {
                    result = null;
                    return false;
                }

                int cmtStart = parts[i].IndexOf('(');
                int cmtEnd = parts[i].IndexOf(')');
                if (cmtStart > 0 && cmtEnd < 0 || cmtStart < 0 && cmtEnd > 0)
                {
                    result = null;
                    return false;
                }
                // i1 and i2 have the same sign due to previous if
                if (cmtStart > 0 && (cmtEnd < cmtStart || cmtStart < sepIndex || cmtEnd < sepIndex ||
                    parts[i].Count(c => c == '(') != 1 || parts[i].Count(c => c == ')') != 1))
                {
                    result = null;
                    return false;
                }

                string name = parts[i].Substring(0, sepIndex);
                string ver = (cmtStart != -1) ?
                    parts[i].Substring(sepIndex + 1, cmtStart - sepIndex - 1) :
                    parts[i].Substring(sepIndex + 1);
                string comment = (cmtStart != -1) ?
                    parts[i].Substring(cmtStart + 1, cmtEnd - cmtStart - 1) :
                    null;

                try
                {
                    result[i] = new BIP0014(name, new Version(ver), comment);
                }
                catch (Exception) // Catch Version ctor exception
                {
                    result = null;
                    return false;
                }
            }

            return true;
        }


        private byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(GetString());
        }
        private string GetString()
        {
            // Name:Version(comment)
            return $"{ClientName}:{ClientVersion.ToString()}{(string.IsNullOrWhiteSpace(Comment) ? "" : $"({Comment})")}";
        }

        /// <summary>
        /// Returns string represntation of this instance in the following format:
        /// <para/>/Name:Version(comment)/
        /// </summary>
        /// <returns>String result</returns>
        public override string ToString()
        {
            return $"/{GetString()}/";
        }
    }
}
