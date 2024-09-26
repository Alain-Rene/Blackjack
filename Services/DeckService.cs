using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blackjack.Models;

namespace Blackjack.Services
{
    public class DeckService
    {
        private readonly HttpClient _httpClient;
        public DeckService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://www.deckofcardsapi.com/api/deck/");
                                             //https://www.deckofcardsapi.com/api/deck/new/
        }

        public async Task<DeckModel> NewDeck()
        {
            DeckModel result = await _httpClient.GetFromJsonAsync<DeckModel>("new/shuffle");
            return result;

        }

        public async Task<DeckModel> DrawCards(int count, string DeckId)
        {
            // You use the deckId so that you keep pulling from the same deck of cards, and count
            // Is how many cards you're taking
            DeckModel result = await _httpClient.GetFromJsonAsync<DeckModel>($"{DeckId}/draw/?count={count}");
            return result;
        }
    }
}