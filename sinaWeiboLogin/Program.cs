using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BloomFilter;
using System.Threading;

namespace sinaWeiboLogin
{
    class Program
    {
        private static readonly Regex UidReg = new Regex("\\b[0-9]+");  //过滤账号
        private static string _strAll = ";";    //存储请求到的URL
        private static string _queueToRequest = "";    //存放某次请求到的账号（<=20） 
        private static string _queryNew = "";   //请求队列用于递归
        private static string _alreadRequesed = "";     //已请求过的账号
        private static int _len = 0;    //总个数

        private static string GetResponseKeyValue(string key,string responseContent)
        {
            Regex regex = new Regex(key + "\\\":\\\"?(.*?)(\\,|\\\")");

            var match = regex.Match(responseContent);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        private static string GetLoginRedirectUrl(string response)
        {
            Regex reg = new Regex("location\\.replace\\('(.*)'");
            var match = reg.Match(response);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public static int SinaLogin(string uid, string psw, CookieContainer cc)
        {
            string uidbase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(uid));

            string url =
                "http://login.sina.com.cn/sso/prelogin.php?entry=weibo&callback=sinaSSOController.preloginCallBack&su=&rsakt=mod&checkpin=1&client=ssologin.js(v1.4.11)&_=" +
                DateTime.Now.Ticks;

            HttpWebRequest webRequest1 = (HttpWebRequest)WebRequest.Create(new Uri(url)); //获取servertime和 nonce
            webRequest1.CookieContainer = cc;
            HttpWebResponse response1 = (HttpWebResponse)webRequest1.GetResponse();
            StreamReader sr1 = new StreamReader(response1.GetResponseStream(), Encoding.UTF8);
            string res = sr1.ReadToEnd();

            string servertime = GetResponseKeyValue("servertime", res);
            string nonce = GetResponseKeyValue("nonce", res);
            string pubkey = GetResponseKeyValue("pubkey", res);

            JSRSAUtil rsaUtil = new JSRSAUtil();
            rsaUtil.RSASetPublic(pubkey,"10001");
            var encryPwd = servertime + '\t' + nonce + '\n' + psw;
            Console.WriteLine(encryPwd);
            string password = rsaUtil.RSAEncrypt(encryPwd);//密码RSA加密

            string str = "entry=weibo&gateway=1&from=&savestate=7&useticket=1&vsnf=1&su=" +
                uidbase64 + "&service=miniblog&servertime=" + servertime + "&nonce=" + nonce + "&pwencode=rsa2&rsakv=1330428213&sp=" + password + "&sr=1366*768&prelt=282&encoding=UTF-8&url=" +
                      HttpUtility.UrlEncode("http://weibo.com/ajaxlogin.php?framelogin=1&callback=parent.sinaSSOController.feedBackUrlCallBack") +
                      "&returntype=META";

            byte[] bytes;
            ASCIIEncoding encoding = new ASCIIEncoding();
            bytes = encoding.GetBytes(str);
            // bytes = System.Text.Encoding.UTF8.GetBytes(HttpUtility.UrlEncode(str));
            HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create("http://login.sina.com.cn/sso/login.php?client=ssologin.js(v1.3.16)");
            webRequest2.Method = "POST";
            webRequest2.ContentType = "application/x-www-form-urlencoded";
            webRequest2.ContentLength = bytes.Length;
            webRequest2.CookieContainer = cc;

            Stream stream;
            stream = webRequest2.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
            StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.Default);
            string res2 = sr2.ReadToEnd();

            Console.WriteLine(res2);

            if (res2.IndexOf("reason=") >= 0)
            {
                Console.WriteLine("登录失败...");
                return -1;
            }

            var redirectUrl = GetLoginRedirectUrl(res2);

            if (string.IsNullOrEmpty(redirectUrl))
            {
                Console.WriteLine("登录失败, redict Url没找到...");
                return -1;
            }
            else
            {
                Console.WriteLine(redirectUrl);
            }


            HttpWebRequest webRequest3 = (HttpWebRequest)WebRequest.Create(new Uri(redirectUrl));
            webRequest3.CookieContainer = cc;
            HttpWebResponse response3 = (HttpWebResponse)webRequest3.GetResponse();
            StreamReader sr3 = new StreamReader(response3.GetResponseStream(), Encoding.UTF8);
            res = sr3.ReadToEnd();

            Console.WriteLine(res);

            foreach (Cookie cookie in response3.Cookies)
            {
                cc.Add(cookie);
            }
            return 0;
        }

        static void Main(string[] args)
        {
            CookieContainer cookies = new CookieContainer();
            var loginStatus = SinaLogin("aaaa", "bbbb", cookies);
            if (loginStatus == 0)
            {
                Console.WriteLine("登录成功！");
            }
            else
            {
                Console.WriteLine("登录失败！");
            }
        
            Console.Read();
        }
    }
}
