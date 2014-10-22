using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sinaWeiboLogin
{
    public class JSRSAUtil
    {
        private static Random rand = new Random();

        private BigInteger n = null;
        private BigInteger e = null;

        private BigInteger pkcs1pad2(string a, int b)
        {
            if (b < a.Length + 11)
            {
                throw new Exception("Message too long for RSA");
            }

            byte[] c = new byte[b];
            int d = a.Length - 1;
            while (d >= 0 && b > 0)
            {
                int e = (int)a[d--];
                if (e < 128)
                {
                    c[--b] = Convert.ToByte(e);
                }
                else if ((e > 127) && (e < 2048))
                {
                    c[--b] = Convert.ToByte(((e & 63) | 128));
                    c[--b] = Convert.ToByte((e >> 6) | 192);
                }
                else
                {
                    c[--b] = Convert.ToByte((e & 63) | 128);
                    c[--b] = Convert.ToByte(((e >> 6) & 63) | 128);
                    c[--b] = Convert.ToByte(((e >> 12) | 224));
                }
            }

            c[--b] = Convert.ToByte(0);

            byte[] temp = new byte[1];

            while (b > 2)
            {
                temp[0] = Convert.ToByte(0);
                while (temp[0] == 0)
                    rand.NextBytes(temp);
                c[--b] = temp[0];
            }

            c[--b] = 2;
            c[--b] = 0;

            return new BigInteger(c);
        }

        public void RSASetPublic(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                throw new Exception("Message too long for RSA");
            }

            n = new BigInteger(a, 16);
            e = new BigInteger(b, 16);
        }

        private BigInteger RSADoPublic(BigInteger x)
        {
            return x.modPow(e, n);
        }

        public string RSAEncrypt(string a)
        {
            BigInteger tmp = pkcs1pad2(a, (n.bitCount() + 7) >> 3);

            tmp = RSADoPublic(tmp);

            string result = tmp.ToHexString();
            return 0 == (result.Length & 1) ? result : "0" + result;
        }
    }
}
