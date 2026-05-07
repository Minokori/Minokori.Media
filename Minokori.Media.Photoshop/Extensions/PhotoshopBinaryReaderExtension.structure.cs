using System.Text;
using System.Text.RegularExpressions;
using Minokori.Media.Photoshop.Exceptions;
using Newtonsoft.Json.Linq;

namespace Minokori.Media.Photoshop.Extensions;

internal static partial class PhotoshopBinaryReaderExtension
    {
    extension(PhotoshopBinaryReader reader)
        {
        internal JObject ReadDescriptor()
            {
            JObject obj = [];
            var _ = reader.ReadInt32();// Version, not used

            obj.Add("Name", reader.ReadString());
            obj.Add("ClassID", reader.ReadAsKey());

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                {
                // key, and osType, both 4 bytes
                var key = reader.ReadAsKey();
                var osType = reader.ReadAsType();
                var prop = reader.Read(osType);
                obj.Add(key.Trim(), prop);
                }

            return obj;


            }
        internal JToken Read(string osType)
            {
            return osType switch
                {
                    // 基本数据类型, 返回 JValue
                    "doub" => reader.ReadDouble(),
                    "TEXT" => reader.ReadString(),
                    "long" => reader.ReadInt32(),
                    "bool" => reader.ReadBoolean(),
                    "comp" => reader.ReadInt64(),

                    // 简单数据类型 (没有嵌套包含其他简单类型), 返回 JObject
                    "prop" => reader.ReadProperty(),
                    "UntF" => reader.ReadUnitFloat(),
                    "type" => reader.ReadClass(),
                    "GlbC" => reader.ReadClass(),
                    "Clss" => reader.ReadClass(),
                    "enum" => reader.ReadEnumerate(),
                    "Enmr" => reader.ReadEnumerateReference(), //TODO 和文档描述不一致,修改前和上面一行一样
                    "alis" => reader.ReadAlias(),
                    "rele" => reader.ReadOffset(),
                    "tdta" => reader.ReadEngineData(),


                    // 复杂数据类型 (包含其他简单类型或复杂类型), 返回 JObject
                    //依赖其他的Structure
                    "obj" => reader.ReadReference(),
                    //会导致递归调用
                    "VlLs" => reader.ReadStructureList(),
                    "Objc" => reader.ReadSubDescriptor(),
                    "GlbO" => reader.ReadSubDescriptor(),

                    // 不受支持的

                    "ObAr" => reader.ReadObjectArray(),
                    _ => throw new NotSupportedException(osType),
                    };
            }


        #region 基本数据类型, 返回 JValue

        private JValue ReadDouble()
            {
            return new(reader.ReadDouble());
            }

        private JValue ReadString()
            {
            return new(reader.ReadString());
            }

        private JValue ReadInt32()
            {
            return new(reader.ReadInt32());
            }

        private JValue ReadBoolean()
            {
            return new(reader.ReadBoolean());
            }

        private JValue ReadInt64()
            {
            return new(reader.ReadInt64());
            }
        #endregion

        #region 简单数据类型 (没有嵌套包含其他简单类型), 返回 JObject
        private JObject ReadProperty()
            {
            return new()
                {
                ["Name"] = reader.ReadString(),
                ["ClassID"] = reader.ReadAsKey(),
                ["KeyID"] = reader.ReadAsKey(),
                };
            }

        private JObject ReadUnitFloat()
            {
            return new()
                {
                ["Type"] = Enum.GetName(reader.ReadAsType().ToUnitType()),
                ["Value"] = reader.ReadDouble(),
                };
            }

        private JObject ReadClass()
            {
            return new() { ["Name"] = reader.ReadString(), ["ClassID"] = reader.ReadAsKey() };
            }

        private JObject ReadEnumerate()
            {
            return new() { ["Type"] = reader.ReadAsKey(), ["Enum"] = reader.ReadAsKey() };
            }

        private JObject ReadEnumerateReference()
            {
            return new()
                {
                ["Name"] = reader.ReadString(),
                ["ClassID"] = reader.ReadAsKey(),
                ["TypeID"] = reader.ReadAsKey(),
                ["EnumID"] = reader.ReadAsKey(),
                };
            }

        private JObject ReadAlias()
            {
            return new() { ["Alias"] = reader.ReadAsAscii(reader.ReadInt32()) };
            }

        private JObject ReadOffset()
            {
            return new()
                {
                ["Name"] = reader.ReadString(),
                ["ClassID"] = reader.ReadAsKey(),
                ["Offset"] = reader.ReadInt32(),
                };
            }

        private JObject ReadEngineData()
            {
                {
                var length = reader.ReadInt32();
                var content = reader.ReadBytes(length);
                var result = SplitByUtf16BeBlocks(content);
                var text = "";
                foreach (var block in result)
                    {
                    if (block[0] == 0xFE && block[1] == 0xFF && block[^1] == 10 && block[^2] == 41)
                        {
                        var rawString = Encoding.BigEndianUnicode.GetString(block, 2, block.Length - 4);
                        if (rawString.StartsWith("、。，．・：；？！ー―’”）〕］｝〉》」』】"))
                            {
                            StringBuilder sb = new();
                            foreach (var c in rawString)
                                {
                                _ = sb.AppendFormat("\\u{0:x4}", (int)c);
                                }

                            rawString = sb.ToString();
                            }

                        text += rawString;
                        text = text.TrimEnd('\r', '\n');
                        text += Encoding.ASCII.GetString(block[^2..]);
                        }
                    else
                        {
                        text += Encoding.ASCII.GetString(block);
                        }
                    }

                return ParseToJson(text);
                }

            }
        #endregion

        #region 复杂数据类型 (嵌套包含简单类型或其他复杂数据类型, 返回 JObject)
        private JObject ReadReference()
            {
            JObject reference = [];
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                {
                switch (reader.ReadAsAscii(4))
                    {
                    case "prop":
                        {
                        reference.Add("Property", ReadReference(reader));
                        break;
                        }
                    case "Clss":
                        {
                        reference.Add("Class", ReadClass(reader));
                        break;
                        }
                    case "Enmr":
                        {
                        reference.Add("Enumerate Reference", ReadEnumerateReference(reader));
                        break;
                        }
                    case "rele":
                        {
                        reference.Add("Offset", ReadOffset(reader));
                        break;
                        }
                    case "idnt":
                        {
                        reference.Add("Identifier", reader.ReadAsAscii(4));
                        break;
                        } //BUG copy from Action
                    case "indx":
                        {
                        reference.Add("Index", reader.ReadInt16());
                        break;
                        } // BUG guess from "Index" in Ctrl+F in document
                    case "name":
                        {
                        reference.Add("Name", reader.ReadString());
                        break;
                        } //BUG Guess
                    default:
                        throw new InvalidFormatException($"Unknown structure type");
                    }
                }

            return reference;
            }

        #endregion

        #region 会造成递归的复杂数据类型 (嵌套包含简单类型或其他复杂数据类型, 返回 JArray/JObject)
        private JArray ReadStructureList()
            {
            JArray list = [];
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                {
                var type = reader.ReadAsType();
                var value = reader.Read(type);
                list.Add(value);
                }

            return list;
            }

        private JObject ReadSubDescriptor()
            {
            // no version to read
            JObject obj = [];
            obj.Add("Name", reader.ReadString());
            obj.Add("ClassID", reader.ReadAsKey());

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                {
                // key, and osType, both 4 bytes
                var key = reader.ReadAsKey();
                var osType = reader.ReadAsType();
                var prop = reader.Read(osType);
                obj.Add(key.Trim(), prop);
                }

            return obj;
            }

        #endregion

        #region 暂时不受支持的(原版程序中没有实现)
        private JObject ReadObjectArray()
            {
            _ = reader.ReadInt32(); //Version
            JObject objectArray = [];
            objectArray.Add("Name", reader.ReadString());
            objectArray.Add("ClassID", reader.ReadAsKey());

            var count = reader.ReadInt32();

            JArray items = [];

            for (var i = 0; i < count; i++)
                {
                JObject props = new()
                    {
                    ["Type1"] = reader.ReadAsKey(),
                    ["EnumName"] = reader.ReadAsType(),
                    ["Type2"] = Enum.GetName(reader.ReadAsType().ToUnitType()),
                    ["Values"] = new JArray(reader.ReadDoubles(reader.ReadInt32())),
                    };

                items.Add(props);
                }

            objectArray.Add("Items", items);
            return objectArray;
            }
        #endregion


        private static JObject ParseToJson(string text)
            {
            // 2. 简单替换结构为JSON格式
            text = Regex.Replace(text, @"/([A-z]*)\r", m => $"\"{m.Groups[1].Value}\":\n");
            text = Regex.Replace(text, @"/([A-z]*)\n", m => $"\"{m.Groups[1].Value}\":\n");
            text = Regex.Replace(text, @"<<", "{");
            text = Regex.Replace(text, @">>", "},");
            text = Regex.Replace(text, @"/([A-z\d]*) ", m => $"\"{m.Groups[1].Value}\":");
            text = Regex.Replace(text, @"\(([\s\S]*?)\)", m => $"\"{m.Groups[1].Value}\",");
            text = Regex.Replace(
                text,
                @"\[ ([\d| |\.]*?) \]",
                m => $"[{m.Groups[1].Value.Replace(" ", ",")}]"
            );
            text = Regex.Replace(text, @"(\])", m => $"{m.Groups[1].Value},");
            text = Regex.Replace(text, @"(:[\d][\d\.]*)", m => $"{m.Groups[1].Value},");
            text = Regex.Replace(text, @":([\.][\d\.]*)", m => $":0{m.Groups[1].Value},");
            text = Regex.Replace(text, @"(:false|true)", m => $"{m.Groups[1].Value},");
            text = Regex.Replace(text, @",(\.\d)", m => $",0{m.Groups[1].Value},");
            text = Regex.Replace(text, @",([\r\n][\t\n]*)}", m => $"{m.Groups[1].Value}}}");
            text = Regex.Replace(text, @",([\r\n][\t\n]*)\]", m => $"{m.Groups[1].Value}]");
            text = Regex.Replace(text, @"\[(\.)", m => $"[0{m.Groups[1].Value}");

            // 3. 进一步处理key-value
            text = text.TrimEnd(',');
            // 4. 反序列化为对象再序列化为格式化JSON
            var obj = JObject.Parse(text);
            return obj;
            }

        /// <summary>
        /// 按顺序将 content 拆分为若干部分，每部分为以 "FE FF" 开头、"00 00" 结尾的片段（包含分隔符），其余为普通片段。
        /// </summary>
        /// <param name="content">要拆分的字节数组</param>
        /// <returns>拆分后的各部分（每部分为 byte[]）</returns>
        private static List<byte[]> SplitByUtf16BeBlocks(byte[] content)
            {
            var result = new List<byte[]>();
            var i = 0;
            while (i < content.Length)
                {
                // 查找下一个 FE FF
                if (i + 1 < content.Length && content[i] == 0xFE && content[i + 1] == 0xFF)
                    {
                    var start = i;
                    i += 2;
                    // 查找 00 00 结尾
                    while (i + 1 < content.Length)
                        {
                        if (content[i] == 41 && content[i + 1] == 10)
                            {
                            i += 2;
                            break;
                            }

                        i += 2;
                        }

                    var end = i;
                    var block = new byte[end - start];
                    Array.Copy(content, start, block, 0, block.Length);
                    result.Add(block);
                    }
                else
                    {
                    var start = i;
                    // 查找下一个 FE FF
                    while (
                        i < content.Length
                        && !(i + 1 < content.Length && content[i] == 0xFE && content[i + 1] == 0xFF)
                    )
                        {
                        i++;
                        }

                    if (i > start)
                        {
                        var block = new byte[i - start];
                        Array.Copy(content, start, block, 0, block.Length);
                        result.Add(block);
                        }
                    }
                }

            return result;
            }
        }
    }