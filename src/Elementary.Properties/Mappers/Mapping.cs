using System;
using System.Reflection;

namespace Elementary.Properties.Test.Mappers
{
    public class Mapping
    {
        public Mapping(MethodInfo sourceGetter, MethodInfo destinationSetter)
        {
            this.SourceGetter = sourceGetter;
            this.DestinationSetter = destinationSetter;
            this.SetDestination = null;
        }

        public Mapping(MethodInfo methodInfo, Action<object, object> setDestinationProperty)
        {
            this.SourceGetter = methodInfo;
            this.DestinationSetter = null;
            this.SetDestination = setDestinationProperty;
        }

        public MethodInfo SourceGetter { get; }

        public MethodInfo DestinationSetter { get; }

        public Action<object, object> SetDestination { get; }
    }
}