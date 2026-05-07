using Minokori.Media.Photoshop.Exceptions;
using Minokori.Media.Photoshop.Sections;

namespace Minokori.Media.Photoshop;

internal partial class PhotoshopBinaryReader
    {
    #region 为了方便 PSD 读取, 定义的 property
    /// <summary>
    /// 所属的 PSD 文档的颜色位深度。<para/>
    /// </summary>
    /// <remarks>
    /// 应该为 8.
    /// </remarks>
    public int Depth { get; set; }

    /// <summary>
    /// 持有的对自己所属的 <see cref="PsdDocument"/> 的引用。<para/>
    /// </summary>
    public PsdDocument Document { get; init; }
    #endregion

    /// <summary>
    /// PSD 文件的版本. 存在于 <see cref="FileHeaderSection"/> 中
    /// </summary>
    /// <remarks>
    /// 始终等于 1。如果与此值不匹配，请不要尝试读取文件。PSB 为 2。
    /// </remarks>
    public int Version
        {
        get => field;
        set
            {
            if (value is not 1 and not 2)
                {
                throw new InvalidFormatException(
                    "Invalid PSD version. Only version 1 and 2 are supported."
                );
                }

            field = value;
            }
        } = 1;

    /// <summary>
    /// 字节流当前的位置。<para/>
    /// </summary>
    public long Position
        {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
        }

    /// <summary>
    /// 字节流的长度
    /// </summary>
    public long Length => BaseStream.Length;

    /// <summary>
    /// 字节流
    /// </summary>
    public Stream Stream => BaseStream;

    }
