using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace TicketLibrary.Models
{
    public class Show
    {
        [Column("Id")]
        public int ShowId { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [NotMapped]
        public int AvailableSeats
        {
            get
            {
                if (Seats != null)
                    return Seats.Count(x => x.Available);
                else
                    return _avail;
            }
            set
            {
                _avail = value;
            }

        }

        [NotMapped]
        public int _avail { get; set; }

        public List<Seat> Seats { get; set; }

    }
}
