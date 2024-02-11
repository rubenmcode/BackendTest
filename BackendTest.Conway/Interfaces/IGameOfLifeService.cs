namespace BackendTest.Conway.Interfaces
{
    public interface IGameOfLifeService
    {
        public Task<int> UploadBoardStateAsync(bool[,] initialState);
        public Task<bool[,]> GetNextStateAsync(int boardId);
        public Task<bool[,]> GetStateAfterXStepsAsync(int boardId, int steps);
        public Task<bool[,]> GetFinalStateAsync(int boardId, int maxAttempts);
    }
}
