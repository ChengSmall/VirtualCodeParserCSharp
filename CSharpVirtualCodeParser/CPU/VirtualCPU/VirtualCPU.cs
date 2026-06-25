using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 自定义程序代码调用方法委托
    /// </summary>
    /// <param name="cpu">执行自定义程序代码的cpu</param>
    /// <param name="code">要执行的程序代码块</param>
    /// <param name="stack">要执行的堆栈</param>
    /// <param name="next4Bit">类型字节代码后的4bit位</param>
    /// <returns>错误代码，若此次执行没有错误则返回<see cref="VirtualErrorCode.NoneError"/></returns>
    public delegate VirtualErrorCode InvokeExecute(VirtualCPU cpu,
        ProgramCode code, VirtualStack stack, byte next4Bit);

    /// <summary>
    /// 可执行指令的虚拟CPU
    /// </summary>
    public unsafe class VirtualCPU : IDisposable
    {

        #region 释放

        private void f_freeEvent()
        {
            p_customExecuteEvent = null;
        }

        /// <summary>
        /// 注销该虚拟CPU的自定义代码系统
        /// </summary>
        public void Close()
        {
            f_dispose(true);
        }

        private bool p_isDispose = false;

        private void f_dispose(bool suppressFinalize)
        {
            if (p_isDispose) return;

            p_isDispose = true;
            if (suppressFinalize)
            {
                f_freeEvent();
            }

            if (suppressFinalize)
            {
                GC.SuppressFinalize(this);
            }
        }

        void IDisposable.Dispose()
        {
            f_dispose(true);
        }

        #endregion

        #region 构造
        /// <summary>
        /// 实例化虚拟cpu
        /// </summary>
        public VirtualCPU()
        {
            p_errorExcp = null;
            p_customExecuteEvent = null;
        }

        /// <summary>
        /// 实例化构造
        /// </summary>
        /// <param name="ctor">true则正常构造字段参数</param>
        protected VirtualCPU(bool ctor)
        {            
            if (ctor)
            {
                p_errorExcp = null;
                p_customExecuteEvent = null;
            }
        }
        #endregion

        #region 参数

        /// <summary>
        /// 执行自定义程序代码时调用的方法
        /// </summary>
        protected InvokeExecute p_customExecuteEvent;

        /// <summary>
        /// 指令执行错误时的异常
        /// </summary>
        protected Exception p_errorExcp;

        #endregion

        #region 功能

        #region 参数访问

        /// <summary>
        /// 指令执行错误时的异常，若没有错误异常则为null
        /// </summary>
        /// <returns>在每次执行指令后会变更为当前引发错误异常，若没有错误或错误没有引发异常则为null</returns>
        public Exception ErrorException => p_errorExcp;

        #endregion

        #region 自定义程序

        /// <summary>
        /// 执行自定义程序代码时调用的方法
        /// </summary>
        /// <exception cref="ObjectDisposedException">自定义系统已注销</exception>
        public InvokeExecute CustomExecuteEvent
        {
            get => p_customExecuteEvent;
            set
            {
                if (p_isDispose) throw new ObjectDisposedException(string.Empty);
                p_customExecuteEvent = value;
            }
        }

        #endregion

        #region 封装

        #region 运算

#if DEBUG
        /// <summary>
        /// 执行运算
        /// </summary>
        /// <param name="next4Type">第一个指令的后4bit</param>
        /// <param name="code">程序</param>
        /// <param name="stack">堆栈</param>
        /// <param name="nextOnceCodePos">代码指向指针，下一个字节代码位</param>
        /// <returns></returns>
#endif
        private VirtualErrorCode f_ALUS(Bit4Type next4Type, ProgramCode code, VirtualStack stack, uint nextOnceCodePos)
        {
            ref Register64 reg = ref stack.register;
            ref uint codePos = ref stack.p_codePos;
            ref uint stackPos = ref stack.p_stackPos;
            byte[] stackBuf = stack.p_stack;
            byte[] codeBuf = code.Memory;

            byte b0, b1, b2, b3;
            uint bu32_1, bu32_2, bu32_3, bu32_4;
            ulong bu64_1, bu64_2, bu64_3;
            VirtualErrorCode errorCode;
            ALUS.Type at;

            //推进指针后的值
            bu32_4 = nextOnceCodePos + 4;

            //计算类型
            b0 = codeBuf[nextOnceCodePos];
            //寄存器a值
            b1 = codeBuf[nextOnceCodePos + 1];
            //b
            b2 = codeBuf[nextOnceCodePos + 2];
            //返回值
            b3 = codeBuf[nextOnceCodePos + 3];

            if (bu32_4 > codeBuf.Length)
            {
                return VirtualErrorCode.CodeOutRange;
            }

            at = (ALUS.Type)b0;

            if (next4Type == Bit4Type.x0)
            {
                //进行4字节整形运算
                bu32_1 = stack.GetRegister<uint>(b1);
                bu32_2 = stack.GetRegister<uint>(b2);
                errorCode = ALUS.Invoke32(at, bu32_1, bu32_2, true, out bu32_3);

                if(errorCode != VirtualErrorCode.NoneError)
                {
                    return errorCode;
                }                

                stack.SetRegister(b3, bu32_3);
            }
            else if (next4Type == Bit4Type.x1)
            {
                //进行8字节整形运算
                bu64_1 = stack.GetRegister<ulong>(b1);
                bu64_2 = stack.GetRegister<ulong>(b2);
                errorCode = ALUS.Invoke64(at, bu64_1, bu64_2, true, out bu64_3);

                if (errorCode != VirtualErrorCode.NoneError)
                {
                    return errorCode;
                }

                stack.SetRegister(b3, bu64_3);
            }
            else if (next4Type == Bit4Type.x2)
            {
                //进行4字节无符号整形运算
                bu32_1 = stack.GetRegister<uint>(b1);
                bu32_2 = stack.GetRegister<uint>(b2);
                errorCode = ALUS.Invoke32(at, bu32_1, bu32_2, false, out bu32_3);

                if (errorCode != VirtualErrorCode.NoneError)
                {
                    return errorCode;
                }

                stack.SetRegister(b3, bu32_3);
            }
            else if (next4Type == Bit4Type.x3)
            {
                //进行8字节无符号整形运算
                bu64_1 = stack.GetRegister<ulong>(b1);
                bu64_2 = stack.GetRegister<ulong>(b2);
                errorCode = ALUS.Invoke64(at, bu64_1, bu64_2, false, out bu64_3);

                if (errorCode != VirtualErrorCode.NoneError)
                {
                    return errorCode;
                }

                stack.SetRegister(b3, bu64_3);
            }
            else if (next4Type == Bit4Type.x4)
            {
                //进行单浮点运算
                bu32_1 = stack.GetRegister<uint>(b1);
                bu32_2 = stack.GetRegister<uint>(b2);
                errorCode = ALUS.InvokeF32(at, *(float*)&bu32_1, *(float*)&bu32_2, out (*(float*)&bu32_3));

                if (errorCode != VirtualErrorCode.NoneError)
                {
                    return errorCode;
                }

                stack.SetRegister(b3, bu32_3);
            }
            else if (next4Type == Bit4Type.x5)
            {
                //进行双浮点运算
                bu64_1 = stack.GetRegister<uint>(b1);
                bu64_2 = stack.GetRegister<uint>(b2);
                errorCode = ALUS.InvokeF64(at, *(double*)&bu64_1, *(double*)&bu64_2, out (*(double*)&bu64_3));

                if (errorCode != VirtualErrorCode.NoneError)
                {
                    return errorCode;
                }

                stack.SetRegister(b3, bu64_3);
            }
            else
            {
                return VirtualErrorCode.UnableParseCodeError;
            }

            codePos = bu32_4;
            return (bu32_4 == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
        }

        #endregion

        #region 赋值

#if DEBUG
        /// <summary>
        /// 赋值运算
        /// </summary>
        /// <param name="next4Type">第一个指令的后4bit</param>
        /// <param name="code">程序</param>
        /// <param name="stack">堆栈</param>
        /// <param name="nextOnceCodePos">代码指向指针，第二个字节代码位</param>
        /// <returns></returns>
#endif
        private VirtualErrorCode f_operAss(Bit4Type next4Type, ProgramCode code, VirtualStack stack, uint nextOnceCodePos)
        {
            ref Register64 reg = ref stack.register;
            ref uint codePos = ref stack.p_codePos;
            ref uint stackPos = ref stack.p_stackPos;
            byte[] stackBuf = stack.p_stack;
            byte[] codeBuf = code.Memory;

            ulong bufu64;
            uint bufu32, bufu32_2;

            short buf16;

            int buf32, buf32_2, buf32_3;

            byte b1, b2, b3, b4;

            bool flag1;
            void* ptr;

            if (nextOnceCodePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

            if (next4Type == Bit4Type.x0)
            {
                //后1byte和后2byte，分别表示读取和写入的寄存器编号

                //代码超出范围
                bufu32 = nextOnceCodePos + 2;
                if (bufu32 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                //将代码指针推进2字节
                codePos = bufu32;

                //获取读取的寄存器索引
                b1 = codeBuf[nextOnceCodePos];
                //获取写入的寄存器索引
                b2 = codeBuf[nextOnceCodePos + 1];

                //超出索引
                flag1 = stack.IsOutOfRangeRegisterIndex(b1) || stack.IsOutOfRangeRegisterIndex(b2);
                if (flag1) return VirtualErrorCode.UnableParseCodeError;

                //读取寄存器值
                bufu64 = 0;
                stack.GetRegister(b1, &bufu64);

                //写入值
                stack.SetRegister(b2, &bufu64);

                //返回执行结果
                if (codePos == codeBuf.Length) return VirtualErrorCode.CodeEnded;
                return VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x1)
            {
                /*
                    从栈区内存赋值到寄存器，内存长度按寄存器大小
                    后1byte表示赋值到的reg索引
                    后2byte表示读取栈区地址值的reg索引
                 */

                bufu32 = nextOnceCodePos + 2;
                if (bufu32 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                codePos = bufu32;

                //赋值到的reg索引
                b1 = codeBuf[nextOnceCodePos];

                //读取到的索引
                b2 = codeBuf[nextOnceCodePos + 1];

                //索引超出判断
                flag1 = stack.IsOutOfRangeRegisterIndex(b1) || stack.IsOutOfRangeRegisterIndex(b2);
                if (flag1) return VirtualErrorCode.UnableParseCodeError;

                //获取栈区地址值
                buf32 = stack.GetRegister<int>(b2);

                //栈区地址溢出
                flag1 = buf32 >= stackBuf.Length;
                if (flag1) return VirtualErrorCode.StackOverflowError;

                //寄存器大小
                buf32_2 = stack.GetRegisterSize(b1);

                switch (buf32_2)
                {
                    case 1:
                        stack.SetRegister(b1, stack.GetStackInt8(buf32));
                        break;
                    case 2:
                        stack.SetRegister(b1, stack.GetStackInt16(buf32));
                        break;
                    case 4:
                        stack.SetRegister(b1, stack.GetStackInt32(buf32));
                        break;
                    case 8:
                        stack.SetRegister(b1, stack.GetStackInt64(buf32));
                        break;
                    default:
                        return VirtualErrorCode.UnableParseCodeError;
                }               

                //返回执行结果
                if (codePos == codeBuf.Length) return VirtualErrorCode.CodeEnded;
                return VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x2)
            {
                /*
                从代码常量赋值到寄存器，常量读取长度按后2bit
                后1byte前6bit表示赋值到的r索引；
                后1byte后2bit表示代码常量读取的长度：
                0b00 = 1byte; 0b01 = 2byte; 0b10 = 4byte; 0b11 = 8byte
                */

                //获取后1byte前6位 reg索引
                b1 = (byte)(codeBuf[nextOnceCodePos] & 0b0011_1111);

                //获取后2位
                b2 = (byte)((codeBuf[nextOnceCodePos] >> 6) & 0b0000_0011);

                //读取代码常量

                fixed (byte* codeBufPtr = codeBuf)
                {
                    bufu32 = nextOnceCodePos + 1;
                    

                    if (b2 == 0b00)
                    {
                        //获取值 1byte
                        if (bufu32 >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                        b3 = codeBufPtr[bufu32];

                        //将值写入寄存器
                        stack.SetRegister<byte>(b1, b3);
                        codePos = bufu32;

                    }
                    else if (b2 == 0b01)
                    {
                        //获取值 2byte
                        codePos = bufu32 + 1;

                        if (codePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                        //OrderMemoryExtend.OrderToInt16(codeBufPtr + bufu32);
                        stack.SetRegister<ushort>(b1, (OrderMemoryExtend.OrderToInt16(codeBufPtr + bufu32)));

                    }
                    else if (b2 == 0b10)
                    {
                        codePos = bufu32 + 3;
                        //获取值 4byte
                        if (codePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                        //OrderMemoryExtend.OrderToInt32(codeBufPtr + bufu32);
                        stack.SetRegister<uint>(b1, OrderMemoryExtend.OrderToInt32(codeBufPtr + bufu32));
                    }
                    else
                    {
                        codePos = bufu32 + 7;
                        //获取值 8byte
                        if (codePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                        stack.SetRegister<ulong>(b1, OrderMemoryExtend.OrderToInt64(codeBufPtr + bufu32));
                    }

                    codePos++;
                    return codePos == codeBuf.Length ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;

                }

                

            }
            else if (next4Type == Bit4Type.x3)
            {
                /*
                从寄存器赋值到栈区内存，内存长度按寄存器大小
                后1byte的前6bit表示读取的r索引
                后2byte表示赋值到的，包含栈区内存地址的reg索引
                */

                bufu32 = nextOnceCodePos;
                codePos = bufu32 + 2;

                if (bufu32 >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                //获取栈区地址的寄存器值
                b1 = (byte)(codeBuf[bufu32 + 1] & 0b00_111111);
                //获取赋值到的栈区地址
                buf32 = stack.GetRegister<int>(b1);

                //获取寄存器索引
                b1 = (byte)(codeBuf[bufu32] & 0b00_111111);

                //获取寄存器值大小
                buf32_2 = stack.GetRegisterSize(b1);
                
                fixed (byte* stackBufPtr = stackBuf)
                {

                    //_ = (stackBufPtr + buf32); 栈区首地址
                    switch (buf32_2)
                    {
                        case 1:
                            //赋值
                            stack.GetRegister(b1, &b2);
                            //*(stackBufPtr + buf32) = b2;
                            stack.SetStackInt8(buf32, b2);
                            break;
                        case 2:
                            stack.GetRegister(b1, &buf16);
                            //*((short*)(stackBufPtr + buf32)) = buf16;
                            stack.SetStackInt16(buf32, (ushort)buf16);
                            break;
                        case 4:
                            stack.GetRegister(b1, &buf32_3);
                            //*((int*)(stackBufPtr + buf32)) = buf32_3;
                            stack.SetStackInt32(buf32, (uint)buf32_3);
                            break;
                        case 8:
                            stack.GetRegister(b1, &bufu64);
                            //*((ulong*)(stackBufPtr + buf32)) = bufu64;
                            stack.SetStackInt64(buf32, bufu64);
                            break;
                        default:
                            return VirtualErrorCode.UnableParseCodeError;
                    }

                }

                return (codePos == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x4)
            {
                /*
                stackSize to reg
                将[栈区最大字节数]值赋值到指定寄存器，内存按寄存器截断
                后1byte的前6bit表示赋值到的r索引
                 */

                //寄存器索引
                b1 = (byte)(codeBuf[nextOnceCodePos] & 0b00_111111);

                //获取栈区容量
                buf32 = stackBuf.Length;
                //写入寄存器
                stack.SetRegister<int>(b1, buf32);

                //提升代码指针
                bufu32 = nextOnceCodePos + 1;

                //返回执行结果
                codePos = bufu32;
                return (bufu32 == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x5)
            {
                /*
                stackPosMem to reg
                将栈区指定偏移内存写入到寄存器，内存按寄存器向后截断

                后1byte字节的前6bit表示写入到的reg索引；
                后2byte为reg索引
                pos = ([栈区位指针] - value); 
                value表示reg的值;
                从值[pos]处的栈区位向后读取的内存
                 */

                bufu32_2 = nextOnceCodePos + 2;
                if (bufu32_2 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;


                //写入的寄存器索引
                b1 = (byte)(codeBuf[nextOnceCodePos] & 0b00_111111);

                //栈区地址值寄存器索引
                b2 = codeBuf[nextOnceCodePos + 1];

                //获取栈区地址值
                bufu32 = stack.GetRegister<uint>(b2);

                //获取pos
                buf32_2 = (int)(stackPos - bufu32);

                //获取栈区长度
                buf32 = stackBuf.Length;

                fixed (byte* stackPtr = stackBuf)
                {
                    //寄存器大小
                    b4 = (byte)stack.GetRegisterSize(b1);

                    //判断堆栈索引错误
                    if (buf32_2 < 0 || (buf32_2 + b4) >= buf32) return VirtualErrorCode.StackOverflowError;

                    //将栈区值写入到索引b1的寄存器
                    //stack.GetStackInt64();
                    stack.SetRegister(b1, ((stackPtr + buf32_2)));

                }

                codePos = bufu32_2;
                return (bufu32_2 == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;

            }
            else if (next4Type == Bit4Type.x6)
            {
                /*
                reg to stackPosMem
                将寄存器写入到栈区指定偏移内存，内存按寄存器向后截断

                后1byte读取的reg索引；
                后2byte为reg索引，代表 pos = ([栈区位指针] - [reg值]);
                从[pos]处的栈区位向后写入的内存
                 */

                bufu32_2 = nextOnceCodePos + 2;
                if (bufu32_2 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;


                //读取的寄存器索引
                b1 = codeBuf[nextOnceCodePos];

                //栈区地址值寄存器索引
                b2 = codeBuf[nextOnceCodePos + 1];

                //获取栈区地址值
                bufu32 = stack.GetRegister<uint>(b2);

                //获取pos
                buf32_2 = (int)(stackPos - bufu32);

                //获取栈区长度
                buf32 = stackBuf.Length;

                fixed (byte* stackPtr = stackBuf)
                {
                    //寄存器大小
                    b4 = (byte)stack.GetRegisterSize(b1);

                    //判断堆栈索引错误
                    if (buf32_2 < 0 || (buf32_2 + b4) >= buf32) return VirtualErrorCode.StackOverflowError;

                    //将索引b1的寄存器写入到pos位置的栈区
                    stack.GetRegister(b1, ((stackPtr + buf32_2)));

                }

                codePos = bufu32_2;
                return (bufu32_2 == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;

            }
            else if (next4Type == Bit4Type.x7)
            {
                /*
                Memory to reg
                从父程序内存赋值到寄存器，内存长度按寄存器大小
                后1byte为写入到的reg索引，后2byte表示reg索引，为.net虚拟内存地址值
                内存读取长度按寄存器大小
                 */

                //推进代码指针
                bufu32_2 = nextOnceCodePos + 2;
                if (bufu32_2 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                //写入到的reg索引
                b1 = codeBuf[nextOnceCodePos];
                //储存.net内存的值的寄存器索引
                b2 = codeBuf[nextOnceCodePos + 1];
                
                //获取地址
                ptr = stack.GetRegister<IntPtr>(b2).ToPointer();

                //为寄存器设置地址所在值
                stack.SetRegister(b1, ptr);

                codePos = bufu32_2;
                return (bufu32_2 == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x8)
            {
                /*
                reg to Memory
                从寄存器赋值到父程序内存，内存长度按寄存器大小
                后1byte为读取到的reg索引，后2byte表示reg索引，为要写入的.net虚拟内存值
                 */

                //推进代码指针
                bufu32_2 = nextOnceCodePos + 2;
                if (bufu32_2 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                //读取的reg索引
                b1 = codeBuf[nextOnceCodePos];
                //写入到的.net内存的值的寄存器索引
                b2 = codeBuf[nextOnceCodePos + 1];

                //获取地址
                ptr = stack.GetRegister<IntPtr>(b2).ToPointer();

                //为寄存器设置地址所在值
                stack.GetRegister(b1, ptr);

                codePos = bufu32_2;
                return (bufu32_2 == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;

            }

            return VirtualErrorCode.UnableParseCodeError;
        }
        #endregion

        #region 跳转

#if DEBUG
        /// <summary>
        /// 代码跳转
        /// </summary>
        /// <param name="next4Type">第一个指令的后4bit</param>
        /// <param name="code">程序</param>
        /// <param name="stack">堆栈</param>
        /// <param name="nextOnceCodePos">代码指向指针，下一个字节代码位</param>
        /// <returns></returns>
#endif
        private VirtualErrorCode f_jumpCodePos(Bit4Type next4Type, ProgramCode code, VirtualStack stack, uint nextOnceCodePos)
        {
            ref Register64 reg = ref stack.register;
            ref uint codePos = ref stack.p_codePos;
            ref uint stackPos = ref stack.p_stackPos;
            byte[] stackBuf = stack.p_stack;
            byte[] codeBuf = code.Memory;

            byte b1, b2, b4;
            bool bo1;
            int b32_1, b32_2;
            uint bu32_1;
            long b64_1;
            short bs1;

            if (next4Type == Bit4Type.x0)
            {
                /*
                无条件跳转，到寄存器地址
				读取1个byte位reg索引，表示跳转到的地址
                 */

                //越界
                if (nextOnceCodePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                //寄存器索引
                b1 = codeBuf[nextOnceCodePos];

                //要跳转到的位置
                //stack.GetRegister(b1, &b32_1);
                b32_1 = stack.GetRegister<int>(b1);

                if (b32_1 < 0 || b32_1 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                //赋值到代码指针
                codePos = (uint)b32_1;

                if (b32_1 == codeBuf.Length)
                {
                    return VirtualErrorCode.CodeEnded;
                }

                return VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x1)
            {
                /*
                无条件跳转，到常量地址
				读取4个byte值，表示跳转到的位置
                 */
                bu32_1 = nextOnceCodePos + 3;

                if (bu32_1 >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                fixed (byte* codePtr = codeBuf)
                {
                    //读取位置
                    //b32_2 = *(int*)(codePtr + nextOnceCodePos);
                    b32_2 = (int)OrderMemoryExtend.OrderToInt32(codePtr + nextOnceCodePos);
                }

                if (b32_2 < 0 || b32_2 > codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                //赋值到代码指针
                codePos = (uint)b32_2;
                if (b32_2 == codeBuf.Length)
                {
                    return VirtualErrorCode.CodeEnded;
                }

                return VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x2)
            {
                /*
                非0跳转，到寄存器地址
				后1byte值表示reg索引，为是否非0的寄存器；
				后2byte，表示reg索引，值为跳转到的位置；
				若寄存器非0，则跳转到指定位置
                 */

                bu32_1 = nextOnceCodePos + 1;
                if (bu32_1 >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                b1 = codeBuf[nextOnceCodePos];

                b4 = (byte)stack.GetRegisterSize(b1);

                switch (b4)
                {
                    case 1:
                        stack.GetRegister(b1, &b1);
                        bo1 = b1 != 0;
                        break;
                    case 2:
                        stack.GetRegister(b1, &bs1);
                        bo1 = bs1 != 0;
                        break;
                    case 4:
                        stack.GetRegister(b1, &b32_1);
                        bo1 = b32_1 != 0;
                        break;
                    case 8:
                        stack.GetRegister(b1, &b64_1);
                        bo1 = b64_1 != 0;
                        break;
                    default:
                        return VirtualErrorCode.UnableParseCodeError;
                }

                if (bo1)
                {
                    //跳转
                    b2 = codeBuf[bu32_1];
                    if (stack.IsOutOfRangeRegisterIndex(b2)) return VirtualErrorCode.UnableParseCodeError;
                    //跳转
                    
                    codePos = stack.GetRegister<uint>(b2);
                    //fixed (void* codePosPtr = &codePos)
                    //{
                    //    stack.GetRegister(b2, codePosPtr);
                    //}
                }
                else
                {
                    //不跳转
                    codePos = nextOnceCodePos + 2;
                }
                if (codePos > codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                return codePos == codeBuf.Length ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x3)
            {
                /*
                非0跳转，到常量地址
                后1byte表示reg索引，为是否非0的寄存器；
                后2byte到后5byte组成4字节值，表示其跳转位置：
                若寄存器非0，则跳转到指定位置
                */

                bu32_1 = nextOnceCodePos + 4;
                if (bu32_1 >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                b1 = codeBuf[nextOnceCodePos];

                b4 = (byte)stack.GetRegisterSize(b1);

                switch (b4)
                {
                    case 1:
                        stack.GetRegister(b1, &b1);
                        bo1 = b1 != 0; //非零
                        break;
                    case 2:
                        stack.GetRegister(b1, &bs1);
                        bo1 = bs1 != 0; //非零
                        break;
                    case 4:
                        stack.GetRegister(b1, &b32_1);
                        bo1 = b32_1 != 0; //非零
                        break;
                    case 8:
                        stack.GetRegister(b1, &b64_1);
                        bo1 = b64_1 != 0; //非零
                        break;
                    default:
                        return VirtualErrorCode.UnableParseCodeError;
                }

                if (bo1)
                {
                    //跳转
                    //if (bu32_1 + 3 >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

                    fixed (byte* codePtr = codeBuf)
                    {
                        //codePos = *(uint*)(codePtr + (nextOnceCodePos + 1));
                        codePos = OrderMemoryExtend.OrderToInt32(codePtr + (nextOnceCodePos + 1));
                    }
                    
                }
                else
                {
                    //不跳转
                    codePos = bu32_1 + 1;
                }

                if (codePos > codeBuf.Length) return VirtualErrorCode.CodeOutRange;
                return codePos == codeBuf.Length ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;

            }

            //else if (next4Type == Bit4Type.x4)
            //{

            //}
            //else if (next4Type == Bit4Type.x5)
            //{

            //}
            //else if (next4Type == Bit4Type.x6)
            //{

            //}
            //else if (next4Type == Bit4Type.x7)
            //{

            //}
            //else if (next4Type == Bit4Type.x8)
            //{

            //}
            //else if (next4Type == Bit4Type.x9)
            //{

            //}
            //else if (next4Type == Bit4Type.xA)
            //{

            //}
            //else if (next4Type == Bit4Type.xB)
            //{

            //}
            //else if (next4Type == Bit4Type.xC)
            //{

            //}
            //else if (next4Type == Bit4Type.xD)
            //{

            //}
            //else if (next4Type == Bit4Type.xE)
            //{

            //}
            //else if (next4Type == Bit4Type.xF)
            //{

            //}

            return VirtualErrorCode.UnableParseCodeError;
        }
        #endregion

        #region 入栈

#if DEBUG
        /// <summary>
        /// 入栈
        /// </summary>
        /// <param name="next4Type">第一个指令的后4bit</param>
        /// <param name="code">程序</param>
        /// <param name="stack">堆栈</param>
        /// <param name="nextOnceCodePos">代码指向指针，下一个字节代码位</param>
        /// <returns></returns>
#endif
        private VirtualErrorCode f_push(Bit4Type next4Type, ProgramCode code, VirtualStack stack, uint nextOnceCodePos)
        {

            ref Register64 reg = ref stack.register;
            ref uint codePos = ref stack.p_codePos;
            ref uint stackPos = ref stack.p_stackPos;
            byte[] stackBuf = stack.p_stack;
            byte[] codeBuf = code.Memory;

            if (nextOnceCodePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

            codePos = nextOnceCodePos + 1;

            var regIndex = codeBuf[nextOnceCodePos];

            if (next4Type == Bit4Type.x0)
            {
                /*
                使用寄存器大小表示入栈字节量
                后1byte：
                表示reg索引，值记作入栈后写入的数据
                入栈字节量取决于寄存器大小
                */
                int size = stack.GetRegisterSize(regIndex);
                uint endPos;

                fixed (byte* stackPtr = stackBuf)
                {

                    switch (size)
                    {
                        case 1:
                            byte v;
                            stack.GetRegister(regIndex, &v);

                            endPos = stackPos;
                            if (endPos >= stackBuf.Length) return VirtualErrorCode.StackOverflowError;

                            stackBuf[stackPos] = v;
                            stackPos = endPos + 1;
                            break;

                        case 2:
                            short v2;
                            stack.GetRegister(regIndex, &v2);

                            endPos = stackPos + 1;
                            if (endPos >= stackBuf.Length) return VirtualErrorCode.StackOverflowError;

                            *(short*)(stackPtr + stackPos) = v2;
                            stackPos = endPos + 1;
                            break;

                        case 4:
                            int v4;
                            stack.GetRegister(regIndex, &v4);

                            endPos = stackPos + 3;
                            if (endPos >= stackBuf.Length) return VirtualErrorCode.StackOverflowError;

                            *(int*)(stackPtr + stackPos) = v4;
                            stackPos = endPos + 1;
                            break;

                        case 8:
                            long v8;
                            stack.GetRegister(regIndex, &v8);

                            endPos = stackPos + 7;
                            if (endPos >= stackBuf.Length) return VirtualErrorCode.StackOverflowError;

                            *(long*)(stackPtr + stackPos) = v8;
                            stackPos = endPos + 1;
                            break;
                        default:
                            return VirtualErrorCode.UnableParseCodeError;
                    }


                }

                return codePos == codeBuf.Length ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
            }
            else if (next4Type == Bit4Type.x1)
            {
                /*
                使用值量表示入栈字节量
                向后1byte，表示寄存器索引；
                寄存器的值表示入栈的大小；
                */

                int size = stack.GetRegister<int>(regIndex);

                uint endPos = (uint)(stackPos + size);

                if (endPos >= stackBuf.Length) return VirtualErrorCode.StackOverflowError;

                stackPos = endPos;

                return codePos == codeBuf.Length ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
            }

            return VirtualErrorCode.UnableParseCodeError;
        }

        #endregion

        #region 出栈

#if DEBUG
        /// <summary>
        /// 弹栈
        /// </summary>
        /// <param name="next4Type">第一个指令的后4bit</param>
        /// <param name="code">程序</param>
        /// <param name="stack">堆栈</param>
        /// <param name="nextOnceCodePos">代码指向指针，下一个字节代码位</param>
        /// <returns></returns>
#endif
        private VirtualErrorCode f_pop(Bit4Type next4Type, ProgramCode code, VirtualStack stack, uint nextOnceCodePos)
        {

            ref Register64 reg = ref stack.register;
            ref uint codePos = ref stack.p_codePos;
            ref uint stackPos = ref stack.p_stackPos;
            byte[] stackBuf = stack.p_stack;
            byte[] codeBuf = code.Memory;

            if (nextOnceCodePos >= codeBuf.Length) return VirtualErrorCode.CodeOutRange;

            codePos = nextOnceCodePos + 1;

            int ri = codeBuf[nextOnceCodePos];

            int rsize = stack.GetRegisterSize(ri);

            int stackEnd;

            if (next4Type == Bit4Type.x0)
            {
                /*
                弹出栈区内存到寄存器
                后1byte表示写入到的reg索引;
                弹栈的大小使用寄存器大小
                */

                //获取弹栈后指针
                stackEnd = (int)(stackPos - rsize);

                if (stackEnd < 0) return VirtualErrorCode.StackOverflowError;
                //赋值到寄存器
                fixed (byte* stackPtr = stackBuf)
                {
                    stack.SetRegister(ri, (stackPtr + stackEnd));
                }

                stackPos = (uint)stackEnd;
            }
            else if (next4Type == Bit4Type.x1)
            {
                /*
                弹出栈区，舍弃值
                后1byte表示reg索引，其值表示要弹出的字节大小的值
                 */

                int valueSize = stack.GetRegister<int>(ri);

                //获取弹栈后指针
                stackEnd = (int)(stackPos - valueSize);

                if (stackEnd < 0) return VirtualErrorCode.StackOverflowError;

                //设置新栈指针
                stackPos = (uint)stackEnd;

            }
            else
            {
                return VirtualErrorCode.UnableParseCodeError;
            }

            return (codePos == codeBuf.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
        }
        #endregion

        #endregion

        /// <summary>
        /// 在指定堆栈执行一条指令
        /// </summary>
        /// <param name="code">要执行的程序代码块</param>
        /// <param name="stack">要执行的堆栈</param>
        /// <returns>指令执行结束代码，若此次执行没有错误则返回<see cref="VirtualErrorCode.NoneError"/></returns>
        /// <exception cref="ArgumentNullException">参数为null</exception>
        /// <exception cref="Exception">其它异常</exception>
        public VirtualErrorCode Execute(ProgramCode code, VirtualStack stack)
        {
            if (code is null || stack is null) throw new ArgumentNullException();

            var cb = code.Memory;
            int length = cb.Length;
            uint codePos = stack.p_codePos;

            //超出代码范围
            if (codePos == length)
            {
                if (codePos > length)
                {
                    return VirtualErrorCode.CodeOutRange;
                }
                else
                {
                    return VirtualErrorCode.CodeEnded;
                }
            }

            byte typeByte = cb[codePos];

            byte lateType = (byte)(typeByte & 0b1111);
            byte nextType = (byte)((typeByte >> 4) & 0b1111);
            var nextPos = codePos + 1;

            switch (lateType)
            {
                case 0x0:
                    //空指令
                    stack.p_codePos = nextPos;
                    return (nextPos == cb.Length) ? VirtualErrorCode.CodeEnded : VirtualErrorCode.NoneError;
                case 0x1:
                    //运算单元
                    try
                    {
                        return f_ALUS((Bit4Type)nextType, code, stack, nextPos);
                    }
                    catch (Exception ex)
                    {
                        p_errorExcp = ex;
                        return VirtualErrorCode.ThrowExceptionError;
                    }
                   
                case 0x2:
                    //赋值
                    try
                    {
                        return f_operAss((Bit4Type)nextType, code, stack, nextPos);
                    }
                    catch (Exception ex)
                    {
                        p_errorExcp = ex;
                        return VirtualErrorCode.ThrowExceptionError;
                    }
                 
                case 0x3:
                    //跳转
                    try
                    {
                        return f_jumpCodePos((Bit4Type)nextType, code, stack, nextPos);
                    }
                    catch (Exception ex)
                    {
                        p_errorExcp = ex;
                        return VirtualErrorCode.ThrowExceptionError;
                    }
                    
                case 0x4:
                    //入栈
                    try
                    {
                        return f_push((Bit4Type)nextType, code, stack, nextPos);
                    }
                    catch (Exception ex)
                    {
                        p_errorExcp = ex;
                        return VirtualErrorCode.ThrowExceptionError;
                    }
                   
                case 0x5:
                    //弹栈
                    try
                    {
                        return f_pop((Bit4Type)nextType, code, stack, nextPos);
                    }
                    catch (Exception ex)
                    {
                        p_errorExcp = ex;
                        return VirtualErrorCode.ThrowExceptionError;
                    }

                case 0xE:
                    stack.JumpToEnd(code);
                    return VirtualErrorCode.CodeEnded;

                case 0xF:
                    //自定义代码
                    stack.p_codePos = nextPos;
                    if (p_customExecuteEvent is null)
                    {
                        //不存在自定义代码
                        return VirtualErrorCode.NoCustomCodeExists;
                    }
                    try
                    {
                        return p_customExecuteEvent.Invoke(this, code, stack, nextType);
                    }
                    catch (Exception ex)
                    {
                        p_errorExcp = ex;
                        return VirtualErrorCode.CustomCodeExceptionError;
                    }

                default:
                    return VirtualErrorCode.UnableParseCodeError;
            }

        }

        #endregion

    }

}
#if DEBUG
#endif