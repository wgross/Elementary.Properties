using System;
using System.Collections.Generic;

namespace Elementary.Properties.PropertyBags
{
    public class ReflectedPropertyBag<T> : PropertyBagBase<T>
        where T : class
    {
        public ReflectedPropertyBag(T instance)
        {
            this.Instance = instance;
        }
    }
}