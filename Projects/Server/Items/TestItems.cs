using System;

namespace Server.Items
{
    [Serializable(1)]
    public partial class TestItem1 : Item
    {
        [SerializableField(1)]
        private int _someProperty;

        public void Configure()
        {
            _someProperty = 1;
            Console.WriteLine(_someProperty);
        }
    }
}
