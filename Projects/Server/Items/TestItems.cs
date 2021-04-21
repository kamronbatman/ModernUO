namespace Server.Items
{
    [Serializable(2)]
    public partial class TestItem1 : Item
    {
        [CommandProperty(AccessLevel.Administrator)]
        [SerializableField(1)]
        private int _someProperty;

        // public void MigrateFrom(SomeItemV1Content v1Content)
        // {
        //     _someProperty = int.Parse(v1Content.SomeProperty);
        // }
    }
}
