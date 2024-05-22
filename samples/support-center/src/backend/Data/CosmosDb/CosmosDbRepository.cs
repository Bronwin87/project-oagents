﻿using Microsoft.Azure.Cosmos;
using SupportCenter.Data.CosmosDb.Entities;
using SupportCenter.Data.CosmosDb.Configurations;

namespace SupportCenter.Data.CosmosDb
{
    public abstract class CosmosDbRepository<TEntity, TOptions>
        where TEntity : Entity
        where TOptions : CosmosDbConfiguration
    {
        protected readonly ILogger Logger;
        protected readonly Container Container;

        protected CosmosDbRepository(TOptions options, ILogger logger)
        {
            Logger = logger;
            CosmosDbConfiguration configuration = options;

            var containerConfiguration = configuration.Containers?.FirstOrDefault(c => c.EntityName == typeof(TEntity).Name)
                ?? throw new InvalidOperationException($"Container configuration for {typeof(TEntity).Name} not found.");

            var client = new CosmosClient(configuration.AccountUri, configuration.AccountKey);
            client.CreateDatabaseIfNotExistsAsync(containerConfiguration.DatabaseName).Wait();

            var database = client.GetDatabase(containerConfiguration.DatabaseName);
            database.CreateContainerIfNotExistsAsync(containerConfiguration.ContainerName, containerConfiguration.PartitionKey ?? "/partitionKey").Wait();

            Container = database.GetContainer(containerConfiguration.ContainerName);
        }

        public async Task<TOutput> GetItemAsync<TOutput>(string id, string partitionKey)
        {
            TOutput item = await Container.ReadItemAsync<TOutput>(id: id, partitionKey: new PartitionKey(partitionKey));
            return item;
        }

        public async Task InsertItemAsync(TEntity entity)
        {
            try
            {
                var response = await Container.CreateItemAsync(entity, new PartitionKey(entity.GetPartitionKeyValue()));
            }
            catch (Exception ex)
            {
                Logger.LogCritical(
                    ex,
                    "An error occurred. MethodName: {methodName} ErrorMessage: {errorMessage}",
                    nameof(InsertItemAsync),
                    ex.Message
                );

                throw;
            }
        }

        public async Task UpsertItemAsync(TEntity entity)
        {
            await Container.UpsertItemAsync(entity, new PartitionKey(entity.GetPartitionKeyValue()));
        }
    }
}
