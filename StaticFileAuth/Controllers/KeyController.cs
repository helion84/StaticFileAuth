using Microsoft.AspNetCore.Mvc;
using StaticFileAuth.Cache;

namespace StaticFileAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeyController : ControllerBase
    {
        private readonly IKeyCache _keyCache;
        public KeyController(IKeyCache keyCache)
        {
            _keyCache = keyCache;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var key = _keyCache.GenerateKey();
            return Ok(key);
        }

        [HttpPut]
        public ActionResult Refresh(string key)
        {
            _keyCache.RefreshKey(key);
            return Ok();
        }
    }
}
