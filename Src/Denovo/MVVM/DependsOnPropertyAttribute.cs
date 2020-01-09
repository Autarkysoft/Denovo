// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Denovo.MVVM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependsOnPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DependsOnPropertyAttribute"/> using depending properties names.
        /// </summary>
        /// <param name="dependingPropertyNames">Names of the properties that the property with this attribute depends on.</param>
        public DependsOnPropertyAttribute(params string[] dependingPropertyNames)
        {
            DependentProps = dependingPropertyNames;
        }

        /// <summary>
        /// Names of all the properties that the property with this attribute depends on.
        /// </summary>
        public readonly string[] DependentProps;
    }
}
