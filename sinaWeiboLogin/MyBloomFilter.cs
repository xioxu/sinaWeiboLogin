using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsFormsApplication1;

namespace BloomFilter
{
    public class MyBloomFilter
    {
        /// <summary>
        /// BitArray用来替代内存块，在C/C++中可使用BITMAP替代
        /// </summary>
        private static BitArray bitArray = null;

        private int size = -1;

        /// <summary>
        /// 构造函数，初始化分配内存
        /// </summary>
        /// <param name="size">分配的内存大小,必须保证被2整除</param>
        public MyBloomFilter(int size)
        {
            if (size % 8 == 0)
            {
                bitArray = new BitArray(size, false);
                this.size = size;
            }
            else
            {
                throw new Exception("错误的长度,不能被2整除");
            }

        }



        private bool Contain(string url)
        {
            return false;
        }
        private void Delete(string url)
        { }
        // BKDR Hash Function
        private int BKDRHash(string str)
        {
            int seed = 131; // 31 131 1313 13131 131313 etc..
            int hash = 0;
            int count;
            char[] bitarray = str.ToCharArray();
            count = bitarray.Length;
            while (count > 0)
            {
                hash = hash * seed + (bitarray[bitarray.Length - count]);
                count--;
            }

            return (hash & 0x7FFFFFFF);
        }
        private int APHash(string str)
        {
            int hash = 0;
            int i;
            int count;
            char[] bitarray = str.ToCharArray();
            count = bitarray.Length;
            for (i = 0; i < count; i++)
            {
                if ((i & 1) == 0)
                {
                    hash ^= ((hash << 7) ^ (bitarray[i]) ^ (hash >> 3));
                }
                else
                {
                    hash ^= (~((hash << 11) ^ (bitarray[i]) ^ (hash >> 5)));
                }

            }

            return (hash & 0x7FFFFFFF);
        }


        /// <summary>
        /// 将str加入Bloomfilter，主要是HASH后找到指定位置置true
        /// </summary>
        /// <param name="str">字符串</param>
        public void Add(string str)
        {
            int[] offsetList = getOffset(str);
            if (offsetList != null)
            {

                put(offsetList[0]);
                put(offsetList[1]);
                put(offsetList[2]);
                put(offsetList[3]);
                put(offsetList[4]);
                put(offsetList[5]);
                put(offsetList[6]);
                put(offsetList[7]);
            }
            else
            {
                throw new Exception("字符串不能为空");
            }
        }

        /// <summary>
        /// 判断该字符串是否重复
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>true重复反之则false</returns>
        public Boolean Contains(string str)
        {
            int[] offsetList = getOffset(str);
            if (offsetList != null)
            {
                int i = 0;

                while (i < 8)
                {
                    if ((get(offsetList[i]) == false)) { return false; }
                    i++;
                }

                return true;


            }
            return false;
        }

        /// <summary>
        /// 返回内存块指定位置状态
        /// </summary>
        /// <param name="offset">位置</param>
        /// <returns>状态为TRUE还是FALSE 为TRUE则被占用</returns>
        private Boolean get(int offset)
        {
            return bitArray[offset];
        }

        /// <summary>
        /// 改变指定位置状态
        /// </summary>
        /// <param name="offset">位置</param>
        /// <returns>改变成功返回TRUE否则返回FALSE</returns>
        private Boolean put(int offset)
        {
            //try
            //{
            if (bitArray[offset])
            {
                return false;
            }
            bitArray[offset] = true;
            //}
            //catch (Exception e)
            //{
            // Console.WriteLine(offset);
            //}
            return true;
        }

        private int[] getOffset(string str)
        {
            if (String.IsNullOrEmpty(str) != true)
            {
                int[] offsetList = new int[8];
                string tmpCode = Hash(str).ToString();
                //    int hashCode = Hash2(tmpCode);
                int hashCode = HashCode.Hash1(tmpCode);
                int offset = Math.Abs(hashCode % (size / 8) - 1);
                offsetList[0] = offset;
                //   hashCode = Hash3(str);
                hashCode = HashCode.Hash2(tmpCode);
                offset = size / 4 - Math.Abs(hashCode % (size / 8)) - 1;
                offsetList[1] = offset;

                hashCode = HashCode.Hash3(tmpCode);
                offset = Math.Abs(hashCode % (size / 8) - 1) + size / 4;
                offsetList[2] = offset;
                //   hashCode = Hash3(str);
                hashCode = HashCode.Hash4(tmpCode);
                offset = size / 2 - Math.Abs(hashCode % (size / 8)) - 1;
                offsetList[3] = offset;

                hashCode = HashCode.Hash5(tmpCode);
                offset = Math.Abs(hashCode % (size / 8) - 1) + size / 2;
                offsetList[4] = offset;
                //   hashCode = Hash3(str);
                hashCode = HashCode.Hash6(tmpCode);
                offset = 3 * size / 4 - Math.Abs(hashCode % (size / 8)) - 1;
                offsetList[5] = offset;

                hashCode = HashCode.Hash7(tmpCode);
                offset = Math.Abs(hashCode % (size / 8) - 1) + 3 * size / 4;
                offsetList[6] = offset;
                //   hashCode = Hash3(str);
                hashCode = HashCode.Hash8(tmpCode);
                offset = size - Math.Abs(hashCode % (size / 8)) - 1;
                offsetList[7] = offset;
                return offsetList;
            }
            return null;
        }
        /// <summary>
        /// 内存块大小
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// 获取字符串HASHCODE
        /// </summary>
        /// <param name="val">字符串</param>
        /// <returns>HASHCODE</returns>
        private int Hash(string val)
        {
            return val.GetHashCode();
        }

        /// <summary>
        /// 获取字符串HASHCODE
        /// </summary>
        /// <param name="val">字符串</param>
        /// <returns>HASHCODE</returns>
        private int Hash2(string val)
        {
            int hash = 0;

            for (int i = 0; i < val.Length; i++)
            {
                hash += val[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        /// <summary>
        /// 获取字符串HASHCODE
        /// </summary>
        /// <param name="val">字符串</param>
        /// <returns>HASHCODE</returns>
        private int Hash3(string str)
        {
            long hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    hash ^= ((hash << 7) ^ str[i] ^ (hash >> 3));
                }
                else
                {
                    hash ^= (~((hash << 11) ^ str[i] ^ (hash >> 5)));
                }
            }
            unchecked
            {
                return (int)hash;
            }
        }
    }
}
