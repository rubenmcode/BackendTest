using BackendTest.Conway.Interfaces;
using BackendTest.Conway.Services;
using Xunit;

namespace BackendTest.Conway.UnitTests.ServicesTests
{
    public class GameLogicServiceTests
    {
        private readonly IGameLogicService _gameLogicService;

        public GameLogicServiceTests()
        {
            _gameLogicService = new GameLogicService();
        }

        [Fact]
        public void CalculateNextState_ShouldReturnCorrectNextState()
        {
            bool[,] currentState = new bool[,]
            {
                { false, false, false },
                { false, true, true },
                { false, true, true },
                { false, false, false }
            };

            bool[,] expectedNextState = new bool[,]
            {
                { false, false, false },
                { false, true, true },
                { false, true, true },
                { false, false, false }
            };

            bool[,] actualNextState = _gameLogicService.CalculateNextState(currentState);

            Assert.Equal(expectedNextState, actualNextState);
        }

        [Fact]
        public void CalculateNextState_GivenEmptyBoard_ReturnsEmptyBoard()
        {
            bool[,] emptyBoard = new bool[3, 3];

            var nextState = _gameLogicService.CalculateNextState(emptyBoard);

            Assert.True(_gameLogicService.AreStatesEqual(emptyBoard, nextState), "The next state of an empty board should also be empty.");
        }

        [Fact]
        public void CalculateNextState_GivenAllAliveBoard_ReturnsCorrectNextState()
        {
            bool[,] allAliveBoard = new bool[,]
            {
                { true, true, true },
                { true, true, true },
                { true, true, true }
            };

            bool[,] expectedState = new bool[,]
            {
                { true, false, true },
                { false, false, false },
                { true, false, true }
            };

            var nextState = _gameLogicService.CalculateNextState(allAliveBoard);

            Assert.True(_gameLogicService.AreStatesEqual(expectedState, nextState), "The next state should follow the overpopulation rule.");
        }

        [Fact]
        public void CalculateNextState_GivenBoardWithEdgeLife_ReturnsCorrectNextState()
        {
            bool[,] boardWithEdgeLife = new bool[,]
            {
                { false, false, false, false },
                { true, true, true, false },
                { false, false, false, false },
                { false, false, false, false }
            };

            bool[,] expectedState = new bool[,]
            {
                { false, true, false, false },
                { false, true, false, false },
                { false, true, false, false },
                { false, false, false, false }
            };

            var nextState = _gameLogicService.CalculateNextState(boardWithEdgeLife);

            Assert.True(_gameLogicService.AreStatesEqual(expectedState, nextState), "The next state should correctly evolve cells at the edge.");
        }

        [Fact]
        public void CountLiveNeighbors_ShouldReturnCorrectLiveNeighborsCount()
        {
            bool[,] board = new bool[,]
            {
                { true, true, false },
                { false, true, false },
                { false, false, true }
            };

            int row = 1;
            int col = 1;
            int expectedLiveNeighborsCount = 3;

            int actualLiveNeighborsCount = _gameLogicService.CountLiveNeighbors(board, row, col, board.GetLength(0), board.GetLength(1));

            Assert.Equal(expectedLiveNeighborsCount, actualLiveNeighborsCount);
        }

        [Fact]
        public void AreStatesEqual_ShouldReturnTrueForEqualStates()
        {
            bool[,] stateA = new bool[,]
            {
                { true, true, false },
                { false, true, false },
                { false, false, true }
            };

            bool[,] stateB = new bool[,]
            {
                { true, true, false },
                { false, true, false },
                { false, false, true }
            };

            bool areEqual = _gameLogicService.AreStatesEqual(stateA, stateB);

            Assert.True(areEqual);
        }

        [Fact]
        public void CalculateNextState_Underpopulation_RuleApplied()
        {
            bool[,] initialState = new bool[,]
            {
                { false, false, false },
                { true, true, false },
                { false, false, false }
            };

            bool[,] expectedState = new bool[,]
            {
                { false, false, false },
                { false, false, false },
                { false, false, false }
            };

            var nextState = _gameLogicService.CalculateNextState(initialState);

            Assert.True(_gameLogicService.AreStatesEqual(expectedState, nextState), "Cells with fewer than two live neighbors should die.");
        }

        [Fact]
        public void CalculateNextState_Survival_RuleApplied()
        {
            bool[,] initialState = new bool[,]
            {
                { false, true, false },
                { false, true, true },
                { false, false, false }
            };

            bool[,] expectedState = new bool[,]
            {
                { false, true, true },
                { false, true, true },
                { false, false, false }
            };

            var nextState = _gameLogicService.CalculateNextState(initialState);

            Assert.True(_gameLogicService.AreStatesEqual(expectedState, nextState), "Cells with two or three live neighbors should survive.");
        }

        [Fact]
        public void CalculateNextState_Overpopulation_RuleApplied()
        {
            bool[,] initialState = new bool[,]
            {
                { true, true, true },
                { true, true, true },
                { true, true, true }
            };

            bool[,] expectedState = new bool[,]
            {
                { true, false, true },
                { false, false, false },
                { true, false, true }
            };

            var nextState = _gameLogicService.CalculateNextState(initialState);

            Assert.True(_gameLogicService.AreStatesEqual(expectedState, nextState), "Cells with more than three live neighbors should die.");
        }

        [Fact]
        public void CalculateNextState_Reproduction_RuleApplied()
        {
            bool[,] initialState = new bool[,]
            {
                { false, false, false },
                { false, false, true },
                { false, true, true }
            };

            bool[,] expectedState = new bool[,]
            {
                { false, false, false },
                { false, true, true },
                { false, true, true }
            };

            var nextState = _gameLogicService.CalculateNextState(initialState);

            Assert.True(_gameLogicService.AreStatesEqual(expectedState, nextState), "A dead cell with exactly three live neighbors should become a live cell.");
        }

        [Fact]
        public void AreStatesEqual_ShouldReturnFalseForDifferentStates()
        {
            bool[,] stateA = new bool[,]
            {
                { true, true, false },
                { false, true, false },
                { false, false, true }
            };

            bool[,] stateB = new bool[,]
            {
                { true, true, false },
                { false, false, false },
                { false, false, true }
            };

            bool areEqual = _gameLogicService.AreStatesEqual(stateA, stateB);

            Assert.False(areEqual);
        }
    }
}
