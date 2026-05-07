using Minokori.Media.Photoshop.Specifications;

namespace Minokori.Media.Photoshop.Extensions;
// 该文件包括 PhotoshopBinaryReader 读取channel信息相关的扩展方法
internal static partial class PhotoshopBinaryReaderExtension
    {
    /// <summary>
    /// 解压RLE算法
    /// </summary>
    /// <param name="src">源</param>
    /// <param name="unpackedLength">目标长度</param>
    /// <exception cref="Exception"></exception>
    private static byte[] DecodeRLE(byte[] src, int unpackedLength)
        {
        var dst = new byte[unpackedLength];
        var index = 0;
        var num2 = 0;
        var num5 = unpackedLength;
        var num6 = src.Length;
        int num3;
        while ((num5 > 0) && (num6 > 0))
            {
            num3 = src[index++];
            num6--;
            if (num3 != 0x80)
                {
                if (num3 > 0x80)
                    {
                    num3 -= 0x100;
                    }

                if (num3 < 0)
                    {
                    num3 = 1 - num3;
                    if (num6 == 0)
                        {
                        throw new Exception("Input buffer exhausted in replicate");
                        }

                    if (num3 > num5)
                        {
                        throw new Exception(
                            string.Format(
                                "Overrun in packedBits replicate of {0} chars",
                                num3 - num5
                            )
                        );
                        }

                    byte num4 = src[index];
                    while (num3 > 0)
                        {
                        if (num5 == 0)
                            {
                            break;
                            }

                        dst[num2++] = num4;
                        num5--;
                        num3--;
                        }

                    if (num5 > 0)
                        {
                        index++;
                        num6--;
                        }

                    continue;
                    }

                num3++;
                while (num3 > 0)
                    {
                    if (num6 == 0)
                        {
                        throw new Exception("Input buffer exhausted in copy");
                        }

                    if (num5 == 0)
                        {
                        throw new Exception("Output buffer exhausted in copy");
                        }

                    dst[num2++] = src[index++];
                    num5--;
                    num6--;
                    num3--;
                    }
                }
            }

        if (num5 > 0)
            {
            for (num3 = 0; num3 < num6; num3++)
                {
                dst[num2++] = 0;
                }
            }

        return dst;
        }

    extension(PhotoshopBinaryReader reader)
        {
        internal byte[] ReadImageStream(int width, int depth, int height, float opacity, CompressionType compressionType, int[] rlePackLengths, int? startPosition = null)
            {
            // 初始化 Data 的大小 (实际宽度 * 高度)
            var rowLength = width.ToPitch(depth);
            var Data = new byte[rowLength * height];

            // 如果是 RLE 压缩, 则需要读取每行的长度
            switch (compressionType)
                {
                case CompressionType.Raw:
                    {
                    //直接将数据读入 Data
                    _ = reader.Read(Data, 0, Data.Length);
                    break;
                    }
                case CompressionType.RLE:
                    {
                    //逐行读取
                    for (var rowIndex = 0; rowIndex < height; rowIndex++)
                        {
                        //读取该行压缩后的数据
                        var packedRowData = reader.ReadBytes(rlePackLengths[rowIndex]);

                        //解压
                        var rowData = DecodeRLE(packedRowData, rowLength);

                        //将解压后的数据写入 Data
                        for (var j = 0; j < rowLength; j++)
                            {
                            Data[(rowIndex * rowLength) + j] = (byte)(rowData[j] * opacity);
                            }
                        }

                    break;
                    }
                }

            return Data;
            }
        }
    }