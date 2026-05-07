using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Minokori.Media.Photoshop;

internal partial class PhotoshopBinaryReader
    {
    /// <summary>
    /// 缓存buffer, 用于大端读取基本数据类型。<para/>
    /// </summary>
    private readonly byte[] scratchBuffer = new byte[8];
    /// <summary>
    /// ArrayPool 用于缓冲区租赁。<para/>
    /// </summary>
    private readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

    #region 由于 PSD 存储采用大端, 所以需要重载读取方法 (BinaryReader使用小端读取)
    public override double ReadDouble()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 8)); //double 占用 8 字节
        return BinaryPrimitives.ReadDoubleBigEndian(scratchBuffer.AsSpan(0, 8));
        }

    public override short ReadInt16()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 2));
        return BinaryPrimitives.ReadInt16BigEndian(scratchBuffer.AsSpan(0, 2));
        }

    public override int ReadInt32()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 4)); //int 占用 4 字节
        return BinaryPrimitives.ReadInt32BigEndian(scratchBuffer.AsSpan(0, 4));
        }

    public override long ReadInt64()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 8)); //long 占用 8 字节
        return BinaryPrimitives.ReadInt64BigEndian(scratchBuffer.AsSpan(0, 8));
        }

    public override ushort ReadUInt16()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 2)); //ushort 占用 2 字节
        return BinaryPrimitives.ReadUInt16BigEndian(scratchBuffer.AsSpan(0, 2));
        }

    public override uint ReadUInt32()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 4)); //uint 占用 4 字节
        return BinaryPrimitives.ReadUInt32BigEndian(scratchBuffer.AsSpan(0, 4));
        }

    public override ulong ReadUInt64()
        {
        ReadExactly(scratchBuffer.AsSpan(0, 8)); //ulong 占用 8 字节
        return BinaryPrimitives.ReadUInt64BigEndian(scratchBuffer.AsSpan(0, 8));
        }

    public override string ReadString()
        {
        var charNumber = ReadInt32();
        if (charNumber == 0)
            {
            return string.Empty;
            }

        var byteCount = checked(charNumber * 2);
        var buffer = arrayPool.Rent(byteCount);

        ReadExactly(buffer.AsSpan(0, byteCount));
        // 如果末尾两个字节为 0，说明有 NULL 结尾
        if (byteCount >= 2 && buffer[byteCount - 1] == 0 && buffer[byteCount - 2] == 0)
            {
            charNumber--;
            }

        var result = Encoding.BigEndianUnicode.GetString(buffer, 0, charNumber * 2);
        arrayPool.Return(buffer);
        return result;
        }

    public override char ReadChar() => (char)ReadByte();
    #endregion

    #region 其他读取基本数据类型的方法, 不存在于 BinaryReader 中
    /// <summary>
    /// 读取指定数量的 double,并返回一个 double 数组。
    /// </summary>
    /// <param name="count">读取 double 数据的个数</param>
    /// <returns>长度为 <paramref name="count"/> 的 double 数组</returns>
    public double[] ReadDoubles(int count)
        {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        if (count == 0) return [];

        var bytesNeeded = checked(count * 8);
        var byteBuffer = arrayPool.Rent(bytesNeeded);

        ReadExactly(byteBuffer.AsSpan(0, bytesNeeded));
        var result = new double[count];
        var span = byteBuffer.AsSpan(0, bytesNeeded);
        for (var i = 0; i < count; i++)
            {
            var chunk = span.Slice(i * 8, 8);
            result[i] = BinaryPrimitives.ReadDoubleBigEndian(chunk);
            }

        arrayPool.Return(byteBuffer);
        return result;
        }
    #endregion
    }
