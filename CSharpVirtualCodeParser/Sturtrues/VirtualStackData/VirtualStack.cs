using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;


namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 虚拟可执行堆栈
    /// </summary>
    public unsafe class VirtualStack
    {

        #region 构造

        /// <summary>
        /// 实例化一个虚拟线程堆栈，使用4KB堆栈大小
        /// </summary>
        public VirtualStack()
        {
            p_stack = new byte[1024 * 4];
            p_stackPos = 0;
            p_codePos = 0;
        }
        
        /// <summary>
        /// 实例化一个虚拟线程堆栈
        /// </summary>
        /// <param name="stack">堆栈区内存块</param>
        /// <exception cref="ArgumentNullException"></exception>
        public VirtualStack(byte[] stack)
        {
            if (stack is null) throw new ArgumentNullException();
            p_stack = stack;
            p_stackPos = 0;
        }

        /// <summary>
        /// 实例化一个虚拟线程堆栈
        /// </summary>
        /// <param name="stackMaxSize">堆栈区内存块字节大小</param>
        /// <exception cref="ArgumentOutOfRangeException">参数不大于0</exception>
        public VirtualStack(int stackMaxSize)
        {
            if (stackMaxSize <= 0) throw new ArgumentOutOfRangeException();
            p_stack = new byte[stackMaxSize];
            p_stackPos = 0;
        }

        /// <summary>
        /// 初始化构造
        /// </summary>
        /// <param name="initPar">参数为true则自动初始化基本参数，false则所有的字段全部为默认值</param>
        protected VirtualStack(bool initPar)
        {
            if (initPar)
            {
                p_stack = new byte[1024 * 4];
                p_stackPos = 0;
                p_codePos = 0;
            }
        }

        #endregion

        #region 参数

        /// <summary>
        /// 寄存器集
        /// </summary>
        public Register64 register;

        /// <summary>
        /// 额外缓存值
        /// </summary>
        public RegisterOther otherCache;

        /// <summary>
        /// 可执行堆栈内存
        /// </summary>
        protected internal byte[] p_stack;

        /// <summary>
        /// 栈区指向指针
        /// </summary>
        protected internal uint p_stackPos;

        /// <summary>
        /// 代码指向指针
        /// </summary>
        protected internal uint p_codePos;

        #endregion

        #region 功能

        #region 参数访问

        /// <summary>
        /// 获取该执行堆栈的内存
        /// </summary>
        public byte[] StackBuffer => p_stack;

        /// <summary>
        /// 获取或设置执行堆栈的内存指向指针
        /// </summary>
        public uint StackPosition
        {
            get => p_stackPos;
            set
            {
                p_stackPos = value;
            }
        }

        /// <summary>
        /// 访问或设置指向当前可执行代码内存位置的指针
        /// </summary>
        public uint CodePosition
        {
            get => p_codePos;
            set
            {
                p_codePos = value;
            }
        }

        #endregion

        #region 功能

        /// <summary>
        /// 清空堆栈
        /// </summary>
        /// <remarks>将堆栈指针设为0</remarks>
        public void ClearStack()
        {
            p_stackPos = 0;
        }

        /// <summary>
        /// 将该执行堆栈的代码指针跳转到指定代码的末尾
        /// </summary>
        /// <param name="code">执行代码</param>
        public void JumpToEnd(ProgramCode code)
        {
            p_codePos = (uint)code.Memory.Length;
        }

        /// <summary>
        /// 按索引访问寄存器值
        /// </summary>
        /// <param name="index">寄存器索引，范围在[0,63]</param>
        /// <param name="value">将值赋值到此处的地址，大小不得小于寄存器大小</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出限制</exception>
        public void GetRegister(int index, void* value)
        {
            if (index < 0 || index > 63) throw new ArgumentOutOfRangeException();

            if (index <= 0xF)
            {
                //32位
                fixed (uint* ip = this.register.reg32)
                {
                    //*((uint*)value) = ip[index];
                    OrderMemoryExtend.OrderInt32ToBytes(ip[index], (byte*)value);
                }
            }
            else if (index <= 0x1F)
            {
                //64位
                index -= 0x10;
                fixed (ulong* ip = this.register.reg64)
                {
                    //*((ulong*)value) = ip[index];
                    OrderMemoryExtend.OrderInt64ToBytes(ip[index], (byte*)value);
                }
            }
            else if (index <= 0x2F)
            {
                //8位
                index -= 0x20;
                fixed (byte* ip = this.register.reg8)
                {
                    *((byte*)value) = ip[index];
                }
            }
            else
            {
                //16位
                index -= 0x30;
                fixed (ushort* ip = this.register.reg16)
                {
                    //*((ushort*)value) = ip[index];
                    OrderMemoryExtend.OrderInt16ToBytes(ip[index], (byte*)value);
                }
            }

        }

        /// <summary>
        /// 按索引设置索引器的值
        /// </summary>
        /// <param name="index">寄存器索引，范围在[0,63]</param>
        /// <param name="value">要设置到寄存器的值，内存大小不得小于寄存器大小</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出限制</exception>
        public void SetRegister(int index, void* value)
        {
            if (index < 0 || index > 63) throw new ArgumentOutOfRangeException();

            if (index <= 0xF)
            {
                //32位
                fixed (uint* ip = this.register.reg32)
                {
                    ip[index] = OrderMemoryExtend.OrderToInt32((byte*)value);
                }
            }
            else if (index <= 0x1F)
            {
                //64位
                index -= 0x10;
                fixed (ulong* ip = this.register.reg64)
                {
                    ip[index] = OrderMemoryExtend.OrderToInt64((byte*)value);
                }
            }
            else if (index <= 0x2F)
            {
                //8位
                index -= 0x20;
                fixed (byte* ip = this.register.reg8)
                {
                    ip[index] = *((byte*)value);
                }
            }
            else
            {
                //16位
                index -= 0x30;
                fixed (ushort* ip = this.register.reg16)
                {
                    ip[index] = OrderMemoryExtend.OrderToInt16((byte*)value);
                }
            }

        }

        /// <summary>
        /// 按索引获取寄存器值
        /// </summary>
        /// <remarks>
        /// 当类型<typeparamref name="T"/>大于寄存器大小时，则按寄存器内存大小拷贝到<typeparamref name="T"/>，余下内存设为0；当<typeparamref name="T"/>小于寄存器大小时，按<typeparamref name="T"/>的字节大小截断寄存器内存返回
        /// </remarks>
        /// <typeparam name="T">获取后的储存类型</typeparam>
        /// <param name="index">寄存器索引</param>
        /// <returns>寄存器内的值；按最小字节截断</returns>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public T GetRegister<T>(int index) where T : unmanaged
        {
            if (index < 0 || index > 63) throw new ArgumentOutOfRangeException();
            byte i8;
            ushort i16;
            uint i32;
            ulong i64;
            //T temp;
            if (index <= 0xF)
            {
                //32位
               
                fixed (uint* ip = this.register.reg32)
                {
                    var re = OrderMemoryExtend.OrderToInt32((byte*)(ip + index));
                    switch (sizeof(T))
                    {
                        case 1:
                            i8 = (byte)re;
                            return *(T*)&i8;
                        case 2:
                            i16 = (ushort)re;
                            return *(T*)&i16;
                        case 4:
                            i32 = (uint)re;
                            return *(T*)&i32;
                        case 8:
                            i64 = (ulong)re;
                            return *(T*)&i64;
                        default:
                            return *(T*)(ip + index);
                    }
                }
            }
            else if (index <= 0x1F)
            {
                //64位
                index -= 0x10;
                fixed (ulong* ip = this.register.reg64)
                {
                    var re = OrderMemoryExtend.OrderToInt64((byte*)(ip + index));
                    switch (sizeof(T))
                    {
                        case 1:
                            i8 = (byte)re;
                            return *(T*)&i8;
                        case 2:
                            i16 = (ushort)re;
                            return *(T*)&i16;
                        case 4:
                            i32 = (uint)re;
                            return *(T*)&i32;
                        case 8:
                            i64 = (ulong)re;
                            return *(T*)&i64;
                        default:
                            return *(T*)(ip + index);
                    }
                }
            }
            else if (index <= 0x2F)
            {
                //8位
                index -= 0x20;
                fixed (byte* ip = this.register.reg8)
                {
                    var re = *(ip + index);
                    switch (sizeof(T))
                    {
                        case 1:
                            return *(T*)&re;
                        case 2:
                            i16 = (ushort)re;
                            return *(T*)&i16;
                        case 4:
                            i32 = (uint)re;
                            return *(T*)&i32;
                        case 8:
                            i64 = (ulong)re;
                            return *(T*)&i64;
                        default:
                            return *(T*)(ip + index);
                    }
                }
            }
            else
            {
                //16位
                index -= 0x30;
                fixed (ushort* ip = this.register.reg16)
                {
                    var re = OrderMemoryExtend.OrderToInt16((byte*)(ip + index));
                    switch (sizeof(T))
                    {
                        case 1:
                            i8 = (byte)re;
                            return *(T*)&i8;
                        case 2:
                            i16 = (ushort)re;
                            return *(T*)&i16;
                        case 4:
                            i32 = (uint)re;
                            return *(T*)&i32;
                        case 8:
                            i64 = (ulong)re;
                            return *(T*)&i64;
                        default:
                            return *(T*)(ip + index);
                    }
                }
            }

        }

        /// <summary>
        /// 按索引设置寄存器值
        /// </summary>
        /// <remarks>
        /// 当<paramref name="value"/>的大小小于要设置的寄存器大小时，寄存器余下内存将被设为0；当<paramref name="value"/>大于寄存器内存长度时，将<paramref name="value"/>截断为寄存器大小设置
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">寄存器索引</param>
        /// <param name="value">要设置的寄存器内值；按最小字节截断</param>
        /// <exception cref="ArgumentOutOfRangeException">索引超出范围</exception>
        public void SetRegister<T>(int index, T value) where T : unmanaged
        {
            if (index < 0 || index > 63) throw new ArgumentOutOfRangeException();

            if (index <= 0xF)
            {
                //32位
                fixed (uint* ip = this.register.reg32)
                {
                    switch (sizeof(T))
                    {
                        case 1:
                            *(ip + index) = (*(byte*)&value);
                            return;
                        case 2:
                            *(ip + index) = (*(ushort*)&value);
                            return;
                        case 4:
                            *(ip + index) = (*(uint*)&value);
                            return;
                        case 8:
                            *(ip + index) = (uint)(*(ulong*)&value);
                            return;
                        default:
                            break;
                    }
                    if (sizeof(T) >= sizeof(uint))
                    {
                        *(ip + index) = *(uint*)&value;
                    }
                    else
                    {
                        ip[index] = default;
                        *(T*)(ip + index) = value;
                    }
                }
            }
            else if (index <= 0x1F)
            {
                //64位
                index -= 0x10;
                fixed (ulong* ip = this.register.reg64)
                {
                    switch (sizeof(T))
                    {
                        case 1:
                            *(ip + index) = (*(byte*)&value);
                            return;
                        case 2:
                            *(ip + index) = (*(ushort*)&value);
                            return;
                        case 4:
                            *(ip + index) = (*(uint*)&value);
                            return;
                        case 8:
                            *(ip + index) = (*(ulong*)&value);
                            return;
                        default:
                            break;
                    }

                    if (sizeof(T) >= sizeof(ulong))
                    {
                        *(ip + index) = *(ulong*)&value;
                    }
                    else
                    {
                        ip[index] = default;
                        *(T*)(ip + index) = value;
                    }
                }

            }
            else if (index <= 0x2F)
            {
                //8位
                index -= 0x20;
                fixed (byte* ip = this.register.reg8)
                {
                    switch (sizeof(T))
                    {
                        case 1:
                            *(ip + index) = (*(byte*)&value);
                            return;
                        case 2:
                            *(ip + index) = (byte)(*(ushort*)&value);
                            return;
                        case 4:
                            *(ip + index) = (byte)(*(uint*)&value);
                            return;
                        case 8:
                            *(ip + index) = (byte)(*(ulong*)&value);
                            return;
                        default:
                            break;
                    }

                    if (sizeof(T) >= sizeof(byte))
                    {
                        *(ip + index) = *(byte*)&value;
                    }
                    else
                    {
                        ip[index] = default;
                        *(T*)(ip + index) = value;
                    }
                }
            }
            else
            {
                //16位
                index -= 0x30;
                fixed (ushort* ip = this.register.reg16)
                {
                    switch (sizeof(T))
                    {
                        case 1:
                            *(ip + index) = (*(byte*)&value);
                            return;
                        case 2:
                            *(ip + index) = (ushort)(*(ushort*)&value);
                            return;
                        case 4:
                            *(ip + index) = (ushort)(*(uint*)&value);
                            return;
                        case 8:
                            *(ip + index) = (ushort)(*(ulong*)&value);
                            return;
                        default:
                            break;
                    }

                    if (sizeof(T) >= sizeof(ushort))
                    {
                        *(ip + index) = *(ushort*)&value;
                    }
                    else
                    {
                        ip[index] = default;
                        *(T*)(ip + index) = value;
                    }
                }
            }

        }

        /// <summary>
        /// 获取栈区指定偏移下的值 - 32位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <returns>指定偏移除向后读取的变量</returns>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public uint GetStackInt32(int offset)
        {
            if (offset < 0 || offset + sizeof(uint) > p_stack.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* sp = p_stack)
            {
                return OrderMemoryExtend.OrderToInt32(sp + offset);
            }
        }

        /// <summary>
        /// 获取栈区指定偏移下的值 - 64位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <returns>指定偏移除向后读取的变量</returns>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public ulong GetStackInt64(int offset)
        {
            if (offset < 0 || offset + sizeof(ulong) > p_stack.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* sp = p_stack)
            {
                return OrderMemoryExtend.OrderToInt64(sp + offset);
            }
        }

        /// <summary>
        /// 获取栈区指定偏移下的值 - 8位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <returns>指定偏移除向后读取的变量</returns>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public byte GetStackInt8(int offset)
        {
            if (offset < 0 || offset + sizeof(byte) > p_stack.Length) throw new ArgumentOutOfRangeException();

            return p_stack[offset];
        }

        /// <summary>
        /// 获取栈区指定偏移下的值 - 16位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <returns>指定偏移除向后读取的变量</returns>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public ushort GetStackInt16(int offset)
        {
            if (offset < 0 || offset + sizeof(ushort) > p_stack.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* sp = p_stack)
            {
                return OrderMemoryExtend.OrderToInt16(sp + offset);
            }
        }

        /// <summary>
        /// 设置栈区指定偏移下的值 - 32位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <param name="value">要设置的指定偏移所在的值</param>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public void SetStackInt32(int offset, uint value)
        {
            if (offset < 0 || offset + sizeof(uint) > p_stack.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* sp = p_stack)
            {
                OrderMemoryExtend.OrderInt32ToBytes(value, sp + offset);
            }
        }

        /// <summary>
        /// 设置栈区指定偏移下的值 - 64位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <param name="value">要设置的指定偏移所在的值</param>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public void SetStackInt64(int offset, ulong value)
        {
            if (offset < 0 || offset + sizeof(ulong) > p_stack.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* sp = p_stack)
            {
                OrderMemoryExtend.OrderInt64ToBytes(value, sp + offset);
            }
        }

        /// <summary>
        /// 设置栈区指定偏移下的值 - 16位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <param name="value">要设置的指定偏移所在的值</param>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public void SetStackInt16(int offset, ushort value)
        {
            if (offset < 0 || offset + sizeof(ulong) > p_stack.Length) throw new ArgumentOutOfRangeException();

            fixed (byte* sp = p_stack)
            {
                OrderMemoryExtend.OrderInt16ToBytes(value, sp + offset);
            }
        }

        /// <summary>
        /// 设置栈区指定偏移下的值 - 8位
        /// </summary>
        /// <param name="offset">从0开始向后的字节偏移</param>
        /// <param name="value">要设置的指定偏移所在的值</param>
        /// <exception cref="ArgumentOutOfRangeException">偏移超出范围</exception>
        public void SetStackInt8(int offset, byte value)
        {
            if (offset < 0 || offset + sizeof(ulong) > p_stack.Length) throw new ArgumentOutOfRangeException();
            p_stack[offset] = value;
        }

        /// <summary>
        /// 获取指定索引下寄存器，所储存的字节大小
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns><paramref name="index"/>索引下寄存器的储存大小；超出索引范围返回0</returns>
        public int GetRegisterSize(int index)
        {
            if(index >= 0 && index < 0x10)
            {
                return 4;
            }
            else if (index < 0x20)
            {
                return 8;
            }
            else if (index < 0x30)
            {
                return 1;
            }
            else if (index < 0x40)
            {
                return 2;
            }

            return 0;
        }

        /// <summary>
        /// 判断索引是否超出寄存器索引范围
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>超出范围返回true，否则返回false</returns>
        public bool IsOutOfRangeRegisterIndex(int index)
        {
            return (index < 0 || index > 63);
        }

        /// <summary>
        /// 判断索引是否没有超出寄存器索引范围
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>超出范围返回false，否则返回true</returns>
        public bool IsNotOutOfRangeRegisterIndex(int index)
        {
            return (index >= 0 && index < 64);
        }

        /// <summary>
        /// 判断指定偏移是否没有超出栈区内存
        /// </summary>
        /// <param name="offset">偏移</param>
        /// <returns>偏移是否处于栈区内存中；处于内存中返回true，否则返回false</returns>
        public bool IsNotOutOfRangeStackOffset(int offset)
        {
            return offset >= 0 && offset < p_stack.Length;
        }

        /// <summary>
        /// 判断指定偏移是否超出栈区内存
        /// </summary>
        /// <param name="offset">偏移</param>
        /// <returns>偏移是否没有处于栈区内存中；处于内存中返回false，否则返回true</returns>
        public bool IsOutOfRangeStackOffset(int offset)
        {
            return offset < 0 || offset >= p_stack.Length;
        }

        #endregion

        #region 派生

        /// <summary>
        /// 以字符串的形式返回该虚拟执行堆栈的实时数据
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// 以字符串的形式返回该虚拟执行堆栈的实时数据
        /// </summary>
        /// <param name="register">是否查看寄存器数据</param>
        /// <returns></returns>
        public string ToString(bool register)
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("可执行堆栈大小:");
            sb.Append((p_stack?.Length).ToString());
            sb.Append("Byte 栈区指针:");
            sb.Append(p_stackPos.ToString());
            sb.Append(" 代码指针:");
            sb.Append(p_codePos.ToString());

            if (register)
            {
                sb.AppendLine();
                sb.Append("寄存器：");
                int i;
                fixed (Register64* rp = &this.register)
                {

                    sb.Append("reg8:");
                    for (i = 0; i < 16; i++)
                    {
                        sb.Append((rp->reg8[i]).ToString("X2"));
                        sb.Append(" ");
                    }
                    sb.AppendLine();
                    sb.Append("reg16:");
                    for (i = 0; i < 16; i++)
                    {
                        sb.Append((rp->reg16[i]).ToString("X4"));
                        sb.Append(" ");
                    }
                    sb.AppendLine();
                    sb.Append("reg32:");
                    for (i = 0; i < 16; i++)
                    {
                        sb.Append((rp->reg32[i]).ToString("X8"));
                        sb.Append(" ");
                    }
                    sb.AppendLine();
                    sb.Append("reg64:");
                    for (i = 0; i < 16; i++)
                    {
                        sb.Append((rp->reg64[i]).ToString("X16"));
                        sb.Append(" ");
                    }

                }

            }


            return sb.ToString();
        }

        /// <summary>
        /// 以字符串的形式返回指定索引下的寄存器内容
        /// </summary>
        /// <param name="index">寄存器索引</param>
        /// <returns>指定索引下的寄存器值</returns>
        /// <exception cref="ArgumentOutOfRangeException">超出索引范围</exception>
        public string ToRegisterString(int index)
        {
            if (IsOutOfRangeRegisterIndex(index)) throw new ArgumentOutOfRangeException();

            var size = GetRegisterSize(index);

            if(size == 1)
            {
                byte b;
                GetRegister(index, &b);
                return b.ToString();
            }
            else if (size == 2)
            {
                ushort b;
                GetRegister(index, &b);
                return b.ToString();
            }
            else if (size == 4)
            {
                int b;
                GetRegister(index, &b);
                return b.ToString();
            }
            else
            {
                long b;
                GetRegister(index, &b);
                return b.ToString();
            }
        }

        #endregion

        #endregion

    }

}
