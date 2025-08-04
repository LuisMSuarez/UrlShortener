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

        public static IList<UrlShortcut> Shortcuts { get; set; } = new List<UrlShortcut>
        {
            new UrlShortcut
            {
                Shortcut = "xyz",
                Url = "https://gamershub.azurewebsites.net"
            }
        };

        // GET: v1/<UrlController>
        [HttpGet]
        public async Task<IEnumerable<UrlShortcut>> Get()
        {
            var shortcut = await this.shortcutService.GetUrlShortcutAsync("xyz");
            Console.Write(shortcut);
            return Shortcuts;
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

            var shortcut = Shortcuts.FirstOrDefault(s => string.Equals(s.Shortcut, id, StringComparison.OrdinalIgnoreCase));
            if (shortcut == null)
            {
                return await Task.FromResult(NotFound());
            }

            Shortcuts.Remove(shortcut);
            Shortcuts.Add(value);
            return await Task.FromResult(Ok());
        }

        // DELETE v1/<UrlController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var shortcut = Shortcuts.FirstOrDefault(s => string.Equals(s.Shortcut, id, StringComparison.OrdinalIgnoreCase));
            if (shortcut == null)
            {
                return await Task.FromResult(NotFound());
            }

            Shortcuts.Remove(shortcut);
            return await Task.FromResult(Ok());
        }
    }
}