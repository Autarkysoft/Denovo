// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Denovo.MVVM
{
    /// <summary>
    /// Base (abstract) class implementing <see cref="INotifyPropertyChanged"/>. 
    /// Could be used for both ViewModels and Models.
    /// </summary>
    public abstract class InpcBase : INotifyPropertyChanged
    {
        public InpcBase()
        {
            PropertyDependencyMap = new Dictionary<string, List<string>>();

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                foreach (DependsOnPropertyAttribute dependsAttr in property.GetCustomAttributes<DependsOnPropertyAttribute>())
                {
                    if (dependsAttr == null)
                    {
                        continue;
                    }

                    foreach (string dependence in dependsAttr.DependentProps)
                    {
                        if (!PropertyDependencyMap.ContainsKey(dependence))
                        {
                            PropertyDependencyMap.Add(dependence, new List<string>());
                        }
                        PropertyDependencyMap[dependence].Add(property.Name);
                    }
                }
            }
        }



        private readonly Dictionary<string, List<string>> PropertyDependencyMap;

        /// <summary>
        /// The PropertyChanged Event to raise to any UI object
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event using the given property name.
        /// The event is only invoked if data binding is used
        /// </summary>
        /// <param name="propertyName">The Name of the property that is changing.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));

                // Also raise the PropertyChanged event for dependant properties.
                if (PropertyDependencyMap.ContainsKey(propertyName))
                {
                    foreach (string p in PropertyDependencyMap[propertyName])
                    {
                        handler(this, new PropertyChangedEventArgs(p));
                    }
                }
            }
        }


        /// <summary>
        /// Sets the value of a property and raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Property's backing field to change</param>
        /// <param name="value">New value to set the <paramref name="field"/> to</param>
        /// <param name="propertyName">
        /// [Default value = null]
        /// The Name of the property that is changing. If it was null, the name is resolved at runtime automatically.
        /// </param>
        /// <returns>Retruns true if the value was changed, false if otherwise.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            else
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
        }
    }
}
