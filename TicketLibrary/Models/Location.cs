using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;


namespace TicketLibrary.Models
{
    public class Location
    {
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        [Column("Id")]
        public int LocationId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
