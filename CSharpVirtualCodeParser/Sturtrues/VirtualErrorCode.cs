using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 虚拟机执行的错误代码
    /// </summary>
    public enum VirtualErrorCode : uint
    {

        /// <summary>
        /// 没有错误
        /// </summary>
        NoneError = 0,

        /// <summary>
        /// 执行了无法解析的代码
        /// </summary>
        UnableParseCodeError,

        /// <summary>
        /// 执行堆栈内存溢出
        /// </summary>
        /// <remarks>通常表现为堆栈指针访问堆栈内存数组时索引越界</remarks>
        StackOverflowError,

        /// <summary>
        /// 进行整数运算时除0
        /// </summary>
        DivideZeroError,

        /// <summary>
        /// 指向代码程序的指针在执行时超出代码块范围
        /// </summary>
        CodeOutRange,

        /// <summary>
        /// 执行过程中捕获了虚拟机代码异常或错误
        /// </summary>
        ThrowExceptionError,

        /// <summary>
        /// 执行自定义类型代码时引发的异常或错误
        /// </summary>
        CustomCodeExceptionError,

        /// <summary>
        /// 执行自定义代码时不存在定义
        /// </summary>
        NoCustomCodeExists,

        /// <summary>
        /// 代码执行结束标识符
        /// </summary>
        /// <remarks>
        /// <para>当<see cref="VirtualCPU.Execute(ProgramCode, VirtualStack)"/>函数返回该值时，表示堆栈指针指向了程序代码块的末端，标志着当前堆栈(stack)已将程序(code)运行完毕</para>
        /// <para>
        /// 在编写程序时，可以手动将<see cref="VirtualStack.StackPosition"/>设置为<see cref="ProgramCode.Memory"/>的<see cref="Array.Length"/>以表示程序执行完毕
        /// </para>
        /// </remarks>
        CodeEnded = uint.MaxValue

    }

}
