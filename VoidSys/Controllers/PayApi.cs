using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoidSys.Modles;
using VoidSys.Service;

namespace VoidSys.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[EnableCors]
	public class PayApi : ControllerBase
    {
        private readonly Set _set;
        private readonly IConfiguration _configuration;

        private static Dictionary<string, string> pays = new Dictionary<string, string>(); 

        public PayApi(Set set, IConfiguration configuration)
        {
            _set = set;
            _configuration = configuration;
		}

		/// <summary>
		/// 发起支付请求
		/// </summary>
		/// <returns></returns>
		[HttpGet("payLink")]
		public string PayRequest(string vid, string username, string totalAmout)
		{
			DefaultAopClient client = new DefaultAopClient(PayData.GatewayUrl, PayData.AppId, PayData.PrivateKey, "json", "2.0",
				PayData.SignType, PayData.AlipayPublicKey, PayData.CharSet, false);

			// 组装业务参数model
			AlipayTradePagePayModel model = new AlipayTradePagePayModel();
            string keys = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "P" + vid;
            model.Body = "充值";
			model.Subject = username;
			model.TotalAmount = totalAmout;
			model.OutTradeNo = keys;
			model.ProductCode = "FAST_INSTANT_TRADE_PAY";
            if(pays.Where(i=>i.Value == vid).Count() > 0)
            {
                var payList = pays.Where(i => i.Value == vid).ToList();
                payList.ForEach(i =>
                {
                    pays.Remove(i.Key);
                });
            }
            pays.Add(keys, vid);
			AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
			// 设置同步回调地址
			request.SetReturnUrl("http://120.76.177.46:5000/api/PayApi/PayComplete");
			// 设置异步通知接收地址
			//request.SetNotifyUrl("http://localhost:5000/api/PayApi/PayComplete");
			// 将业务model载入到request
			request.SetBizModel(model);

			var response = client.SdkExecute(request);
			Console.WriteLine($"用户充值发起成功，订单key：{keys}");
			//跳转支付宝支付
			return PayData.GatewayUrl + "?" + response.Body;
		}

        /// <summary>
		/// 发起支付请求
		/// </summary>
		/// <returns></returns>
		[HttpGet("payLinkAndorid")]
        public string PayRequestAndorid(string vid, string username, string totalAmout)
        {
            IAopClient client = new DefaultAopClient(PayData.GatewayUrl, PayData.AppId, PayData.PrivateKey, "json", "2.0",
                PayData.SignType, PayData.AlipayPublicKey, PayData.CharSet, false);

            // 组装业务参数model
            AlipayTradeWapPayModel model = new AlipayTradeWapPayModel();
            string keys = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "P" + vid;
            model.Body = "充值";
            model.Subject = username;
            model.TotalAmount = totalAmout;
            model.OutTradeNo = keys;
            model.ProductCode = "FAST_INSTANT_TRADE_PAY";
            if (pays.Where(i => i.Value == vid).Count() > 0)
            {
                var payList = pays.Where(i => i.Value == vid).ToList();
                payList.ForEach(i =>
                {
                    pays.Remove(i.Key);
                });
            }
            pays.Add(keys, vid);
            AlipayTradeWapPayRequest request = new AlipayTradeWapPayRequest();
            // 设置同步回调地址
            request.SetReturnUrl("http://120.76.177.46:5000/api/PayApi/PayComplete");
            // 设置异步通知接收地址
            //request.SetNotifyUrl("http://localhost:5000/api/PayApi/PayComplete");
            // 将业务model载入到request
            request.SetBizModel(model);
            //Response.ContentType = "text/html;charset="+PayData.CharSet;
            
            var response = client.SdkExecute(request);
            Console.WriteLine($"用户充值发起成功，订单key：{keys}");
            //跳转支付宝支付
            return PayData.GatewayUrl + "?" + response.Body;
        }

        /// <summary>
        /// 向指定者投币硬币
        /// </summary>
        [HttpPost("toCoin")]
        [Authorize]
        async public Task<Result<bool>> toCoin(string toid, long size)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                var user = _set.v_usersData.Find(vid);
                if (user.coin >= size)
                {
                    _set.v_usersData.Find(vid).coin -= size;
                    _set.v_usersData.Find(toid).coin += size;
                    _set.coinget.Add(new Coinget()
                    {
                        coin = size,
                        toid = toid,
                        vid = vid,
                        time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }) ;
                    result.data = true;
                    _set.SaveChanges();
                }
                else
                {
                    result.code = 500;
                    result.msg = "硬币不足";
                }
                return result;
            });
        }


        /// <summary>
        /// 向目标添加硬币
        /// </summary>
        [HttpGet("PayComplete")]
        async public Task<string> addCoin(double total_amount, string out_trade_no)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string vid = null;
                if (pays.GetValueOrDefault(out_trade_no) !=null)
                {
                    vid = pays.GetValueOrDefault(out_trade_no);
                    _set.v_usersData.Find(vid).coin += (long)total_amount*100;
                    _set.pay_his.Add(new Pay_His()
                    {
                        time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        vid = vid,
                        coin = (long)total_amount * 100
                    });
                    _set.SaveChanges();
                    result.data = true;
                    result.msg = "充值成功";
                }
                else
                {
                    result.code = 500;
                    result.msg = "充值失败，找不到指定订单的key";
                }
                return result.msg;
            });
        }

        /// <summary>
        /// 硬币提现申请
        /// </summary>
        [HttpPost("downCoin")]
        [Authorize]
        async public Task<Result<bool>> downCoin(long coin)
        {
            return await Task.Run(() =>
            {
                Result<bool> result = new Result<bool>();
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                var user = _set.v_usersData.Find(vid);
                if (user.cardNum == null||user.cardNum == "")
                {
                    result.msg = "提现失败，银行卡号未绑定";
                    result.code = 500;
                    return result;
                }
                user.coin -= coin;
                _set.v_usersData.Update(user);
                _set.pay_please.Add(new Pay_please()
                {
                    money= coin*1.0/100,
                    coin = coin,
                    pleaseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    vid = vid
                });
                result.data = _set.SaveChanges() > 0;
                return result;
            });
        }

        /// <summary>
        /// 管理员提醒提现成功
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("putCoin")]
        [Authorize]
        async public Task<Result<bool>> putCoin(int id)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                Result<bool> result = new Result<bool>();
                Pay_please p = _set.pay_please.Find(id);
                p.readed = true;
                p.reader = vid;
                p.outTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _set.pay_please.Update(p);
                _set.SaveChanges();
                result.data = true;
                EmailSend.toEmil(_set.v_usersData.Where(i => i.vid == p.vid).FirstOrDefault().email, p.money);
                return result;
            });
        }

        /// <summary>
        /// 获取充值历史
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getPayHistory")]
        async public Task<ResultPage<List<Pay_HisRes>>> getPayHistory(int page,int size,string vid)
        {
            return await Task.Run(() =>
            {
                List<Pay_His> res;
                if (vid != null)
                {
                    res = _set.pay_his.Where(i => i.vid == vid).OrderByDescending(i => i.time).ToList();
                }
                else
                {
                    res = _set.pay_his.OrderByDescending(i => i.time).ToList();

                }
                return new ResultPage<List<Pay_HisRes>>()
                {
                    page = page,
                    limt = (int)Math.Ceiling(res.Count * 1.0 / size),
                    data = res.Skip((page - 1) * size).Take(size).Select(i => new Pay_HisRes()
                    {
                        id = i.id,
                        coin = i.coin,
                        time = i.time,
                        userData = _set.v_usersData.Where(it => it.vid == i.vid).FirstOrDefault(),
                        vid = i.vid
                    }).ToList()
                };
            });
        }

        /// <summary>
        /// 获取提现申请
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("getPayPlease")]
        async public Task<ResultPage<List<Pay_PleaseRes>>> getPayPlease(int page, int size, string vid)
        {
            return await Task.Run(() =>
            {
                List<Pay_please> res;
                if (vid != null)
                {
                    res = _set.pay_please.Where(i => i.vid == vid).OrderByDescending(i => i.pleaseTime).ToList();
                }
                else
                {
                    res = _set.pay_please.OrderByDescending(i => i.pleaseTime).ToList();

                }
                return new ResultPage<List<Pay_PleaseRes>>()
                {
                    page = page,
                    limt = (int)Math.Ceiling(res.Count * 1.0 / size),
                    data = res.Skip((page - 1) * size).Take(size).Select(i => new Pay_PleaseRes()
                    {
                        id = i.id,
                        coin = i.coin,
                        money = i.money,
                        reader = i.reader,
                        outTime = i.outTime,
                        userData = _set.v_usersData.Where(it => it.vid == i.vid).FirstOrDefault(),
                        vid = i.vid,
                        pleaseTime = i.pleaseTime,
                        readed = i.readed,
                        worker = _set.v_admins.Where(it=>it.vid == i.reader).FirstOrDefault()
                    }).ToList()
                };
            });
        }

        [HttpGet("getCoinToMe")]
        [Authorize]
        async public Task<ResultPage<List<CoinRes>>> getPayTo(int page, int size)
        {
            return await Task.Run(() =>
            {
                string header = HttpContext.Request.Headers["Authorization"];
                string vid = header.getVid();
                List<Coinget> res = _set.coinget.Where(i=>i.toid == vid).OrderByDescending(i => i.time).ToList(); ;
                return new ResultPage<List<CoinRes>>()
                {
                    page = page,
                    limt = (int)Math.Ceiling(res.Count * 1.0 / size),
                    data = res.Skip((page - 1) * size).Take(size).Select(i => new CoinRes()
                    {
                        id = i.id,
                        coin = i.coin,
                        time = i.time,
                        userData = _set.v_usersData.Where(it => it.vid == i.vid).FirstOrDefault(),
                        vid = i.vid,
                        toid = i.toid
                    }).ToList()
                };
            });
        }
    }
}
