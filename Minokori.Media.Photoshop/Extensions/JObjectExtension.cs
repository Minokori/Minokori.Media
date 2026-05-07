using Minokori.Media.Photoshop.Exceptions;
using Minokori.Media.Photoshop.Specifications;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

internal static class JObjectExtensions
    {
    /// <summary>
    /// 对 <see cref="JObject"/> 的扩展方法"/>
    /// </summary>
    /// <param name="jObject"> <see cref="JObject"/> 对象 </param>
    extension(JObject jObject)
        {
        /// <summary>
        /// 通过 jsonPath 表达式获得 json 对象的值。
        /// </summary>
        /// <typeparam name="T">要获得的值类型</typeparam>
        /// <param name="jsonPath">待查找的若干 jsonPath</param>
        /// <returns></returns>
        /// <remarks>
        /// jsonPath 是一个字符串数组，表示要依次查找的属性路径。一旦在某个路径上找到值，就会返回该值。<para/>
        /// 若没有找到任何值，则返回 <typeparamref name="T"/> 类型的默认值。因此, 可能会返回 null
        /// </remarks>
        public T? GetValue<T>(params string[] jsonPath)
            {
            foreach (var property in jsonPath)
                {
                var token = jObject.SelectToken(property);
                if (token == null)
                    continue;
                else
                    return token.Value<T>();
                }

            return default;
            }


        /// <summary>
        /// 从 LayerRecord 解析通道信息。
        /// </summary>
        /// <returns>(通道类型, 宽度, 高度) 构成的列表</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="InvalidFormatException"></exception>
        internal List<(ChannelType type, int width, int height)> ToChannelInfo()
            {
            if (!jObject.Path.Contains("LayerRecord"))
                {
                throw new InvalidOperationException("Only LayerRecord can be parsed to Channel Info.");
                }

            var width = jObject.Value<int>("Right") - jObject.Value<int>("Left");
            var height = jObject.Value<int>("Bottom") - jObject.Value<int>("Top");

            // 图片尺寸过大则抛出异常
            if (width > 0x3000 || height > 0x3000)
                {
                throw new InvalidFormatException($"Invalid photoshop channel size ({width}, {height})");
                }

            List<(ChannelType type, int width, int height)> channelInfo = [];

            foreach (var item in jObject.Value<JArray>("ChannelID")!.Values<string>())
                {
                if (item == "Mask")
                    {
                    var maskWidth = jObject.SelectToken("Mask.Width")?.Value<int>() ?? width;
                    var maskHeight = jObject.SelectToken("Mask.Height")?.Value<int>() ?? height;
                    channelInfo.Add((ChannelType.Mask, maskWidth, maskHeight));
                    }
                else
                    {
                    channelInfo.Add((Enum.Parse<ChannelType>(item!), width, height));
                    }
                }

            return channelInfo;

            }




        /// <summary>
        /// 通过 jsonPath 表达式检查 json 对象是否包含某个值。
        /// </summary>
        /// <param name="jsonPath">jsonPath 表达式</param>
        /// <returns>json 对象下是否存在该属性</returns>
        public bool Contains(string jsonPath) => jObject.SelectToken(jsonPath) != null;
        }
    }