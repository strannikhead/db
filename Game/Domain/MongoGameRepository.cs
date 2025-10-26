using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        private readonly IMongoCollection<GameEntity> gameCollection;
        public const string CollectionName = "games";

        public MongoGameRepository(IMongoDatabase db)
        {
            gameCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameCollection.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            var game = gameCollection.Find(g => g.Id == gameId).FirstOrDefault();
            return game;
        }

        public void Update(GameEntity game)
        {
            gameCollection.ReplaceOne(g => g.Id == game.Id, game);
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            var games = gameCollection
                .Find(game => game.Status == GameStatus.WaitingToStart)
                .Limit(limit)
                .ToList();
            return games;
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var filter = Builders<GameEntity>.Filter.And(
                Builders<GameEntity>.Filter.Eq(g => g.Id, game.Id),
                Builders<GameEntity>.Filter.Eq(g => g.Status, GameStatus.WaitingToStart)
            );

            var result = gameCollection.ReplaceOne(filter, game);
            return result.IsAcknowledged && result.MatchedCount > 0;
        }
    }
}