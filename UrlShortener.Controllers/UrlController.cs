namespace UrlShortenerApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using UrlShortenerApi.Services;
    using UrlShortenerApi.Services.Contracts;

    /// <summary>
    /// Controller for managing URL shortcuts.
    /// Provides endpoints to create, retrieve, and redirect shortened URLs.
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class UrlsController : ControllerBase
    {
        private readonly IUrlShortcutService shortcutService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlsController"/> class.
        /// </summary>
        /// <param name="shortcutService">Service for handling URL shortcut operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shortcutService"/> is null.</exception>
        public UrlsController(IUrlShortcutService shortcutService)
        {
            this.shortcutService = shortcutService ?? throw new ArgumentNullException(nameof(shortcutService));
        }

        /// <summary>
        /// Retrieves the original URL associated with a given shortcut ID and redirects to it.
        /// </summary>
        /// <param name="id">The shortcut identifier.</param>
        /// <returns>Redirects to the original URL or returns an error response.</returns>
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

        /// <summary>
        /// Redirects to the original URL using a shortcut path segment.
        /// This route bypasses the controller prefix for direct access.
        /// </summary>
        /// <param name="shortcut">The shortcut string.</param>
        /// <returns>Redirects to the original URL or returns an error response.</returns>
        [HttpGet("/{shortcut}")]
        public async Task<IActionResult> RedirectByShortcut([FromRoute] string shortcut)
        {
            return await Get(shortcut);
        }

        /// <summary>
        /// Retrieves all shortcuts associated with a given original URL.
        /// </summary>
        /// <param name="url">The original URL to search for.</param>
        /// <returns>A list of matching shortcuts or an error response.</returns>
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

        /// <summary>
        /// Creates a new URL shortcut.
        /// </summary>
        /// <param name="value">The shortcut object containing the original URL and desired shortcut.</param>
        /// <returns>The created shortcut or an error response.</returns>
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

        /// <summary>
        /// Updates an existing URL shortcut.
        /// </summary>
        /// <param name="id">The shortcut identifier.</param>
        /// <param name="value">The updated shortcut object.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <exception cref="NotImplementedException">Always thrown; method not yet implemented.</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UrlShortcut value)
        {
            if (!string.Equals(id, value.Shortcut, StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(BadRequest());
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a URL shortcut by ID.
        /// </summary>
        /// <param name="id">The shortcut identifier.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <exception cref="NotImplementedException">Always thrown; method not yet implemented.</exception>
        [HttpDelete("{id}")]
        public Task<IActionResult> Delete(string id)
        {
            throw new NotImplementedException();
        }
    }
}
