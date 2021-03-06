﻿using System;
using System.Collections.Generic;
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.Route;
using DotNetWorkQueue.Transport.Redis.Basic;
using Xunit;

namespace DotNetWorkQueue.Transport.Redis.IntegrationTests.Route
{
    [Collection("Redis")]
    public class RouteMultiTests
    {
        [Theory]
        [InlineData(100, 1, 400, 1, 2, ConnectionInfoTypes.Linux),
        InlineData(50, 2, 400, 1, 3, ConnectionInfoTypes.Linux),
        InlineData(10, 2, 400, 1, 3, ConnectionInfoTypes.Linux),
        InlineData(50, 2, 400, 1, 2, ConnectionInfoTypes.Windows),
        InlineData(10, 2, 400, 1, 5, ConnectionInfoTypes.Windows),
        InlineData(100, 0, 180, 1, 2, ConnectionInfoTypes.Windows)]
        public void Run(int messageCount, int runtime, int timeOut, int readerCount,
          int routeCount, ConnectionInfoTypes type)
        {
            var queueName = GenerateQueueName.Create();
            var logProvider = LoggerShared.Create(queueName, GetType().Name);
            var connectionString = new ConnectionInfo(type).ConnectionString;
            using (var queueCreator =
                new QueueCreationContainer<RedisQueueInit>(
                    serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
            {
                try
                {

                    using (
                        var oCreation =
                            queueCreator.GetQueueCreation<RedisQueueCreation>(queueName,
                                connectionString)
                        )
                    {
                        var result = oCreation.CreateQueue();
                        Assert.True(result.Success, result.ErrorMessage);

                        var routeTest = new RouteMultiTestsShared();
                        routeTest.RunTest<RedisQueueInit, FakeMessageA>(queueName, connectionString,
                            true, messageCount, logProvider, Helpers.GenerateData, Helpers.Verify, false,
                            GenerateRoutes(routeCount, 1), GenerateRoutes(routeCount, routeCount + 1), runtime, timeOut, readerCount, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(12));

                        using (var count = new VerifyQueueRecordCount(queueName, connectionString))
                        {
                            count.Verify(0, false, -1);
                        }
                    }
                }
                finally
                {
                    using (
                        var oCreation =
                            queueCreator.GetQueueCreation<RedisQueueCreation>(queueName,
                                connectionString)
                        )
                    {
                        oCreation.RemoveQueue();
                    }
                }
            }
        }
        private List<string> GenerateRoutes(int routeCount, int seed)
        {
            var data = new List<string>();
            for (var i = seed; i < routeCount + seed; i++)
            {
                data.Add("Route" + i.ToString());
            }
            return data;
        }
    }
}
