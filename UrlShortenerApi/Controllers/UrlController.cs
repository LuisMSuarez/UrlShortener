namespace UrlShortenerApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.Services;
    using UrlShortenerApi.Services.Contracts;

    [Route("v1/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        private readonly IUrlShortcutService shortcutService;
        public UrlsController(IUrlShortcutService shortcutService)
        {
            this.shortcutService = shortcutService ?? throw new ArgumentNullException(nameof(shortcutService));
        }

        // GET v1/<UrlController>/xyz
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(id);
            }

            try
            {
                var shortcut = await this.shortcutService.GetUrlShortcutAsync(id);
                return Redirect(shortcut.Url);
            }
            catch (ServiceException ex) when (ex.ResultCode == ServiceResultCode.NotFound)
            {
                return NotFound($"Shortcut with id {id} is not found.");
            }
        }

        // GET: v1/<UrlController>?url=<url>
        [HttpGet]
        public async Task<IActionResult> GetByUrl([FromQuery] string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(url);
            }

            try
            {
                var shortcuts = await shortcutService.GetUrlShortcutsByUrlAsync(url);
                return Ok(shortcuts);
            }
            catch (ServiceException ex) when (ex.ResultCode == ServiceResultCode.NotFound)
            {
                return NotFound($"Shortcut for url '{url}' not found.");
            }
        }

        // POST v1/<UrlController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UrlShortcut value)
        {
            try
            {
                var shortcut = await this.shortcutService.CreateUrlShortcutAsync(value);
                return Created(shortcut.Shortcut, shortcut);
            }
            catch (Exception)
            {
                // TODO: Catch more specific exceptions
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // PUT v1/<UrlController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UrlShortcut value)
        {
            if (!string.Equals(id, value.Shortcut, StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(BadRequest());
            }

            throw new NotImplementedException();
        }

        // DELETE v1/<UrlController>/5
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(string id)
        {
            throw new NotImplementedException();
        }
    }
}