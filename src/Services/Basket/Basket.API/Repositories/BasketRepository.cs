using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using ServiceStack.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IRedisClient _redisClient;

        public BasketRepository(IRedisClientsManager redisClientsManager)
        {
            _redisClient = redisClientsManager.GetClient();
        }

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var json = _redisClient.GetValue(userName);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ShoppingCart>(json);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            var json = JsonSerializer.Serialize(basket);
            _redisClient.SetValue(basket.UserName, json);
            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            _redisClient.Remove(userName);
        }
    }
}
