using System;

namespace Cheng.VirtualCodeParser
{

    /// <summary>
    /// 运算模块
    /// </summary>
    public unsafe static class ALUS
    {

        /// <summary>
        /// 运算类型
        /// </summary>
        public enum Type : byte
        {
            #region 逻辑
            /// <summary>
            /// 按位非
            /// </summary>
            Not,
            /// <summary>
            /// 按位或
            /// </summary>
            Or,
            /// <summary>
            /// 按位与
            /// </summary>
            And,
            /// <summary>
            /// 按位异或
            /// </summary>
            Xor,
            /// <summary>
            /// 按位同或
            /// </summary>
            NXor,
            /// <summary>
            /// 逻辑右移运算
            /// </summary>
            BinRightMove,
            /// <summary>
            /// 逻辑左移运算
            /// </summary>
            BinLeftMove,
            #endregion

            #region 算数
            /// <summary>
            /// 算数右移
            /// </summary>
            NumRightMove,
            /// <summary>
            /// 算术左移
            /// </summary>
            NumLeftMove,

            Neg,
            Add,
            Sub,
            Mult,
            Dev,
            Mod,
            Abs,
            Max,
            Min,
            #endregion

            #region 比较
            Equals,
            NotEquals,
            Less,
            Greater,
            LessOrEqual,
            GreaterOrEqual,
            IsNotZero,
            #endregion

            #region 高级运算
            /// <summary>
            /// float强转int
            /// </summary>
            FloatToInt,
            /// <summary>
            /// int强转float
            /// </summary>
            IntToFloat,
            /// <summary>
            /// 四舍五入
            /// </summary>
            Round,
            /// <summary>
            /// 取小数位
            /// </summary>
            DecimalPlaces,

            Sin,
            Cos,
            Tan,
            ASin,
            ACos,
            ATan,
            Sqrt,
            Pow,
            Floor,
            Exp,
            Ceiling

            #endregion

        }

