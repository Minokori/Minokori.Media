using Emgu.CV.Structure;
using Minokori.Media.Photoshop;
using Minokori.Media.Photoshop.Extensions;

namespace Minokori.Media.Demo.Godot;

public partial class Character
    {
    /// <summary>
    /// 角色印象色, 默认值为白色 (#FFFFFF)
    /// </summary>
    public string ImpressionColor { get; init; } = "#FFFFFF";

    public int Width => File.Width;
    public int Height => File.Height;

    private PsdDocument File { get; init; }

    /// <summary>
    /// 从图片源文件中裁取头像的定位点. (单位: 像素) (横向定位点, 纵向定位点)
    /// </summary>
    internal ValueTuple<int, int> AvatarAnchorPosition { get; init; }

    /// <summary>
    /// 从图片源文件中裁取头像的大小. (单位: 像素)
    /// </summary>
    internal ValueTuple<int, int> AvatarAnchorSize { get; init; }

    /// <summary>
    /// 从图片源文件中裁取立绘的大小 (单位: 像素) (宽, 高)
    /// </summary>
    /// <remarks>
    /// 默认留空即为图片源文件大小
    /// </remarks>
    internal ValueTuple<int, int> TachieAnchorSize { get; init; }

    /// <summary>
    /// 从图像源文件中裁取立绘的定位点. (单位: 像素) (横向定位点, 纵向定位点)
    /// </summary>
    /// <remarks>
    /// 默认留空即为 (0, 0)
    /// </remarks>
    internal ValueTuple<int, int> TachieAnchorPosition { get; init; }

    /// <summary>
    /// 差分
    /// </summary>
    /// <remarks>
    /// 差分通过 psd 的不同图层实现。<para/>
    /// <code>
    /// {[差分名]: [图层标号,...]}
    /// </code>
    /// </remarks>
    internal Dictionary<string, IEnumerable<int>> SabennDictionary;

    public bool SetSabenn(string key, IEnumerable<int> value)
        {

        switch (SabennDictionary.ContainsKey(key), value.Any())
            {
            case (false, false):
                return false;
            case (false, true):
                SabennDictionary.Add(key, value);
                return true;
            case (true, false):
                _ = SabennDictionary.Remove(key);
                return true;
            case (true, true):
                SabennDictionary[key] = value;
                return true;
            }
        }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sabenn"></param>
    /// <returns></returns>
    /// <remarks>
    /// 在DEBUG模式下, 输入 "random" 以测试功能是否正常
    /// </remarks>
    public Character this[string sabenn]
        {
        get
            {
# if DEBUG
            if (sabenn == "random")
                {
                var random = new Random();
                var layerCount = File.ImageLayers.Count();
                var randomLayerIndices = Enumerable.Range(0, layerCount).Where(_ => random.NextDouble() < 0.5);
                return this[randomLayerIndices];
                }
#endif
            var has = SabennDictionary.TryGetValue(sabenn, out var value);
            if (!has) { return this; }

            foreach (var (layer, idx) in File.ImageLayers.Select((layer, idx) => (layer, idx)))
                {
                layer.IsVisible = value.Contains(idx);
                }

            var image = Image.CreateFromData(
                File.Width,
                File.Height,
                false,
                Image.Format.Rgba8,
                File.MergeVisibleChildsToCVImage().Convert<Rgba, byte>().Bytes
            );
            Texture.Update(image);
            return this;
            }
        }
    public Character this[IEnumerable<int> index]
        {
        get
            {
            foreach (var (layer, idx) in File.ImageLayers.Select((layer, idx) => (layer, idx)))
                {
                layer.IsVisible = index.Contains(idx);
                }

            var image = Image.CreateFromData(
                File.Width,
                File.Height,
                false,
                Image.Format.Rgba8,
                File.MergeVisibleChildsToCVImage().Convert<Rgba, byte>().Bytes
            );
            Texture.Update(image);
            return this;
            }
        }
    }
