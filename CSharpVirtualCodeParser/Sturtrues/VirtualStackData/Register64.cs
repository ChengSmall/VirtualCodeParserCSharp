using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 寄存器集
    /// </summary>
    public unsafe struct Register64
    {

        #region 参数

        #region 8b
        /// <summary>
        /// 16个64位寄存器
        /// </summary>
        public fixed ulong reg64[16];
        #endregion

        #region 4b
        /// <summary>
        /// 16个32位寄存器
        /// </summary>
        public fixed uint reg32[16];
        #endregion

        #region 2b
        /// <summary>
        /// 16个16位寄存器
        /// </summary>
        public fixed ushort reg16[16];
        #endregion

        #region 1b
        /// <summary>
        /// 16个8位寄存器
        /// </summary>
        public fixed byte reg8[16];
        #endregion

        #endregion

    }

    /// <summary>
    /// 额外缓存值
    /// </summary>
    public unsafe struct RegisterOther
    {

        /// <summary>
        /// 8个连续的32位缓存值
        /// </summary>
        public fixed int buffer[8];

    }

}
