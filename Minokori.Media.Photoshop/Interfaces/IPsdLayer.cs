
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Interfaces;

/// <summary>
/// Photoshop 图层接口，包括 <see cref="IChannel"/> 和其他相关属性"/>
/// </summary>
public interface IPsdLayer
    {
    #region 元数据(图层信息)
    /// <summary>
    /// 图层的蒙版模式
    /// </summary>
    BlendMode BlendMode { get; }

    /// <summary>
    /// 图层的下边距
    /// </summary>
    int Bottom { get; }

    /// <summary>
    /// 数据位深度，通常为 8 或 16
    /// </summary>
    int Depth { get; }

    /// <summary>
    /// 是否有图像数据, 由 图层 和 文档 对象实现
    /// </summary>
    bool HasImage { get; }

    /// <summary>
    /// 是否有蒙版
    /// </summary>
    bool HasMask { get; }

    /// <summary>
    /// 图像高度(像素)
    /// </summary>
    int Height { get; }

    bool IsClipping { get; }

    /// <summary>
    /// 图层的左边距
    /// </summary>
    int Left { get; }

    /// <summary>
    /// 图层名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 图像的透明度
    /// </summary>
    float Opacity { get; }

    /// <summary>
    /// 图层的 属性
    /// </summary>
    JObject Resources { get; }

    /// <summary>
    /// 图层的右边距
    /// </summary>
    int Right { get; }

    /// <summary>
    /// 图层的上边距
    /// </summary>
    int Top { get; }

    /// <summary>
    /// 图像宽度(像素
    /// </summary>
    int Width { get; }

    bool IsVisible { get; set; }
    #endregion 元数据(图层信息)

    #region 引用信息
    /// <summary>
    /// 图层的子图层
    /// </summary>
    /// <remarks>
    /// 由于图层组会被视为一个虚拟图层，因此会有可能存在子图层的情况。
    /// </remarks>
    IPsdLayer[] Childs { get; }

    /// <summary>
    /// 图层所属于的文档对象
    /// </summary>
    PsdDocument Document { get; }

    /// <summary>
    /// 所链接图层的图层信息
    /// </summary>
    ILinkedLayer? LinkedLayer { get; }

    /// <summary>
    /// 图层的父图层
    /// </summary>
    IPsdLayer? Parent { get; }

    #endregion 引用信息

    #region 图像信息

    /// <summary>
    /// 图像的颜色通道
    /// </summary>
    Channel[] Channels { get; }

    /// <summary>
    /// 由图层各通道合并而成的图像数据. 格式为 Bgra8888
    /// </summary>
    byte[] MergedImage { get; set; }

    #endregion 图像信息

    /// <summary>
    /// 由左往右,由上往下地 获得图层的所有像素数据
    /// </summary>
    /// <param name="whereToSetTransparent">一个委托, 接受一个 bgr 像素数组, 返回该像素值是否要被设置为透明</param>
    /// <returns></returns>
    /// <remarks>
    /// 像素数据的格式为 [Blue, Green, Red, Alpha].<para/>
    /// 若图层没有 Alpha 通道，将根据传入的委托将指定的颜色设置为透明 (默认为白色(255,255,255) )。
    /// </remarks>
    IEnumerable<byte[]> GetBgraPixels(Func<byte[], bool>? whereToSetTransparent = null)
        {
        // bgr 像素
        var PixelData = Channels[^1].Data
            .Zip(Channels[^2].Data, Channels[^3].Data)
            .Select(i => new[] { i.First, i.Second, i.Third });

        // 添加原有的 Alpha 通道
        if (Channels.Length >= 4)
            {
            PixelData = Channels[^4].Data.Zip(PixelData, (a, rgb) => rgb.Append(a).ToArray());
            }
        // 为了兼容没有 Alpha 通道的情况, 根据委托设置透明度
        else
            {
            whereToSetTransparent ??= (bgr) => bgr.All(i => i == 255);
            PixelData = PixelData.Select(bgr =>
                bgr.Append(whereToSetTransparent(bgr) ? (byte)0 : (byte)255).ToArray()
            );
            }

        return PixelData;
        }

    /// <summary>
    /// 递归遍历指定 <see cref="IPsdLayer"/> 及其所有子层，返回包含自身及所有后代层的枚举序列。
    /// </summary>
    /// <param name="filter">筛选委托, 接受一个 <see cref="IPsdLayer"/> 参数, 返回一个布尔值, 指示该层是否返回</param>
    /// <returns></returns>
    IEnumerable<IPsdLayer> Descendants(Func<IPsdLayer, bool>? filter = null)
        {
        filter ??= _ => true;
        if (filter(this) == true)
            {
            yield return this;
            }

        foreach (var item in Childs)
            {
            if (filter(item) == false)
                continue;

            yield return item;

            foreach (var child in item.Descendants(filter))
                {
                yield return child;
                }
            }
        }
    }
