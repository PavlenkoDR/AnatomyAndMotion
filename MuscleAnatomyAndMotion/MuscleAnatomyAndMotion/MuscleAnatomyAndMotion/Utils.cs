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

    public static class ListExtention
    {
        public static List<T> AddSequence<T>(this List<T> list, T info)
        {
            list.Add(info);
            return list;
        }
        public static List<T> AddRangeSequence<T>(this List<T> list, IEnumerable<T> info)
        {
            list.AddRange(info);
            return list;
        }
    }
}
