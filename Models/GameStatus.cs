using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blackjack.Models
{
     // If not static, it resets after every call
    public class GameStatus
    {
       public string DeckId {get; set;}
       public List<Card> DealerCards {get; set;}
       public List<Card> PlayerCards{get; set;}
       public int DealerScore {get; set;}
       public int PlayerScore {get; set;}
       public bool GameOver{get; set;}
       public string? Outcome {get; set;}
       
    }
}