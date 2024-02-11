using BackendTest.Conway.Data;
using BackendTest.Conway.Interfaces;
using BackendTest.Conway.Models;
using BackendTest.Conway.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BackendTest.Conway.UnitTests.ServicesTests
{
    public class GameOfLifeServiceTests
    {
        private readonly Mock<IGameLogicService> _mockGameLogicService;
        private BackendTestConwayContext _dbContext;
        private GameOfLifeService _gameOfLifeService;

        public GameOfLifeServiceTests()
        {
            // Setup in-memory database context
            var options = new DbContextOptionsBuilder<BackendTestConwayContext>()
                .UseInMemoryDatabase(databaseName: "GameOfLifeTestDb")
                .Options;
            _dbContext = new BackendTestConwayContext(options);

            _mockGameLogicService = new Mock<IGameLogicService>();

            _gameOfLifeService = new GameOfLifeService(_dbContext, _mockGameLogicService.Object);

            // Ensure the database is clean before each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task UploadBoardStateAsync_StoresStateCorrectly_ReturnsNewBoardId()
        {
            var initialState = new bool[,] { { true, false }, { false, true } };

            var boardId = await _gameOfLifeService.UploadBoardStateAsync(initialState);

            var storedBoard = await _dbContext.Board.FindAsync(boardId);
            Assert.NotNull(storedBoard);
            Assert.Equal(initialState, storedBoard.State);
        }

        [Fact]
        public async Task GetNextStateAsync_WithValidBoardId_ReturnsCalculatedNextState()
        {
            var initialState = new bool[,] { { false, true }, { true, false } };
            var expectedNextState = new bool[,] { { false, false }, { false, false } };

            // Setting up the mock to return a specific next state
            _mockGameLogicService.Setup(s => s.CalculateNextState(It.IsAny<bool[,]>())).Returns(expectedNextState);

            // Simulating board state upload
            var board = new Board { State = initialState, LastUpdated = DateTime.UtcNow };
            await _dbContext.Board.AddAsync(board);
            await _dbContext.SaveChangesAsync();

            var nextState = await _gameOfLifeService.GetNextStateAsync(board.Id);

            Assert.Equal(expectedNextState, nextState);
        }

        [Fact]
        public async Task GetStateAfterXStepsAsync_WithValidBoardIdAndSteps_ReturnsStateAfterXSteps()
        {
            var initialState = new bool[,] { { true, false }, { false, true } };
            var intermediateState = new bool[,] { { false, false }, { false, true } };
            var finalState = new bool[,] { { false, false }, { false, false } };
            var steps = 2;

            // Mock the GameLogicService to simulate state changes over steps
            _mockGameLogicService.SetupSequence(s => s.CalculateNextState(It.IsAny<bool[,]>()))
                .Returns(intermediateState)
                .Returns(finalState);

            // Simulating board state upload
            var board = new Board { State = initialState, LastUpdated = DateTime.UtcNow };
            await _dbContext.Board.AddAsync(board);
            await _dbContext.SaveChangesAsync();

            var stateAfterXSteps = await _gameOfLifeService.GetStateAfterXStepsAsync(board.Id, steps);

            Assert.Equal(finalState, stateAfterXSteps);
        }

        [Fact]
        public async Task GetFinalStateAsync_WithStateBecomingStable_ReturnsStableState()
        {
            var initialState = new bool[,] { { false, true, false }, { false, true, false }, { false, true, false } };
            var stableState = new bool[,] { { false, false, false }, { true, true, true }, { false, false, false } };
            var maxAttempts = 10; // Arbitrary number of steps to simulate progression to a stable state

            // Setup to simulate a state change followed by stability
            _mockGameLogicService.SetupSequence(s => s.CalculateNextState(It.IsAny<bool[,]>()))
                .Returns(stableState)
                .Returns(stableState); // Stable state, no change on subsequent calls

            _mockGameLogicService.Setup(s => s.AreStatesEqual(It.IsAny<bool[,]>(), It.IsAny<bool[,]>()))
                .Returns((bool[,] prevState, bool[,] nextState) => prevState.Rank == nextState.Rank &&
                    Enumerable.Range(0, prevState.Rank).All(dimension => prevState.GetLength(dimension) == nextState.GetLength(dimension)) &&
                    Enumerable.Range(0, prevState.GetLength(0)).All(i => Enumerable.Range(0, prevState.GetLength(1)).All(j => prevState[i, j] == nextState[i, j])));

            // Simulating board state upload
            var board = new Board { State = initialState, LastUpdated = DateTime.UtcNow };
            await _dbContext.Board.AddAsync(board);
            await _dbContext.SaveChangesAsync();

            var finalState = await _gameOfLifeService.GetFinalStateAsync(board.Id, maxAttempts);

            Assert.Equal(stableState, finalState);
        }

        [Fact]
        public async Task GetNextStateAsync_WithInvalidBoardId_ThrowsKeyNotFoundException()
        {
            var invalidBoardId = 999; // Assuming this ID does not exist

            // Expecting a KeyNotFoundException due to the invalid board ID
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _gameOfLifeService.GetNextStateAsync(invalidBoardId));
        }

        [Fact]
        public async Task GetStateAfterXStepsAsync_WithNonPositiveSteps_ThrowsArgumentException()
        {
            var boardId = 1; // Assuming this is a valid board ID for the sake of the test
            var invalidSteps = 0; // Zero steps are invalid

            // Setup a valid board in the context for the test
            var initialState = new bool[,] { { false, false }, { false, false } };
            await _dbContext.Board.AddAsync(new Board { Id = boardId, State = initialState, LastUpdated = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            // Expecting an ArgumentException due to the invalid number of steps
            await Assert.ThrowsAsync<ArgumentException>(() => _gameOfLifeService.GetStateAfterXStepsAsync(boardId, invalidSteps));
        }

        [Fact]
        public async Task GetFinalStateAsync_WithMaxAttemptsReached_ReturnsLastCalculatedState()
        {
            var boardId = 1;
            var initialState = new bool[,] { { true, false }, { false, true } };
            var nextState = new bool[,] { { false, true }, { true, false } }; // Simulate oscillation
            var maxAttempts = 5;

            _mockGameLogicService.Setup(s => s.CalculateNextState(It.IsAny<bool[,]>())).Returns(nextState);
            _mockGameLogicService.Setup(s => s.AreStatesEqual(It.IsAny<bool[,]>(), It.IsAny<bool[,]>())).Returns(false); // Always returns false to simulate constant change

            await _dbContext.Board.AddAsync(new Board { Id = boardId, State = initialState, LastUpdated = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            var finalState = await _gameOfLifeService.GetFinalStateAsync(boardId, maxAttempts);

            // The service should return the last calculated state after reaching the maximum number of attempts
            Assert.Equal(nextState, finalState);
        }

        [Fact]
        public async Task GetStateAfterXStepsAsync_CallsCalculateNextStateCorrectNumberOfTimes()
        {
            var boardId = 1;
            var initialState = new bool[,] { { true, false }, { false, true } };
            var steps = 3; // Number of steps to evolve the board

            // Setup initial board state in the in-memory database
            await _dbContext.Board.AddAsync(new Board { Id = boardId, State = initialState, LastUpdated = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            // Setup the mock to return any state, as the actual state evolution is not the focus of this test
            _mockGameLogicService.Setup(s => s.CalculateNextState(It.IsAny<bool[,]>())).Returns(initialState);

            await _gameOfLifeService.GetStateAfterXStepsAsync(boardId, steps);

            // Verify CalculateNextState was called exactly 'steps' times
            _mockGameLogicService.Verify(s => s.CalculateNextState(It.IsAny<bool[,]>()), Times.Exactly(steps));
        }

        [Fact]
        public async Task GetFinalStateAsync_CallsAreStatesEqualToDetermineStability()
        {
            var boardId = 1;
            var initialState = new bool[,] { { true, false }, { false, true } };
            var stableState = initialState; // Simulate immediate stability for simplicity
            var maxAttempts = 5;

            // Setup initial board state
            await _dbContext.Board.AddAsync(new Board { Id = boardId, State = initialState, LastUpdated = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            // Mock logic service to return the same state, simulating a stable configuration
            _mockGameLogicService.Setup(s => s.CalculateNextState(It.IsAny<bool[,]>())).Returns(stableState);
            _mockGameLogicService.Setup(s => s.AreStatesEqual(It.IsAny<bool[,]>(), It.IsAny<bool[,]>())).Returns(true);

            await _gameOfLifeService.GetFinalStateAsync(boardId, maxAttempts);

            // Since the state is immediately stable, AreStatesEqual should be called once to confirm stability
            _mockGameLogicService.Verify(s => s.AreStatesEqual(It.IsAny<bool[,]>(), It.IsAny<bool[,]>()), Times.AtLeastOnce());
        }
    }
}
