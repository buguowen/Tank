using System;
using UnityEngine;

namespace Framework.Web
{
    public class ByteArray 
    {
        public ByteArray(int capacity)
        {
            bytes = new byte[capacity];
            readIdx = 0;
            writeIdx = 0;
            this.capacity = capacity;
            
        }
        public ByteArray(byte[] bytes)
        {
            this.bytes = bytes;
            readIdx = 0;
            writeIdx = bytes.Length;
            capacity = bytes.Length;
        }

        public byte[] bytes;
        public int readIdx;
        public int writeIdx;
        public int capacity;

        public int Size => writeIdx-readIdx;
        public int Remain => capacity-Size;

        /*public void Write(byte[] bs, int offset, int len)
        {
            if(Remain < len)
            {
                Expand(capacity + len);
            }
            if(capacity-writeIdx < len)
            {
                MoveBytes();
            }
            Array.Copy(bs, offset, bytes, writeIdx, len);
            writeIdx = writeIdx + len;
        }*/
        /*public byte[] Read(int len)
        {
            byte[] bs = new byte[len];
            Array.Copy(bytes, readIdx, bs, 0, len);
            readIdx = readIdx + len;
            return bs;
        }*/        
        public void MoveBytes()
        {
            Array.Copy(bytes, readIdx, bytes, 0, Size);
            readIdx = 0;
            writeIdx = readIdx + Size;
        }
        public void Expand(int len)
        {
            while(capacity<len)
            {
                capacity = capacity * 2;
            }
            byte[] newBs = new byte[capacity];
            Array.Copy(bytes, readIdx, newBs, 0, Size);
            bytes = newBs;
            readIdx = 0;
            writeIdx = readIdx + Size;
        }
        
        public void PrintStr()
        {
            string str = System.Text.Encoding.UTF8.GetString(bytes, readIdx, Size);
            Debug.Log(str);
        }
        public void PrintBytes()
        {
            string str = BitConverter.ToString(bytes);
            Debug.Log(str);
        }
   
    }
}