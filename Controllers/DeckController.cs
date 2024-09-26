using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blackjack.Models;
using Blackjack.Services;
using Microsoft.AspNetCore.Mvc;

namespace Blackjack.Controllers
{
    [ApiController]
    // When you make the calls it goes (website url)/blackjack
    [Route("blackjack")]
    public class DeckController : ControllerBase
    {
        static GameStatus status = new GameStatus();
        private readonly DeckService _service;
        public DeckController(DeckService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<IActionResult> GetGame()
        {
            // Validation that checks if a game has been started
            if(status.DeckId == null)
            {
                return NotFound("Game not started");
            }
            return Ok(status);
        }

        [HttpPost()]
        public async Task<IActionResult> NewGame()
        {
            if(status.GameOver == false && status.DeckId != null)
            {
                return Conflict("Game still in progress");
            }
            DeckModel newDeck = await _service.NewDeck();
            status = new GameStatus();
            // Anytime you set up a new game, it resets
            status.DeckId = newDeck.deck_id;
            DeckModel resultCards = await _service.DrawCards(3, status.DeckId);
            status.DealerCards = new List<Card>() {resultCards.cards[0]};
            status.PlayerCards = new List<Card>() {resultCards.cards[1], resultCards.cards[2]};
            status.DealerScore = GetCardScore(resultCards.cards[0]);
            status.PlayerScore = GetCardScore(resultCards.cards[1]) + GetCardScore(resultCards.cards[2]);
            status.GameOver = false;
            status.Outcome = "";

            // Incase player gets 2 aces from start, lower values
            status.PlayerScore = LowerAceValues(status.PlayerCards, status.PlayerScore);

             if(status.PlayerScore > 21)
                {
                    status.GameOver = true;
                    status.Outcome = "Bust";
                }

            return Created("", status);
        }

        // "play" adds it to the end of our route: blackjack/play
        [HttpPost("play")]
        public async Task<IActionResult> GameAction(string action)
        {
            if(status.DeckId == null)
            {
                return NotFound("Game not started");
            }
            if(status.GameOver == true)
            {
                return Conflict("Game is over, sorry! Try starting a new one!");
            }

            if(action == "hit")
            {
                DeckModel cardsResult = await _service.DrawCards(1, status.DeckId);
                status.PlayerCards.Add(cardsResult.cards[0]);
                status.PlayerScore += GetCardScore(cardsResult.cards[0]);

                //use the lowering aces method
                status.PlayerScore = LowerAceValues(status.PlayerCards, status.PlayerScore);

                if(status.PlayerScore > 21)
                {
                    status.GameOver = true;
                    status.Outcome = "Bust";
                }
            }
            else if(action == "stand")
            {
                // Going off of googling blackjack rules, standing ends player's turn, and
                // The dealer keeps drawing until they get at least a value of 17 or more
                // So I put this section in a while loop
                // I think that's right idk
                while(status.DealerScore < 17)
                {
                    DeckModel cardsResult = await _service.DrawCards(1, status.DeckId);
                    status.DealerCards.Add(cardsResult.cards[0]);
                    status.DealerScore += GetCardScore(cardsResult.cards[0]);
                    status.DealerScore = LowerAceValues(status.DealerCards, status.DealerScore);
                }

                // Once dealer's turn is over, determine outcome of the game
                if(status.DealerScore > 21)
                {
                    // Dealer went over 21, you win
                    status.GameOver = true;
                    status.Outcome = "Win";
                }
                else if(status.DealerScore > status.PlayerScore)
                {
                    // Dealer has higher score, you lose
                    status.GameOver = true;
                    status.Outcome = "Loss";
                }
                else if(status.DealerScore < status.PlayerScore)
                {
                    // Player has higher score, you win
                    status.GameOver = true;
                    status.Outcome = "Win";
                }
                else if(status.DealerScore == status.PlayerScore )
                {
                    // Tie game
                    status.GameOver = true;
                    status.Outcome = "Standoff";
                }
            }
            else
            {
                 return NotFound("Invalid action");
            }
            return Ok(status);
        }

        private int GetCardScore(Card c)
        {
            if (c.value == "ACE")
            {
                return 11;
            }
            else if(c.value == "KING" || c.value == "QUEEN" || c.value == "JACK")
            {
                return 10;
            }
            else if(c.value == "JOKER")
            {
                return 0;
            }
            else
            {
                return int.Parse(c.value);
            }
        }
        // Lower Ace values if the current score is greater than 21
        private int LowerAceValues(List<Card> cards, int currentScore)
        {
            // Gets number of aces in current hand
            int aceCount = cards.Count(c => c.value == "ACE");

            // Keeps lowering the score while the user has aces and a value
            // Greater than 21
            while (currentScore > 21 && aceCount > 0)
            {
                currentScore -= 10;
                aceCount--;
            }

            return currentScore;
        }
    }
}