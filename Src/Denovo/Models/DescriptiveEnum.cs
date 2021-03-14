// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.ComponentModel;
using System.Reflection;

namespace Denovo.Models
{
    public class DescriptiveEnum<T> where T : Enum
    {
        public DescriptiveEnum(T value)
        {
            Value = value;

            FieldInfo fi = value.GetType().GetField(value.ToString());
            object[] attributes = fi?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            Description = (attributes != null && attributes.Length != 0) ?
                                                                ((DescriptionAttribute)attributes[0]).Description :
                                                                value.ToString();
        }

        public string Description { get; set; }
        public T Value { get; set; }
    }
}
