namespace TROIKA.Formats.TTH
{
    struct Header
    {
        public readonly byte[] data;

        public Header(byte[] data)
        {
            this.data = data;
        }
    }
}
