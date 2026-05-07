using Minokori.Media.Photoshop.Specifications;

namespace Minokori.Media.Photoshop;

public sealed partial class Channel(ChannelType type, int width, int height, int depth)
    {
    #region 元数据
    /// <summary>
    /// 通道颜色类型
    /// </summary>
    public ChannelType Type { get; init; } = type;

    /// <summary>
    /// 通道的宽度 (像素)
    /// </summary>
    public int Width { get; init; } = width;

    /// <summary>
    /// 通道的高度 (像素)
    /// </summary>
    public int Height { get; init; } = height;

    /// <summary>
    /// 通道位深度 (与每个像素数据字节数有关)<para/>
    /// </summary>
    public int Depth { get; init; } = depth;
    #endregion


    #region 像素数据
    /// <summary>
    /// 从左到右, 从上到下排列的像素数据. 每一个像素的数据由若干位(bit)组成(与<see cref="Depth"/>有关)<para/>
    /// </summary>
    /// <remarks>
    /// Data[行索引x * 行长度(宽度Width) + y] = 图片 (x,y) 处 的通道像素值
    /// </remarks>
    public byte[] Data { get; internal set; } = [];

    /// <summary>
    /// 透明度, 0-1 (完全不透明)
    /// </summary>
    public float Opacity { get; set; } = 1.0f;

    #endregion
    }
