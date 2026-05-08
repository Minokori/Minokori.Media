using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Minokori.Media.Photoshop.Interfaces;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

public static class PsdLayerExtensions
    {
    extension(IPsdLayer layer)
        {
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
        /// 混合图层的通道数据，将所有通道的数据合并为一个字节数组。
        /// </summary>
        public Image<Bgra, byte> MergeChannelsToCVImage()
            {
            var buffer = layer.MergeChannels();
            return new Image<Bgra, byte>(layer.Width, layer.Height) { Bytes = buffer };
            }

        /// <summary>
        /// 获得包含图像数据的所有图层的列表，按图层顺序排列。
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

    extension(PsdDocument document)
        {
        /// <summary>
        /// 获取文档的所有图层的结构字符串
        /// </summary>
        /// <returns></returns>
        public JObject GetCompleteProperties()
            {
            return new()
                {
                ["FileHeader"] = document.FileHeader,
                ["ColorMode"] = document.ColorModeDataSection,
                ["LayerAndMask"] = document.LayerAndMaskSection,
                };
            }
        }
    }