        /// <summary>
        /// 计算32为整形
        /// </summary>
        /// <param name="type">计算类型</param>
        /// <param name="a">第1个值</param>
        /// <param name="b">第2个值</param>
        /// <param name="signed">是否计算符号位</param>
        /// <param name="re">返回值</param>
        /// <returns>错误代码</returns>
        public static VirtualErrorCode Invoke32(Type type, uint a, uint b, bool signed, out uint re)
        {

            float fa;
            bool judge = default;
            re = default;

            switch (type)
            {
                case Type.Not:
                    re = ~a;
                    break;
                case Type.Or:
                    re = a | b;
                    break;
                case Type.And:
                    re = a & b;
                    break;
                case Type.Xor:
                    re = a ^ b;
                    break;
                case Type.NXor:
                    re = ~(a ^ b);
                    break;
                case Type.BinRightMove:
                    re = a >> (int)b;
                    break;
                case Type.BinLeftMove:
                    re = a << (int)b;
                    break;
                case Type.NumRightMove:
                    if(signed) re = (uint)((int)a >> (int)b);
                    else re = (a >> (int)b);
                    break;
                case Type.NumLeftMove:
                    if (signed) re = (uint)((int)a << (int)b);
                    else re = (a << (int)b);
                    break;
                case Type.Neg:
                    re = (uint)-(*(int*)&a);
                    break;
                case Type.Add:
                    if (signed) re = (uint)((int)a + (int)b);
                    else re = a + b;
                    break;
                case Type.Sub:
                    if (signed) re = (uint)((int)a - (int)b);
                    else re = a - b;
                    break;
                case Type.Mult:
                    if (signed) re = (uint)((int)a * (int)b);
                    else re = a * b;
                    break;
                case Type.Dev:
                    if(b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    if (signed) re = (uint)((int)a / (int)b);
                    else re = a / b;
                    break;
                case Type.Mod:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    if (signed) re = (uint)((int)a % (int)b);
                    else re = a % b;
                    break;
                case Type.Abs:
                    if (signed) re = a;
                    else re = (uint)Math.Abs(*(int*)&a);
                    break;
                case Type.Max:
                    if (signed) re = (uint)Math.Max(*(int*)&a, *(int*)&b);
                    else re = Math.Max(a, b);
                    break;
                case Type.Min:
                    if (signed) re = (uint)Math.Min(*(int*)&a, *(int*)&b);
                    else re = Math.Min(a, b);
                    break;
                case Type.Equals:
                    re = (uint)((a == b) ? 1 : 0);
                    break;
                case Type.NotEquals:
                    re = (uint)((a != b) ? 1 : 0);
                    break;
                case Type.Less:
                    if(signed) judge = ((int)a < (int)b);
                    else judge = (a < b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.Greater:
                    if (signed) judge = ((int)a > (int)b);
                    else judge = (a > b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.LessOrEqual:
                    if (signed) judge = ((int)a <= (int)b);
                    else judge = (a <= b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.GreaterOrEqual:
                    if (signed) judge = ((int)a >= (int)b);
                    else judge = (a >= b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.IsNotZero:
                    judge = a != 0;
                    break;
                case Type.FloatToInt:
                    fa = *(float*)&a;
                    if(signed) re = (uint)((int)fa);
                    else re = (uint)fa;
                    break;
                case Type.IntToFloat:
                    if (signed) fa = (float)a;
                    else fa = (float)((int)a);
                    re = *(uint*)&fa;
                    break;               

                default:
                    re = default;
                    return VirtualErrorCode.UnableParseCodeError;
            }

            return VirtualErrorCode.NoneError;
        }

        /// <summary>
        /// 计算浮点值，浮点值无法进行按位逻辑运算
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="a">值1</param>
        /// <param name="b">值2</param>
        /// <param name="re">返回值</param>
        /// <returns>错误代码</returns>
        public static VirtualErrorCode InvokeF32(Type type, float a, float b, out float re)
        {
            int ia;
            re = default;
            bool judge = default;
            switch (type)
            {
                case Type.Neg:
                    re = -a;
                    break;
                case Type.Add:
                    re = a + b;
                    break;
                case Type.Sub:
                    re = a - b;
                    break;
                case Type.Mult:
                    re = a * b;
                    break;
                case Type.Dev:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    re = a / b;
                    break;
                case Type.Mod:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    re = a % b;
                    break;
                case Type.Abs:
                    re = Math.Abs(a);
                    break;
                case Type.Max:
                    re = Math.Max(a, b);
                    break;
                case Type.Min:
                    re = Math.Min(a, b);
                    break;
                case Type.Equals:
                    judge = a == b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.NotEquals:
                    judge = a != b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.Less:
                    judge = a < b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.Greater:
                    judge = a > b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.LessOrEqual:
                    judge = a <= b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.GreaterOrEqual:
                    judge = a >= b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.IsNotZero:
                    judge = (*(int*)&a) != 0;
                    re = judge ? 1U : 0U;
                    break;
                case Type.FloatToInt:
                    ia = (int)a;
                    re = *(float*)&ia;
                    break;
                case Type.IntToFloat:
                    ia = *(int*)&a;
                    re = ia;
                    break;
                case Type.Round:
                    re = (float)Math.Round(a);
                    break;
                case Type.DecimalPlaces:
                    re = (float)(a - Math.Floor(a));
                    break;
                case Type.Sin:
                    re = (float)Math.Sin(a);
                    break;
                case Type.Cos:
                    re = (float)Math.Cos(a);
                    break;
                case Type.Tan:
                    re = (float)Math.Tan(a);
                    break;
                case Type.ASin:
                    re = (float)Math.Asin(a);
                    break;
                case Type.ACos:
                    re = (float)Math.Acos(a);
                    break;
                case Type.ATan:
                    re = (float)Math.Atan(a);
                    break;
                case Type.Sqrt:
                    re = (float)Math.Sqrt(a);
                    break;
                case Type.Pow:
                    re = (float)Math.Pow(a, b);
                    break;
                case Type.Floor:
                    re = (float)Math.Floor(a);
                    break;
                case Type.Exp:
                    re = (float)Math.Exp(a);
                    break;
                case Type.Ceiling:
                    re = (float)Math.Ceiling(a);
                    break;

                default:
                    re = default;
                    return VirtualErrorCode.UnableParseCodeError;
            }

            return VirtualErrorCode.NoneError;
        }

        /// <summary>
        /// 计算64位整形
        /// </summary>
        /// <param name="type">计算类型</param>
        /// <param name="a">第1个值</param>
        /// <param name="b">第2个值</param>
        /// <param name="signed">是否计算符号位</param>
        /// <param name="re">返回值</param>
        /// <param name="judge">条件运算返回值</param>
        /// <returns>错误代码</returns>
        public static VirtualErrorCode Invoke64(Type type, ulong a, ulong b, bool signed, out ulong re)
        {
            double fa;
            bool judge = default;
            re = default;

            switch (type)
            {
                case Type.Not:
                    re = ~a;
                    break;
                case Type.Or:
                    re = a | b;
                    break;
                case Type.And:
                    re = a & b;
                    break;
                case Type.Xor:
                    re = a ^ b;
                    break;
                case Type.NXor:
                    re = ~(a ^ b);
                    break;
                case Type.BinRightMove:
                    re = a >> (int)b;
                    break;
                case Type.BinLeftMove:
                    re = a << (int)b;
                    break;
                case Type.NumRightMove:
                    if (signed) re = (ulong)((long)a >> (int)b);
                    else re = (a >> (int)b);
                    break;
                case Type.NumLeftMove:
                    if (signed) re = (ulong)((long)a << (int)b);
                    else re = (a << (int)b);
                    break;
                case Type.Neg:
                    re = (ulong)-(*(int*)&a);
                    break;
                case Type.Add:
                    if (signed) re = (ulong)((long)a + (long)b);
                    else re = a + b;
                    break;
                case Type.Sub:
                    if (signed) re = (ulong)((long)a - (long)b);
                    else re = a - b;
                    break;
                case Type.Mult:
                    if (signed) re = (ulong)((long)a * (long)b);
                    else re = a * b;
                    break;
                case Type.Dev:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    if (signed) re = (ulong)((long)a / (long)b);
                    else re = a / b;
                    break;
                case Type.Mod:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    if (signed) re = (ulong)((long)a % (long)b);
                    else re = a % b;
                    break;
                case Type.Abs:
                    if (signed) re = a;
                    else re = (ulong)Math.Abs(*(long*)&a);
                    break;
                case Type.Max:
                    if (signed) re = (ulong)Math.Max(*(long*)&a, *(long*)&b);
                    else re = Math.Max(a, b);
                    break;
                case Type.Min:
                    if (signed) re = (ulong)Math.Min(*(long*)&a, *(long*)&b);
                    else re = Math.Min(a, b);
                    break;
                case Type.Equals:
                    re = (ulong)((a == b) ? 1 : 0);
                    break;
                case Type.NotEquals:
                    re = (ulong)((a != b) ? 1 : 0);
                    break;
                case Type.Less:
                    if (signed) judge = ((long)a < (long)b);
                    else judge = (a < b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.Greater:
                    if (signed) judge = ((long)a > (long)b);
                    else judge = (a > b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.LessOrEqual:
                    if (signed) judge = ((long)a <= (long)b);
                    else judge = (a <= b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.GreaterOrEqual:
                    if (signed) judge = ((long)a >= (long)b);
                    else judge = (a >= b);
                    re = judge ? 1U : 0U;
                    break;
                case Type.IsNotZero:
                    judge = a != 0;
                    re = judge ? 1U : 0U;
                    break;
                case Type.FloatToInt:
                    fa = *(double*)&a;
                    if (signed) re = (ulong)((long)fa);
                    else re = (uint)fa;
                    break;
                case Type.IntToFloat:
                    if (signed) fa = (double)a;
                    else fa = (double)((long)a);
                    re = *(ulong*)&fa;
                    break;

                default:
                    re = default;
                    return VirtualErrorCode.UnableParseCodeError;
            }

            return VirtualErrorCode.NoneError;
        }

        /// <summary>
        /// 计算双浮点值，浮点值无法进行按位逻辑运算
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="a">值1</param>
        /// <param name="b">值2</param>
        /// <param name="re">返回值</param>
        /// <param name="judge">条件运算返回值</param>
        /// <returns>错误代码</returns>
        public static VirtualErrorCode InvokeF64(Type type, double a, double b, out double re)
        {
            long ia;
            re = default;
            bool judge = default;
            switch (type)
            {
                case Type.Neg:
                    re = -a;
                    break;
                case Type.Add:
                    re = a + b;
                    break;
                case Type.Sub:
                    re = a - b;
                    break;
                case Type.Mult:
                    re = a * b;
                    break;
                case Type.Dev:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    re = a / b;
                    break;
                case Type.Mod:
                    if (b == 0)
                    {
                        re = default;
                        return VirtualErrorCode.DivideZeroError;
                    }
                    re = a % b;
                    break;
                case Type.Abs:
                    re = Math.Abs(a);
                    break;
                case Type.Max:
                    re = Math.Max(a, b);
                    break;
                case Type.Min:
                    re = Math.Min(a, b);
                    break;
                case Type.Equals:
                    judge = a == b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.NotEquals:
                    judge = a != b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.Less:
                    judge = a < b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.Greater:
                    judge = a > b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.LessOrEqual:
                    judge = a <= b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.GreaterOrEqual:
                    judge = a >= b;
                    re = judge ? 1U : 0U;
                    break;
                case Type.IsNotZero:
                    judge = (*(long*)&a) != 0;
                    re = judge ? 1U : 0U;
                    break;
                case Type.FloatToInt:
                    ia = (long)a;
                    re = *(double*)&ia;
                    break;
                case Type.IntToFloat:
                    ia = *(long*)&a;
                    re = ia;
                    break;
                case Type.Round:
                    re = Math.Round(a);
                    break;
                case Type.DecimalPlaces:
                    re = (a - Math.Floor(a));
                    break;
                case Type.Sin:
                    re = Math.Sin(a);
                    break;
                case Type.Cos:
                    re = Math.Cos(a);
                    break;
                case Type.Tan:
                    re = Math.Tan(a);
                    break;
                case Type.ASin:
                    re = Math.Asin(a);
                    break;
                case Type.ACos:
                    re = Math.Acos(a);
                    break;
                case Type.ATan:
                    re = Math.Atan(a);
                    break;
                case Type.Sqrt:
                    re = Math.Sqrt(a);
                    break;
                case Type.Pow:
                    re = Math.Pow(a, b);
                    break;
                case Type.Floor:
                    re = Math.Floor(a);
                    break;
                case Type.Exp:
                    re = Math.Exp(a);
                    break;
                case Type.Ceiling:
                    re = Math.Ceiling(a);
                    break;

                default:
                    re = default;
                    return VirtualErrorCode.UnableParseCodeError;
            }

            return VirtualErrorCode.NoneError;
        }

    }

}
