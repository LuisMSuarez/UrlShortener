namespace UrlShortenerApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using UrlShortenerApi.Contracts;
    using UrlShortenerApi.DataAccess;

    [Route("v1/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        private readonly IUrlShortcutRepository repository;
        public UrlsController(IUrlShortcutRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
            var shortcut = await repository.GetUrlShortcutAsync("xyz");
            Console.Write(shortcut);
            return Shortcuts;
        }

        // GET v1/<UrlController>/xyz
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var shortcut = Shortcuts.FirstOrDefault(s => string.Equals(s.Shortcut, id, StringComparison.OrdinalIgnoreCase));
            if (shortcut == null)
            {
                return await Task.FromResult(NotFound());
            }

            return await Task.FromResult(
                Redirect(shortcut.Url));
        }

        // POST v1/<UrlController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UrlShortcut value)
        {
            var shortcut = Shortcuts.FirstOrDefault(s => string.Equals(s.Shortcut, value.Shortcut, StringComparison.OrdinalIgnoreCase));
            if (shortcut != null)
            {
                return await Task.FromResult(BadRequest());
            }

            Shortcuts.Add(value);
            return await Task.FromResult(Created());
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
