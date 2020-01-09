// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Reflection;
using Xunit;

namespace Tests
{
    public static class Helper
    {
        public static void ComparePrivateField<InstanceType, FieldType>(InstanceType instance, string fieldName, FieldType expected)
        {
            FieldInfo fi = typeof(InstanceType).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi is null)
            {
                Assert.True(false, "The private field was not found.");
            }

            object fieldVal = fi.GetValue(instance);
            if (fieldVal is null)
            {
                Assert.True(false, "The private field value was null.");
            }
            else if (fieldVal is FieldType actual)
            {
                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.True(false, $"Field value is not the same type as expected.{Environment.NewLine}" +
                    $"Actual type: {fieldVal.GetType()}{Environment.NewLine}" +
                    $"Expected type: {expected.GetType()}");
            }
        }

    }
}
