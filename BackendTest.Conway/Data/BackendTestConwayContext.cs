using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackendTest.Conway.Models;

namespace BackendTest.Conway.Data
{
    public class BackendTestConwayContext : DbContext
    {
        public BackendTestConwayContext (DbContextOptions<BackendTestConwayContext> options)
            : base(options)
        {
        }

        public DbSet<BackendTest.Conway.Models.Board> Board { get; set; } = default!;
    }
}
