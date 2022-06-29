using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoidSys.Modles;

namespace VoidSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaApi : ControllerBase
    {
        private readonly Set _set;

        public AreaApi(Set set)
        {
            _set = set;
        }

        /// <summary>
        /// 获取专区分类
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [EnableCors]
        async public Task<Result<List<v_Area>>> GetV_Areas()
        {
            var task = Task.Run(() =>
            {
                Result<List<v_Area>> result = new Result<List<v_Area>>();
                result.data = _set.v_area.ToList().Select(i=> {
                    i.pageNum = _set.v_page.Where(i2 => i2.aid == i.aid).Count();
                    return i;
                }).ToList();
                return result;
            });
            return await task;
        }
    }
}
