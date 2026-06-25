using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 虚拟可执行程序代码
    /// </summary>
    public unsafe sealed class ProgramCode
    {

        #region 构造

        /// <summary>
        /// 实例化虚拟程序代码，指定内存大小
        /// </summary>
        /// <param name="size">程序内存大小</param>
        /// <exception cref="ArgumentOutOfRangeException">参数小于0</exception>
        public ProgramCode(int size)
        {
            if (size < 0) throw new ArgumentOutOfRangeException();
            p_buffer = new byte[size];
        }

        /// <summary>
        /// 使用字节数组实例化虚拟程序代码
        /// </summary>
        /// <param name="code">可执行虚拟程序代码</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        public ProgramCode(byte[] code)
        {
            if (code is null) throw new ArgumentNullException();
            p_buffer = code;
        }

        /// <summary>
        /// 使用流实例化虚拟程序代码
        /// </summary>
        /// <param name="code">要从中读取代码的流</param>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        /// <exception cref="NotSupportedException">没有读取权限</exception>
        public ProgramCode(Stream code)
        {

            if (code is null) throw new ArgumentNullException();

            if (!code.CanRead) throw new NotSupportedException();

            MemoryStream ms;

            const int count = 1024 * 4;

            if (code.CanSeek)
            {
                var length = code.Length;
                ms = new MemoryStream(length < int.MaxValue ? (int)length : int.MaxValue);
            }
            else
            {
                ms = new MemoryStream(count);
            }

            byte[] buf = new byte[count];

            copyTo(code, ms, buf, count);
            
            p_buffer = ms.ToArray();

        }

        static void copyTo(Stream stream, Stream to, byte[] buffer, int length)
        {
            int ri;

            Loop:
            ri = stream.Read(buffer, 0, length);

            if (ri == 0) return;

            to.Write(buffer, 0, ri);
            goto Loop;
        }

        #endregion

        #region 参数

        private byte[] p_buffer;

        #endregion

        #region 功能

        #region 访问

        /// <summary>
        /// 获取或设置程序代码的内存块
        /// </summary>
        /// <exception cref="ArgumentNullException">参数设为null</exception>
        public byte[] Memory
        {
            get => p_buffer;
            set
            {
                if(value is null)
                {
                    throw new ArgumentNullException();
                }
                p_buffer = value;
            }
        }

        #endregion

        /// <summary>
        /// 获取指定偏移下的内存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset">字节偏移量</param>
        /// <returns>获取到的内存，以<typeparamref name="T"/>类型的方式读取</returns>
        /// <exception cref="ArgumentOutOfRangeException">偏移量超出内存块范围</exception>
        public T GetMemory<T>(int offset) where T : unmanaged
        {

            if (offset < 0 || offset + sizeof(T) > p_buffer.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* bp = p_buffer)
            {
                return *((T*)(bp + offset));
            }
        }

        /// <summary>
        /// 在指定偏移下写入内存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset">起始偏移</param>
        /// <param name="value">要写入的内存</param>
        /// <exception cref="ArgumentOutOfRangeException">偏移量超出内存块范围</exception>
        public void SetMemory<T>(int offset, T value) where T : unmanaged
        {

            if (offset < 0 || offset + sizeof(T) > p_buffer.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* bp = p_buffer)
            {
                *((T*)(bp + offset)) = value;
            }

        }

        #endregion

    }

}
