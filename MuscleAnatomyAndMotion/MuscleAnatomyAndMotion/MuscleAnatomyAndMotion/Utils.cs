using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace MuscleAnatomyAndMotion
{
    public class Utils
    {
        public static Derived As<Base, Derived>(Base obj) where Derived : Base
        {
            var instanceDerived = Activator.CreateInstance(typeof(Derived));
            PropertyInfo[] propertiesBase = typeof(Base).GetProperties();

            foreach (var property in propertiesBase)
            {
                property.SetValue(instanceDerived, property.GetValue(obj, null), null);
            }

            return (Derived)instanceDerived;
        }
        public class Grouping<K, T> : ObservableCollection<T>
        {
            public K Name { get; private set; }
            public Grouping(K name, IEnumerable<T> items)
            {
                Name = name;
                foreach (T item in items)
                    Items.Add(item);
            }
        }
    }
}
