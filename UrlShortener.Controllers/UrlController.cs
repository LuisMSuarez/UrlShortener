namespace UrlShortenerApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
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
                if (shortcut == null)
                {
                    return NotFound($"Shortcut with id {id} is not found.");
                }

                return Redirect(shortcut.Url);
            }
            catch (ServiceException ex) when (ex.ResultCode == ServiceResultCode.InternalServerError)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET /{shortcut}
        // This method will respond to requests like GET https://.../XYZ and redirect to the shortcut URL.
        // This route is absolute and will not be affected by the controller's[Route("v1/[controller]")] prefix.
        // It is provided for convenience to allow direct access to shortcuts without needing the full API path,
        // therefore truly providing a shortened URL experience.
        // However, it is important to note that this route will not support versioning, if the contract changes,
        // callers may see changes in the response or behavior.
        [HttpGet("/{shortcut}")]
        public async Task<IActionResult> RedirectByShortcut([FromRoute] string shortcut)
        {
            return await Get(shortcut);
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
            catch (ServiceException ex) when (ex.ResultCode == ServiceResultCode.BadRequest)
            {
                return BadRequest(ex.Message);
            }
            catch (ServiceException ex) when (ex.ResultCode == ServiceResultCode.InternalServerError)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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