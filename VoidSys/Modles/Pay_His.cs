using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoidSys.Modles
{
    public class Pay_His
    {
        public int id { get; set; }

        public string vid { get; set; }

        public long coin { get; set; }

        public long time { get; set; }
    }

    public class Pay_HisRes:Pay_His
    {
        public v_UsersData userData { get; set; }
    }

    public class Pay_please
    {
        public int id { get; set; }

        public string vid { get; set; }

        public long coin { get; set; }

        public double money { get; set; }

        public string reader { get; set; }

        public bool readed { get; set; }

        public long pleaseTime { get; set; }

        public long outTime { get; set; }
    }

    public class Pay_PleaseRes : Pay_please
    {
        public v_UsersData userData { get; set; }

        public v_Admin worker { get; set; }
    }

    public static class PayData
    {
        /// <summary>
        /// 发起请求的应用ID。沙箱与线上不同，请更换代码中配置;
        /// </summary>
        public static string AppId { get; set; } = "2021000119655065";

        /// <summary>
        /// 用于支付宝账户登录授权业务的入参 pid
        /// </summary>
        public static string PId { get; set; } = "2088621958224531";

        /// <summary>
        /// 支付宝私匙
        /// </summary>
        public static string PrivateKey { get; set; } = "MIIEowIBAAKCAQEAsK3WKhVF0njlKc+MHl2qlbhHn3WRGXRAvtXPcgU8nn0Ga6bfscwxFfQTxiubxCt4nJTl026Uqy0+q2LEu1MZdKyFGEJiAsGBPA5v8quElxVOxNtjAyVzUjTGwTg5HIBLYarRyH1a6RU7N/V8s9/i7svcvPRbfOwmoGFRTMBM/AfCS7IFdWeAqahGXpJQ8y6hfDZysOmncKbn54GHpjXi7gkldNREIJUPvUG7f5+ruuxo1t1FYDswunwntwHUpKVQ1C/0HUOs7R6ZASG44eUlJZ/H4wfTiL5deU0izmCHNObux4GUj9+PJAOP4D0l2Jt5urS5l1+ugwLenI4PPUAFKQIDAQABAoIBABO/+vbNVfbHKObZmpIyakU+SlNa8xNjWXF7uSrHxxT4aOVTzCG1767CkAtRCKKPMXZfdqmB7QCNmDnUWqWODoRqXJ5vnUEtnGK4Qm9gGPxCl4GN/K13XELOP4GN5WR4OvT6AWVmDemMRAhnWr9Iwbdr9orwzeFTDKZU+p+Xx4NMsycaZYrqDQ3OPzBmZAaP667Hsga36iLfmvq+dK8u19VPDPSu1GklMX2EejE9GNZ2mNsrYSU/rinTg13CtANnS7paQA4GD4K4y0kknXxVhlVq/j8aU9Szs1QYXOrS+IHjMYg3j/ErIAEvZXe/XWwjO+qzwpwOmf6e5Q2eYU0o9uECgYEA3Y1HnR+HHyC6OkY4ec2NlkPgdN8EkChoOroDS82hvRPGF7ZPUTK96D+D3AbkHDQ6FOp3qIZNFslkBbd2ZhINFZmBGvbhut0jmgpIaT9bqQqj1UMoGJlGkgrt/SRQpmBLqRwp9XpIFQ54yrmTVAcHoafvSsfjBgqa9HmPQ7/fH30CgYEAzCZswt1VcdLlWKt64fJDYqAkM/f8Z1itdEbP8/GdejPB9g+8nIxcRNG/j3WustXCfumpvhiXQEH3UEoc4HyIvx7MZYuleUS8PHfLuxbvbZsK6l/WH3fn5OCmQf9UUnBB+bi4+E5NtDheBNlWisKHVQkRbNwqDG/8/0t69C5BhB0CgYEAjlXNxGwU6zKjcfzbG6WraPaCpZMB43uSOuh2ZaTeXBLwGRvPKWNoDmV/2UO0GUqUZguchHCD5jfMQr/zGPBP56iPATvLGboovTYN/0/tG1TynHhgsi0G4ZyT+SXzinAjK6okasj8tkpt4BAJTtqVVI4HdgulFqSJmLbgC66hMiECgYB82o8vyg+ksleDmDRqFTEzEz+w7NxYFDrY3yn1RnKKNzdLlqpCj/EqQaFHSrDSPLtbxOoEEaQHL5DicMEiZed4A4z8GaDU/r2kuZtGy6sYvHa7ims5CaswJlhoCpD2biNbZ6bwbWtGaibODmHIMVp1Hjui+S9aNe9j7zS+O7/j3QKBgDAEdq7UP2svxKuuSH2r4XigmPg+KqTMUrTdbRFUUpLe74Wr6Fhs7tL0K7ssXZuogcHnN7QlBs3U3NKCWTsUelfVR4DdZm2XbDY8OR2VFpVizL3arHo+o0Zx1OYxLe6a0DKuXoANy/ktZ/+SnWFfRCRuYzLriPGYMkxr6iHx/oPN";

        /// <summary>
        /// 应用公钥
        /// </summary>
        public static string Publickey { get; set; } = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsK3WKhVF0njlKc+MHl2qlbhHn3WRGXRAvtXPcgU8nn0Ga6bfscwxFfQTxiubxCt4nJTl026Uqy0+q2LEu1MZdKyFGEJiAsGBPA5v8quElxVOxNtjAyVzUjTGwTg5HIBLYarRyH1a6RU7N/V8s9/i7svcvPRbfOwmoGFRTMBM/AfCS7IFdWeAqahGXpJQ8y6hfDZysOmncKbn54GHpjXi7gkldNREIJUPvUG7f5+ruuxo1t1FYDswunwntwHUpKVQ1C/0HUOs7R6ZASG44eUlJZ/H4wfTiL5deU0izmCHNObux4GUj9+PJAOP4D0l2Jt5urS5l1+ugwLenI4PPUAFKQIDAQAB";

        /// <summary>
        /// 支付宝公匙
        /// </summary>
        public static string AlipayPublicKey { get; set; } = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhFNhVP1s4dFEKTjYyiII/qEGaHavuV7q8+D7GlXHBLr8QUoIYw9me9oZagCyKB55k9hhZJuU9iOSTjUTkvpAEKkeafSJkhL2jz6Po3G+yROcExiJ0KZIpRLZvF+6D/HajPQcZ502AIrl8fRlhD3ndZDenFdMlC2jYntSDnv7K5BZUidru45boj+6khCH6DUOMNHfV+SJt72OofIH3cvw1ioHjZTiAvH0lmCvL5BPoZfVxnXf8fBje4T4dm87i5mbaruGXaTwEO/9ubLoBBcmEErOnWQDlo+dxORfnAkUqmQB0vPABCDJcxlauh5ke+07YN8PpZtz9aWDNhpqnp+ntwIDAQAB";

        /// <summary>
        ///  公匙类型/签名类型
        /// </summary>
        public static string SignType { get; set; } = "RSA2";

        /// <summary>
        ///  编码格式
        /// </summary>
        public static string CharSet { get; set; } = "UTF-8";

        /// <summary>
        /// 向支付宝发起请求的网关。沙箱与线上不同，请更换代码中配置;沙箱:https://openapi.alipaydev.com/gateway.do上线https://openapi.alipay.com/gateway.do
        /// </summary>
        public static string GatewayUrl { get; set; } = "https://openapi.alipaydev.com/gateway.do";

        /// <summary>
        /// 仅支持JSON
        /// </summary>
        public static string Format { get; set; } = "json";

        public static bool KeyFromFile { get; set; } = false;
    }
}
