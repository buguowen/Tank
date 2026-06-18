using Newtonsoft.Json;
using System;
using UnityEngine;


namespace Framework.Web
{
    public class MsgBase 
    {
        public string protoName;
        public string ProtoName => protoName;

        // 协议格式
        // 协议总长度(2B) | 协议名长度(2B) + 协议名 | 协议内容
        public static byte[] Encode(MsgBase msgBase)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
            Int16 nameLen = (Int16)nameBytes.Length;

            string json = JsonConvert.SerializeObject(msgBase);
            byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(json);

            Int16 totalLen = (Int16)(2 + nameLen + bodyBytes.Length);

            byte[] result = new byte[2 + totalLen];
            result[0] = (byte)(totalLen & 0xFF);
            result[1] = (byte)((totalLen >> 8) & 0xFF);
            result[2] = (byte)(nameLen & 0xFF);
            result[3] = (byte)((nameLen >> 8) & 0xFF);

            Array.Copy(nameBytes, 0, result, 4, nameBytes.Length);
            Array.Copy(bodyBytes, 0, result, 4 + nameBytes.Length, bodyBytes.Length);

            return result;
        }

        public static MsgBase Decode(byte[] bytes, int offset, int count, out string protoName)
        {
            protoName = "";
            string json = "";
            try
            {
                Int16 nameLen = (Int16)(bytes[offset] | (bytes[offset+1] << 8));
                protoName = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, nameLen);

                int bodyOffset = offset + 2 + nameLen;
                int bodyCount = count - 2 - nameLen;

                json = System.Text.Encoding.UTF8.GetString(bytes, bodyOffset, bodyCount);
                Type type = Type.GetType("Proto." + protoName);   // 全命名空间, 否则反射失败
                if (type == null) return null;

                Debug.Log($"Decode: [ProtoName] {protoName} | [Content] {json}");

                return (MsgBase)JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Decode JSON Error] {protoName} 失败: {ex.Message}. JSON Content: {json}");
                return null; // 返回 null，防止崩溃，允许客户端继续运转
            }
        }

        
   
    }
}