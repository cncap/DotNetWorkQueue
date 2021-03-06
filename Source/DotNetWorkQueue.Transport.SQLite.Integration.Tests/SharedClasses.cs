﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2017 Brian Lehnen
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// ---------------------------------------------------------------------
using System;
using System.Data.SQLite;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.Messages;
using DotNetWorkQueue.Transport.SQLite.Basic;
using Xunit;

namespace DotNetWorkQueue.Transport.SQLite.Integration.Tests
{
    public static class Helpers
    {
        public static void Verify(string queueName, string connectionString, QueueProducerConfiguration queueProducerConfiguration, long messageCount)
        {
            new VerifyQueueData(queueName, connectionString, queueProducerConfiguration.Options()).Verify(messageCount, null);
        }

        public static void Verify(string queueName, string connectionString, QueueProducerConfiguration queueProducerConfiguration, long messageCount, string route)
        {
            new VerifyQueueData(queueName, connectionString, queueProducerConfiguration.Options()).Verify(messageCount, route);
        }

        public static void Verify(string queueName, string connectionString, long messageCount)
        {
            var connection = new SqliteConnectionInformation(queueName, connectionString);
            var helper = new TableNameHelper(connection);
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"select count(*) from {helper.MetaDataName}";
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.True(reader.Read());
                        var records = reader.GetInt32(0);
                        Assert.Equal(messageCount, records);
                    }
                }
            }
        }

        public static void SetError(string queueName, string connectionString)
        {
            var connection = new SqliteConnectionInformation(queueName, connectionString);
            var helper = new TableNameHelper(connection);
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"update {helper.StatusName} set status = 2";
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void NoVerification(string queueName, string connectionString, QueueProducerConfiguration queueProducerConfiguration, long messageCount)
        {
            
        }

        public static AdditionalMessageData GenerateData(QueueProducerConfiguration configuration)
        {
            if (configuration.Options().EnableMessageExpiration ||
                configuration.Options().EnableDelayedProcessing ||
                configuration.Options().EnablePriority ||
                configuration.Options().AdditionalColumns.Count > 0)
            {
                var data = new AdditionalMessageData();

                if (configuration.Options().EnableMessageExpiration)
                    data.SetExpiration(TimeSpan.FromSeconds(1));

                if (configuration.Options().EnableDelayedProcessing)
                    data.SetDelay(TimeSpan.FromSeconds(5));

                if (configuration.Options().EnablePriority)
                    data.SetPriority(5);

                if (configuration.Options().AdditionalColumns.Count > 0)
                {
                    data.AdditionalMetaData.Add(new AdditionalMetaData<int>("OrderID", 123));
                }

                return data;
            }

            return null;
        }
    }

    public class IncrementWrapper
    {
        public IncrementWrapper()
        {
            ProcessedCount = 0;
        }
        public long ProcessedCount;
    }
}
