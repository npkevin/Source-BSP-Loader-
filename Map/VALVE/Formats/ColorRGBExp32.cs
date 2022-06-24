namespace VALVE.Formats
{
    struct ColorRGBExp32
    {
        public int size { get => 4; }

        public byte r, g, b, exponent;

        public ColorRGBExp32(byte[] data)
        {
            r = data[0];
            g = data[1];
            b = data[2];
            exponent = data[3];
        }
    }
}
