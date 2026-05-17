using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Minokori.Media.Photoshop.Interfaces;

namespace Minokori.Media.Photoshop.Extensions;

public static partial class PsdLayerExtensions
    {
    extension(IPsdLayer layer)
        {
        /// <summary>
        /// 获得包含图像数据的所有图层的列表，按图层顺序(从上到下)排列.
        /// </summary>
        public IPsdLayer[] ImageLayers
            {
            get
                {
                List<IPsdLayer> layers = [];
                foreach (var child in layer.Childs)
                    {
                    if (child.Childs.Length > 0)
                        {
                        layers.AddRange(child.ImageLayers);
                        }
                    else
                        {
                        layers.Add(child);
                        }
                    }

                return [.. layers];
                }
            }


        /// <summary>
        /// 混合图层的通道数据，将所有通道的数据合并为一个字节数组。
        /// </summary>
        /// <returns>[(B,G,R,A), (B,G,R,A), B,G,R,A), ..., ]</returns>
        /// <remarks>
        /// 返回的格式为 <c>BGRA8888</c>, <b><u>没有</u></b> 预乘 Alpha 通道。
        /// </remarks>
        public byte[] MergeChannels()
            {
            if (layer.MergedImage.Length != 0)
                return layer.MergedImage;

            var buffer = new byte[layer.Width * layer.Height * 4];
            var index = 0;
            foreach (var pixel in layer.GetBgraPixels())
                {
                buffer[index++] = pixel[0]; // Blue
                buffer[index++] = pixel[1]; // Green
                buffer[index++] = pixel[2]; // Red
                buffer[index++] = pixel[3]; // Alpha
                }

            layer.MergedImage = buffer;
            return buffer;
            }

        /// <summary>
        /// 混合图层的通道数据，将所有通道的数据合并为一个 OpenCV 图像.
        /// </summary>
        public Image<Bgra, byte> MergeChannelsToCVImage()
            {
            var buffer = layer.MergeChannels();
            var cvImage = new Image<Bgra, byte>(layer.Width, layer.Height) { Bytes = buffer };
            return cvImage;
            }



        /// <summary>
        /// 将所有可见的子图层的图像数据混合到一个 OpenCV 图像中，按照图层顺序(从上到下)进行 alpha 混合。
        /// </summary>
        /// <returns>
        /// 拼合后的图像数据.
        /// </returns>
        /// <remarks>
        /// 要访问拼合后的原始图像字节数据, 参照<see cref="PsdLayer.MergedImage"/>, 其格式为 <c>BGRA8888</c>, <b><u>没有</u></b> 预乘 Alpha 通道。
        /// </remarks>
        public Image<Bgra, byte> MergeVisibleChildsToCVImage()
            {
            // image是 HWC
            var image = new Image<Bgra, byte>(layer.Width, layer.Height);
            foreach (var (sublayer, idx) in layer.ImageLayers.Select((layer, idx) => (layer, idx)).Where(x => x.layer.IsVisible).Reverse())
                {
                _ = image.AddImage(sublayer.MergeChannelsToCVImage(), sublayer.Left, sublayer.Top);
                }

            layer.MergedImage = image.Bytes;
            return image;
            }

        /// <summary>
        /// used for debug.
        /// </summary>
        /// <param name="groupDepth"></param>
        /// <returns></returns>
        public string GetStructureString(int groupDepth = 0)
            {
            StringBuilder stringBuilder = new();
            if (layer is PsdDocument)
                {
                groupDepth += 1;
                }

            foreach (var childLayer in layer.Childs)
                {
                _ =
                    childLayer.Childs.Length == 0
                        ? stringBuilder.Append(' ', groupDepth * 4).AppendLine($"{childLayer.Name}, ({childLayer.IsVisible})")
                        : stringBuilder
                            .Append(' ', groupDepth * 4)
                            .AppendLine($"LayerSet {childLayer.Name}:")
                            .Append(childLayer.GetStructureString(groupDepth + 1));
                }

            return stringBuilder.ToString();
            }
        }
    }
