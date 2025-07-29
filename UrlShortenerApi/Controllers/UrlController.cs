namespace UrlShortenerApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using UrlShortenerApi.Contracts;

    [Route("v1/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        public IList<UrlShortcut> Shortcuts { get; set; } = new List<UrlShortcut>();

        // GET: v1/<UrlController>
        [HttpGet]
        public IEnumerable<UrlShortcut> Get()
        {
            return this.Shortcuts;
        }

        // GET v1/<UrlController>/xyz
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return await Task.FromResult(
                Redirect("https://gamershub.azurewebsites.net"));
        }

        // POST v1/<UrlController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT v1/<UrlController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE v1/<UrlController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
