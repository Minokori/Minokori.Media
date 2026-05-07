using System.Text;
using Minokori.Media.Photoshop.Exceptions;
using Minokori.Media.Photoshop.Extensions;
using Minokori.Media.Photoshop.Specifications;

namespace Minokori.Media.Photoshop;

/// <summary>
/// 读取 PSD 文件的二进制数据流的读取器。<para/>
/// </summary>
/// <param name="stream">PSD 文件的数据流</param>
internal sealed partial class PhotoshopBinaryReader(Stream stream) : BinaryReader(stream)
    {
    #region 以 "ReadAs" 开头的方法, 功能类似于 BinaryReader 的 "Read" 开头方法, 但会返回特定格式的字符串或数据
    /// <summary>
    /// 从流中读取一个 Pascal 字符串，字符串长度由第一个字节指定，后续字节为字符串内容。<para/>
    /// 读取的长度必须是 <paramref name="alignmentSize"/> 的倍数。
    /// </summary>
    /// <param name="alignmentSize">对齐长度</param>
    /// <returns>Pascal 字符串</returns>
    public string ReadAsPascalString(byte alignmentSize = 1)
        {
        var count = ReadByte();
        if (count == 0)
            {
            _ = ReadBytes(alignmentSize - 1); // 至少读取 alignmentSize 个字节, count已经读取了一个字节, 因此再读取 alignmentSize -1 个
            return string.Empty;
            }

        var bytes = ReadBytes(count);
        var text = Encoding.UTF8.GetString(bytes);
        var totalLength = count + 1;
        var padding = (alignmentSize - (totalLength % alignmentSize)) % alignmentSize;
        //count += (byte)(alignmentSize - ((count + 1) % alignmentSize)); // 确保 count 是 alignmentSize 的倍数
        if (padding > 0)
            {
            _ = ReadBytes(padding); // 读取填充字节
            }

        //Debug.WriteLine(Encoding.Default.GetString(bytes));
        return text;
        }

    /// <summary>
    /// 读取指定长度的字节，并以 ASCII 码的形式解析
    /// </summary>
    /// <param name="length">要读取的字节/字符串长度</param>
    /// <returns>长度为 <paramref name="length"/> 的字符串</returns>
    public string ReadAsAscii(int length)
        {
        var bytes = ReadBytes(length);
        return Encoding.ASCII.GetString(bytes);
        }

    /// <summary>
    /// 读取 4 字节的 ASCII 字符串，通常用于读取 PSD 文件类型标识符
    /// </summary>
    /// <returns></returns>
    public string ReadAsType() => ReadAsAscii(4);

    /// <summary>
    /// 读取 Descripter 的 Key<para/>
    /// 读取一个 <see cref="int"/>, 若为0, 则后续是一个 长度为4 的ascii 字符串<para/>
    /// 若不为零, 则后续是长度为该int 的 ascii 字符串
    /// </summary>
    /// <returns></returns>
    public string ReadAsKey()
        {
        var length = ReadInt32();
        length = (length > 0) ? length : 4;
        return ReadAsAscii(length);
        }

    /// <summary>
    /// 根据 PSD 文件头内的 DescriptorVersion 读取一个 <see cref="int"/> 或 <see cref="long"/>, 通常作为数据结构流的长度.
    /// </summary>
    /// <returns>数据结构的字节长度</returns>
    /// <remarks>
    /// 一般而言, PSD 文件的 DescriptorVersion <b>始终</b>为 1.<para/>
    /// PSB 文件的 DescriptorVersion 为 2, 但本程序集不支持 PSB 文件的读取.<para/>
    /// </remarks>
    public long ReadAsStreamLength() => Version == 1 ? ReadInt32() : ReadInt64();

    /// <summary>
    /// 读取一个 <see cref="short"/> 类型的值, 并作为 <see cref="ColorMode"/> 枚举类型返回。<para/>
    /// </summary>
    /// <returns><see cref="ColorMode"/></returns>
    public ColorMode ReadAsColorMode() => (ColorMode)ReadInt16();

    /// <summary>
    /// 读取为长度为4个字节的 ASCII 字符串, 并作为 <see cref="BlendMode"/> 枚举类型返回。<para/>
    /// </summary>
    /// <returns><see cref="BlendMode"/></returns>
    public BlendMode ReadAsBlendMode() => ReadAsAscii(4).ToBlendMode();

    /// <summary>
    /// 读取一个字节, 并作为 <see cref="LayerFlags"/> 枚举类型返回。<para/>
    /// </summary>
    /// <returns><see cref="LayerFlags"/></returns>
    public LayerFlags ReadAsLayerFlags() => (LayerFlags)ReadByte();

    /// <summary>
    /// 读取一个 <see cref="short"/> 类型的值, 并作为 <see cref="ChannelType"/> 枚举类型返回。<para/>
    /// </summary>
    /// <returns> <see cref="ChannelType"/></returns>
    public ChannelType ReadAsChannelType() => (ChannelType)ReadInt16();

    /// <summary>
    /// 读取一个 <see cref="short"/> 类型的值, 并作为 <see cref="CompressionType"/> 枚举类型返回。<para/>
    /// </summary>
    /// <returns><see cref="CompressionType"/></returns>
    public CompressionType ReadAsCompressionType() => (CompressionType)ReadInt16();

    /// <summary>
    /// 读取一个长度为 <paramref name="height"/> 的整数数组, 该数组表示一个 channel 的 RLE 压缩长度。<para/>
    /// <paramref name="height"/> 参数是 channel 的高度。
    /// </summary>
    /// <param name="height">channel 的高度</param>
    /// <returns></returns>
    /// <remarks>
    /// 读取的字节数和 <see cref="Version"/> 有关:
    /// </remarks>
    public int[] ReadAsChannelRlePackLengths(int height)
        {
        var l = new int[height];

        for (var i = 0; i < height; i++)
            {
            l[i] = Version == 1 ? ReadInt16() : ReadInt32();
            }

        return l;
        }
    #endregion


    #region Verify 方法. 用于验证读取的数据是否符合预期

    /// <summary>
    /// 验证读取的签名是否与预期的签名(之一)匹配。<para/>
    /// 从字节流中读取 4 字节的 ASCII 字符串，并与提供的签名进行比较。<para/>
    /// </summary>
    /// <param name="signature">签名/类型, 每一个都是长度为 4 的 ASCII 字符串</param>
    /// <exception cref="InvalidFormatException"></exception>
    /// <remarks>
    /// <b>注意: 该方法将移动字节流的 Position</b>
    /// </remarks>
    public string VerifySignatureIs(params string[] signature)
        {
        var readSignature = ReadAsType();
        return signature.Contains(readSignature)
            ? readSignature
            : throw new InvalidFormatException(
                $"Expected signature/type is one of {string.Join(", ", signature)}, but got '{readSignature}'."
            );
        }

    /// <summary>
    /// 验证读取的 <see cref="int"/> 或 <see cref="short"/> 值是否与预期的值匹配。<para/>
    /// 从字节流中读取一个 <see cref="int"/> 或 <see cref="short"/> 值，并与提供的值进行比较。<para/>
    /// </summary>
    /// <typeparam name="T"> <see cref="int"/> 或 <see cref="short"/></typeparam>
    /// <param name="value">要验证的值</param>
    /// <exception cref="InvalidFormatException"></exception>
    /// <remarks>
    /// <b>注意: 该方法将移动字节流的 Position</b>
    /// </remarks>
    public int VerifyIntIs<T>(T value)
        where T : struct
        {
        switch (value)
            {
            case int intValue:
                {
                if (intValue == ReadInt32())
                    return intValue;
                break;
                }
            case short shortValue:
                {
                if (shortValue == ReadInt16())
                    return shortValue;
                break;
                }
            }

        throw new InvalidFormatException($"expect {typeof(T)} value {value}");
        }
    #endregion
    }
