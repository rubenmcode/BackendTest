using Microsoft.AspNetCore.Mvc;
using BackendTest.Conway.Interfaces;
using BackendTest.Conway.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BackendTest.Conway.Controllers
{
    [Route("api/boards")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IGameOfLifeService _gameOfLifeService;
        private readonly ILogger<BoardController> _logger;

        public BoardController(IGameOfLifeService gameOfLifeService, ILogger<BoardController> logger)
        {
            _gameOfLifeService = gameOfLifeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadBoardAsync([FromBody] BoardUploadRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UploadBoardAsync called with invalid model state.");
                return BadRequest(ModelState);
            }

            try
            {
                var id = await _gameOfLifeService.UploadBoardStateAsync(request.State);
                _logger.LogInformation("Board {BoardId} uploaded successfully.", id);
                return Created($"{id}/state/next", id);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error uploading board.");
            }
        }

        [HttpGet("{boardId}/next")]
        public async Task<IActionResult> GetNextStateAsync(int boardId)
        {
            return await ExecuteServiceMethodAsync(
                () => _gameOfLifeService.GetNextStateAsync(boardId),
                "Retrieving next state for board {BoardId}.", boardId);
        }

        [HttpGet("{boardId}/{steps}")]
        public async Task<IActionResult> GetStateAfterXStepsAsync(int boardId, int steps)
        {
            if (steps <= 0)
            {
                _logger.LogWarning("GetStateAfterXStepsAsync called with invalid steps: {Steps}", steps);
                return BadRequest("Steps must be positive.");
            }

            return await ExecuteServiceMethodAsync(
                () => _gameOfLifeService.GetStateAfterXStepsAsync(boardId, steps),
                "Retrieving state after {Steps} steps for board {BoardId}.", steps, boardId);
        }

        [HttpGet("{boardId}/final")]
        public async Task<IActionResult> GetFinalStateAsync(int boardId, [FromQuery] int maxAttempts = 100)
        {
            return await ExecuteServiceMethodAsync(
                () => _gameOfLifeService.GetFinalStateAsync(boardId, maxAttempts),
                "Retrieving final state for board {BoardId} with max attempts {MaxAttempts}.", boardId, maxAttempts);
        }

        private async Task<IActionResult> ExecuteServiceMethodAsync(Func<Task<bool[,]>> serviceMethod, string logMessage, params object[] logValues)
        {
            try
            {
                _logger.LogInformation(logMessage, logValues);
                var result = await serviceMethod();
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return HandleException(ex, $"Board with provided ID not found.", logValues);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "An unexpected error occurred.", logValues);
            }
        }

        private IActionResult HandleException(Exception ex, string message, params object[] values)
        {
            _logger.LogError(ex, message, values);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
