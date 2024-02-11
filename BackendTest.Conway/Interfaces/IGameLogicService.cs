namespace BackendTest.Conway.Interfaces
{
    public interface IGameLogicService
    {
        bool[,] CalculateNextState(bool[,] currentState);
        int CountLiveNeighbors(bool[,] board, int row, int col, int rows, int cols);
        bool AreStatesEqual(bool[,] stateA, bool[,] stateB);
    }
}
