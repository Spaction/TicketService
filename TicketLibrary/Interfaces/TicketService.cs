using System.Collections.Generic;
using TicketLibrary.Models;
using System.Threading.Tasks;

namespace TicketLibrary.Interfaces
{
    public interface TicketService
    {
        Task<int> NumSeatsAvailable(int showId);
        Task<ICollection<Seat>> FindAndHoldSeats(int showId, int numSeats, string customerEmail);
        Task<string> ReserverSeats(int showId, ICollection<int> seatIds, string customerEmail);
        
    }
}
