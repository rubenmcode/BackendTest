using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BackendTest.Conway.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        public string StateJson { get; set; } = null!;

        [NotMapped]
        public bool[,] State
        {
            get => JsonConvert.DeserializeObject<bool[,]>(StateJson) ?? new bool[0, 0];
            set => StateJson = JsonConvert.SerializeObject(value);
        }

        public DateTime LastUpdated { get; set; }

        public Board(int rows, int columns)
        {
            State = new bool[rows, columns];
            LastUpdated = DateTime.UtcNow;
        }

        public Board() { }
    }
}
