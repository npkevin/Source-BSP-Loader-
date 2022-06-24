using System;

namespace VALVE.Formats.BSP
{
    enum SURFACE_EDGE_DIRECTION
    {
        FWD,
        REV,
    }

    struct SurfaceEdge : ILump
    {
        public int bytes { get => 4; }

        private int value;

        public int index
        {
            get => value < 0 ? -value : value;
            private set => this.value = value;
        }

        public SURFACE_EDGE_DIRECTION direction
        {
            get => value < 0 ? SURFACE_EDGE_DIRECTION.FWD : SURFACE_EDGE_DIRECTION.REV;
        }

        public SurfaceEdge(byte[] data)
        {
            value = BitConverter.ToInt32(data, 0);
        }
    }
}
