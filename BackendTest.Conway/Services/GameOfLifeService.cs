using BackendTest.Conway.Interfaces;
using BackendTest.Conway.Models;
using BackendTest.Conway.Data;
using System.Collections.Concurrent;

namespace BackendTest.Conway.Services
{
    public class GameOfLifeService : IGameOfLifeService
    {
        private readonly BackendTestConwayContext _dbContext;
        private readonly IGameLogicService _gameLogicService;

        public GameOfLifeService(BackendTestConwayContext dbContext, IGameLogicService gameLogicService)
        {
            _dbContext = dbContext;
            _gameLogicService = gameLogicService;
        }

        public async Task<int> UploadBoardStateAsync(bool[,] initialState)
        {
            var board = new Board
            {
                State = initialState,
                LastUpdated = DateTime.UtcNow
            };
            _dbContext.Board.Add(board);
            await _dbContext.SaveChangesAsync();
            return board.Id;
        }

        public async Task<bool[,]> GetNextStateAsync(int boardId)
        {
            var board = await _dbContext.Board.FindAsync(boardId);
            if (board != null)
            {
                var nextState = _gameLogicService.CalculateNextState(board.State);
                board.State = nextState;
                board.LastUpdated = DateTime.UtcNow;
                return nextState;
            }
            throw new KeyNotFoundException("Board not found.");
        }

        public async Task<bool[,]> GetStateAfterXStepsAsync(int boardId, int steps)
        {
            // Check for negative or zero steps and throw ArgumentException
            if (steps <= 0)
            {
                throw new ArgumentException("Number of steps must be greater than 0.", nameof(steps));
            }

            var board = await _dbContext.Board.FindAsync(boardId);
            if (board != null)
            {
                var state = board.State;
                for (int i = 0; i < steps; i++)
                {
                    state = _gameLogicService.CalculateNextState(state);
                }
                return state;
            }
            throw new KeyNotFoundException("Board not found.");
        }

        public async Task<bool[,]> GetFinalStateAsync(int boardId, int maxAttempts)
        {
            var board = await _dbContext.Board.FindAsync(boardId);
            if (board != null)
            {
                var currentState = board.State;
                bool hasChanged;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var nextState = _gameLogicService.CalculateNextState(currentState);
                    hasChanged = !_gameLogicService.AreStatesEqual(currentState, nextState);

                    if (!hasChanged)
                    {
                        // No change in state, considered stable/final
                        return nextState;
                    }

                    currentState = nextState;
                }

                // Max attempts reached without finding a stable state
                return currentState;
            }
            throw new KeyNotFoundException("Board not found.");
        }
    }
}
