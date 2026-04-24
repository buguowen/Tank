using Newtonsoft.Json;
using System;


namespace Framework.Web
{
    public class MsgBase 
    {
        protected string protoName;
        public string ProtoName => protoName;

        // 协议格式
        // 协议总长度(2B) | 协议名长度(2B) + 协议名 | 协议内容
        public static byte[] Encode(MsgBase msgBase)
        {
            string name = msgBase.protoName;
            byte[] nameBytes = EncodeName(name);
            byte[] contentBytes = EncodeContent(msgBase);
            
            int len = nameBytes.Length + contentBytes.Length;        
            byte[] result = new byte[len + 2];
            result[0] = (byte)(len & 0xFF);
            result[1] = (byte)((len >> 8) & 0xFF);
            Array.Copy(nameBytes, 0, result, 2, nameBytes.Length);
            Array.Copy(contentBytes, 0, result, 2 + nameBytes.Length, contentBytes.Length);

            return result;
        }

        // bytes{协议名长度(2B) | 协议名 | 协议内容}
        public static MsgBase Decode(byte[] bytes, int offset, int count, out string protoName)
        {
            Int16 lenName = (Int16)(bytes[offset+1]<<8 | bytes[offset]);
            protoName = DecodeName(bytes, offset + 2, lenName);

            MsgBase msg = DecodeContent(bytes, offset + lenName, count, protoName);
            return msg;
        }

        // MsgBase->byte[]
        public static byte[] EncodeContent(MsgBase msgBase)
        {
            string json = JsonConvert.SerializeObject(msgBase);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return bytes;
        }
        // byts[]->MsgBase
        public static MsgBase DecodeContent(byte[] bytes, int offset, int count, string protoName)
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
            Type type = Type.GetType(protoName);
            MsgBase msgBase = (MsgBase)JsonConvert.DeserializeObject(json, type);
            return msgBase;
        }
        // name(string) -> length + name (byte[])
        public static byte[] EncodeName(string name)
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(name);
            Int16 len = (Int16)bs.Length;
            byte[] lenBs = new byte[2 + len];
            lenBs[0] = (byte)(len & 0xFF);
            lenBs[1] = (byte)((len >> 8)&0xFF);

            Array.Copy(bs, 0, lenBs, 2, len);
            return lenBs;
        }
        // name (byte[]) -> name(string)
        public static string DecodeName(byte[] bs, int offset, int count)
        {
            string name = System.Text.Encoding.UTF8.GetString(bs, offset, count);
            return name;
        }

        
   
    }
}