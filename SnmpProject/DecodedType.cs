namespace SnmpProject
{
    public enum VisibilitClass
    {
        Universal,
        Application,
        ContextSpecific,
        Private
    }

    public class DecodedType
    {
        public int TypeTagId { get; set; }
        public VisibilitClass Visibility { get; set; }
        public byte[] Data { get; set; }
        public long Length { get; set; }
        public bool IsConstructed { get; set; }
    }
}
