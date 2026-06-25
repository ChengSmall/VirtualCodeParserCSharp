

namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 定小端内存布局功能
    /// </summary>
    public static unsafe class OrderMemoryExtend
    {

        #region 内存转值


        public static ushort OrderToInt16(byte* buffer)
        {
            return (ushort)((uint)buffer[0] |
                (((uint)buffer[1]) << 8));
        }


        public static uint OrderToInt32(byte* buffer)
        {
            return ((uint)buffer[0] |
                (((uint)buffer[1]) << 8) |
                (((uint)buffer[2]) << (2 * 8)) |
                (((uint)buffer[3]) << (3 * 8)));
        }


        public static ulong OrderToInt64(byte* buffer)
        {
            return ((ulong)buffer[0] |
                (((ulong)buffer[1]) << 8) |
                (((ulong)buffer[2]) << (2 * 8)) |
                (((ulong)buffer[3]) << (3 * 8)) |
                (((ulong)buffer[4]) << (4 * 8)) |
                (((ulong)buffer[5]) << (5 * 8)) |
                (((ulong)buffer[6]) << (6 * 8)) |
                (((ulong)buffer[7]) << (7 * 8))
                );
        }


        public static float OrderToFloat(byte* buffer)
        {
            var re = OrderToInt32(buffer);
            return *((float*)&re);
        }

        public static double OrderToDouble(byte* buffer)
        {
            var re = OrderToInt64(buffer);
            return *((double*)&re);
        }

        #endregion

        #region 值转内存


        public static void OrderInt16ToBytes(this ushort value, byte* buffer)
        {
            buffer[0] = (byte)((value) & (0xFF));
            buffer[1] = (byte)((value >> 8) & (0xFF));
        }


        public static void OrderInt32ToBytes(this uint value, byte* buffer)
        {
            buffer[0] = (byte)((value) & (0xFF));
            buffer[1] = (byte)((value >> 8) & (0xFF));
            buffer[2] = (byte)((value >> (8 * 2)) & (0xFF));
            buffer[3] = (byte)((value >> (8 * 3)) & (0xFF));
        }


        public static void OrderInt64ToBytes(this ulong value, byte* buffer)
        {
            for (int i = 0; i < 8; i++)
            {
                buffer[i] = (byte)((value >> (8 * i)) & (0xFF));
            }
        }


        #endregion

    }

}
