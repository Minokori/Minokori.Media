namespace Minokori.Media.Photoshop.Extensions;

internal static class ReaderUtilExtensions
    {

    extension(int value)
        {
        /// <summary>
        /// 填充到偶数
        /// </summary>
        /// <returns></returns>
        internal int PadToEven()
            {
            value += value % 2;
            return value;
            }
        }
    extension(long value)
        {
        /// <summary>
        /// 填充到偶数
        /// </summary>
        /// <returns></returns>
        internal long PadToEven()
            {
            value += value % 2;
            return value;
            }

        /// <summary>
        /// 填充到四的倍数
        /// </summary>
        /// <returns></returns>
        internal long PadToFour() => (value + 3) & (~3);
        }
    }
