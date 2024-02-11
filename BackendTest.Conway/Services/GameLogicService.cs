using BackendTest.Conway.Interfaces;

namespace BackendTest.Conway.Services
{
    public class GameLogicService : IGameLogicService
    {
        public bool[,] CalculateNextState(bool[,] currentState)
        {
            int rows = currentState.GetLength(0);
            int cols = currentState.GetLength(1);
            bool[,] nextState = new bool[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int liveNeighbors = CountLiveNeighbors(currentState, row, col, rows, cols);

                    if (currentState[row, col] && (liveNeighbors < 2 || liveNeighbors > 3))
                    {
                        nextState[row, col] = false;
                    }
                    else if (!currentState[row, col] && liveNeighbors == 3)
                    {
                        nextState[row, col] = true;
                    }
                    else
                    {
                        nextState[row, col] = currentState[row, col];
                    }
                }
            }

            return nextState;
        }

        public int CountLiveNeighbors(bool[,] board, int row, int col, int rows, int cols)
        {
            int liveNeighbors = 0;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborRow = row + i;
                    int neighborCol = col + j;

                    if (neighborRow >= 0 && neighborRow < rows && neighborCol >= 0 && neighborCol < cols)
                    {
                        liveNeighbors += board[neighborRow, neighborCol] ? 1 : 0;
                    }
                }
            }

            return liveNeighbors;
        }

        public bool AreStatesEqual(bool[,] stateA, bool[,] stateB)
        {
            if (stateA.GetLength(0) != stateB.GetLength(0) || stateA.GetLength(1) != stateB.GetLength(1))
            {
                return false;
            }

            for (int i = 0; i < stateA.GetLength(0); i++)
            {
                for (int j = 0; j < stateA.GetLength(1); j++)
                {
                    if (stateA[i, j] != stateB[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
