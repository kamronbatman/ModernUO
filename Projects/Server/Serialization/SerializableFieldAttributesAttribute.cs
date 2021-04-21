using System;

namespace Server
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializableFieldAttributesAttribute : Attribute
    {
        public string[] Attributes { get; }

        public SerializableFieldAttributesAttribute(params string[] attributes) => Attributes = attributes;
    }
}
