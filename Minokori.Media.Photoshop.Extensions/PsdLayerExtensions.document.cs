using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

public static partial class PsdLayerExtensions
    {
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
