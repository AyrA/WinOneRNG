using System;

namespace rnd
{
    public class ByteBuffer
    {
        private byte[] Buffer;
        private int Position = 0;

        public ByteBuffer(int Size)
        {
            Buffer = new byte[Size];
        }

        public void Add(byte[] Data, int Index, int Count)
        {
            for (int i = 0; i < Count; i++)
            {
                SetByte(Data[Index + i]);
            }
        }

        public void Add(byte[] Data)
        {
            Add(Data, 0, Data.Length);
        }

        public byte[] Read(int Count)
        {
            byte[] Data = new byte[Count];
            Read(Data, 0, Count);
            return Data;
        }

        public void Read(byte[] Data, int Index, int Count)
        {
            for (int i = 0; i < Count; i++)
            {
                Data[Index + i] = GetByte();
            }
        }

        public byte GetByte()
        {
            return Buffer[Position = (Position + 1) % Buffer.Length];
        }

        public byte SetByte(byte Data)
        {
            return Buffer[Position = (Position + 1) % Buffer.Length] = Data;
        }
    }
}
