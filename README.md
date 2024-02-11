# Conway's Game of Life API

This project is a .NET 7-based backend service for Conway's Game of Life. It offers an API to manage and interact with the game's board states, providing functionalities to upload initial states, calculate subsequent states, and explore the evolution of the game over time.

## Features

- **Board State Management:** Upload and store initial configurations of the game board.
- **Next State Calculation:** Dynamically compute the next board state based on Conway's Game of Life rules.
- **Future State Projection:** Retrieve the board state after a specified number of generations.
- **Stable State Discovery:** Identify a stable or final state where no changes occur between generations, up to a maximum number of steps.

## Technologies Used

- .NET 7
- Entity Framework
- xUnit for unit testing
- Moq for mocking objects in tests

## Setup and Installation

Ensure the following prerequisites are installed on your machine:
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- A suitable IDE, such as [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)

To set up the project:
1. Clone the repository to your local machine:
    ```
    git clone https://github.com/rubenmcode/BackendTest.git
    ```
2. Navigate to the project directory:
    ```
    cd BackendTest.Conway
    ```
3. Restore the project dependencies:
    ```
    dotnet restore
    ```

## Running the Project

To run the project on your local development machine:
1. Make sure you're in the project directory.
2. Execute the following command to start the application:
    ```
    dotnet run --project BackendTest.Conway
    ```

## Testing

Execute unit tests by running the following command from the solution root:

    ```
    dotnet test
    ```