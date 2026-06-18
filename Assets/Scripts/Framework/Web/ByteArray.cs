using System;
using UnityEngine;

namespace Framework.Web
{
    public class ByteArray 
    {
        public byte[] bytes;
        public int readIdx;
        public int writeIdx;
        public int capacity;

        public int Length => writeIdx - readIdx;    // 已经存储的数据大小
        public int Remain => capacity - writeIdx;   // 还差多少到末尾

        public ByteArray(int capacity = 1024)
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

        public void CheckAndExpand(int count)
        {
            if(Remain < count)
            {
                MoveBytes();
            }
            if(Remain < count)
            {
                Expand(count);
            }
        }
        public void MoveBytes()
        {
            if (Length <= 0)
            {
                readIdx = 0;
                writeIdx = 0;
                return;
            }
            if (readIdx <= 0) return;

            int len = Length;
            Array.Copy(bytes, readIdx, bytes, 0, len);
            readIdx = 0;
            writeIdx = len;
        }
        public void Expand(int len)
        {
            int newCapacity = capacity;
            while(newCapacity - writeIdx< len)
            {
                newCapacity *= 2;
            }

            byte[] newBs = new byte[newCapacity];
            Array.Copy(bytes, readIdx, newBs, 0, Length);

            bytes = newBs;
            capacity = newCapacity;
            writeIdx = Length;
            readIdx = 0;
        }
        
        public void PrintStr()
        {
            string str = System.Text.Encoding.UTF8.GetString(bytes, readIdx, Length);
            Debug.Log(str);
        }
        public void PrintBytes()
        {
            string str = BitConverter.ToString(bytes);
            Debug.Log(str);
        }
   
    }
}