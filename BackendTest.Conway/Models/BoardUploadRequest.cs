using System.ComponentModel.DataAnnotations;

namespace BackendTest.Conway.Models
{
    public class BoardUploadRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Board state is required.")]
        public required bool[,] State { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Example: Ensure the board is not too large
            int maxRows = 100; // Max allowed rows
            int maxCols = 100; // Max allowed columns
            if (State.GetLength(0) > maxRows || State.GetLength(1) > maxCols)
            {
                yield return new ValidationResult($"Board size must not exceed {maxRows}x{maxCols}.");
            }
        }
    }
}
