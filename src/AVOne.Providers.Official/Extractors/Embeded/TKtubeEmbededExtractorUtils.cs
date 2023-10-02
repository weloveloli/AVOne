// Copyright (c) 2023 Weloveloli. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Providers.Official.Extractors.Embeded
{
    using System.Linq;

    public static class TKtubeEmbededExtractorUtils
    {
        // license_code: "$432515114269431" => "54364362706040403733399244753648"
        public static string GetCode(string lincenseCode)
        {
            var str = "";
            int g;
            for (g = 1; g < lincenseCode.Length; g++)
            {
                var integer = parseInt(lincenseCode[g]);
                str += integer ?? 1;
            }

            var j = str.Length / 2;
            var k = int.Parse(str.Substring(0, j + 1));
            var l = int.Parse(str.Substring(j));
            g = l - k;
            g = Math.Max(g, -g);
            var temp = g;
            g = k - l;
            g = Math.Max(g, -g);
            temp += g;
            temp *= 2;
            str = "" + temp;
            var i = 10;
            var m = "";
            for (g = 0; g < j + 1; g++)
                for (var h = 1; h <= 4; h++)
                {
                    var n = parseInt(lincenseCode[g + h]) + parseInt(str[g]);
                    if (n >= i)
                    {
                        n -= i;
                    }
                    m += n;
                }

            return m;
        }

        public static int? parseInt(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return int.Parse("" + c);
            }
            return null;

        }

        public static string Prefix = "function/";
        /// <summary>
        /// function/3588/https://tktube.com/get_file/24/87d4c4ccdd8f15d26cadb1ab38008f54dc7a2c32e4/121000/121938/121938_480p.mp4/?embed=true => 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="lencense"></param>
        /// <returns></returns>
        public static string GetRealUrl(string url, string lencense)
        {
            url = url.Substring(Prefix.Length);
            var g = url.Split('/');
            var code = GetCode(lencense);
            var h = g[6].Substring(0, 32);
            var j = h;
            for (var k = h.Length - 1; k >= 0; k--)
            {
                var l = k;
                var m = k;
                for (; m < code.Length; m++)
                    l += parseInt(code[m]).Value;
                for (; l >= h.Length;)
                    l -= h.Length;
                var n = "";
                for (var o = 0; o < h.Length; o++)
                {
                    n += o == k ? h[l] : o == l ? h[k] : h[o];
                }

                h = n;
            }
            g[6] = g[6].Replace(j, h);
            return string.Join('/', g.Skip(1));
        }
    }
}
