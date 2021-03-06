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
using System.Collections.Generic;
using DotNetWorkQueue.QueueStatus;
using DotNetWorkQueue.Transport.SQLite.Basic.Query;

namespace DotNetWorkQueue.Transport.SQLite.Basic
{
    /// <summary>
    /// Returns status information about the current queue
    /// </summary>
    internal class SqLiteQueueStatusProvider: QueueStatusProviderBase
    {
        private readonly Lazy<SqLiteMessageQueueTransportOptions> _options;
        private readonly IInternalSerializer _serializer;
        private readonly SqLiteMessageQueueStatusQueries _queries;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqLiteQueueStatusProvider" /> class.
        /// </summary>
        /// <param name="connectionInformation">The connection information.</param>
        /// <param name="getTimeFactory">The get time factory.</param>
        /// <param name="optionsFactory">The options factory.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="queries">The queries.</param>
        public SqLiteQueueStatusProvider(IConnectionInformation connectionInformation,
            IGetTimeFactory getTimeFactory,
            ISqLiteMessageQueueTransportOptionsFactory optionsFactory,
            IInternalSerializer serializer, 
            SqLiteMessageQueueStatusQueries queries) : base(connectionInformation, getTimeFactory)
        {
            _serializer = serializer;
            _queries = queries;
            _options = new Lazy<SqLiteMessageQueueTransportOptions>(optionsFactory.Create);
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<SystemEntry> GetData()
        {
            yield return GetPendingCount();
            yield return GetPendingDelayCount();
            yield return GetWorkingCount();
            yield return GetErrorCount();
            yield return GetConfiguration();
        }

        /// <summary>
        /// Gets the pending count.
        /// </summary>
        /// <returns></returns>
        private SystemEntry GetPendingCount()
        {
            const string name = "Pending";
            const string nameDelay = "PendingDelayExcluded";
            if (_options.Value.EnableDelayedProcessing && _options.Value.EnableStatus)
            {
                try
                {
                    return new SystemEntry(nameDelay, _queries.PendingExcludeDelayQueryHandler.Handle(new GetPendingExcludeDelayCountQuery()).ToString());
                }
                catch (Exception error)
                {
                    SetError(error);
                    return new SystemEntry(name, error.ToString());
                }
            }
            else if (_options.Value.EnableStatusTable)
            {
                try
                {
                    return new SystemEntry(name, _queries.PendingQueryHandler.Handle(new GetPendingCountQuery()).ToString());
                }
                catch (Exception error)
                {
                    SetError(error);
                    return new SystemEntry(name, error.ToString());
                }
            }
            return new SystemEntry(name, "The status table must be enabled to get a count; set EnableStatusTable to true when creating queues to enable this.");
        }

        /// <summary>
        /// Gets the pending delay count.
        /// </summary>
        /// <returns></returns>
        private SystemEntry GetPendingDelayCount()
        {
            const string name = "DelayedPending";
            if (_options.Value.EnableDelayedProcessing && _options.Value.EnableStatus)
            {
                try
                {
                    return new SystemEntry(name, _queries.PendingDelayedQueryHandler.Handle(new GetPendingDelayedCountQuery()).ToString());
                }
                catch (Exception error)
                {
                    SetError(error);
                    return new SystemEntry(name, error.ToString());
                }
            }
            if (_options.Value.EnableDelayedProcessing)
            {
                return new SystemEntry(name, "The status field must be enabled to get a count; set EnableStatus to true when creating queues to enable this.");
            }
            return new SystemEntry(name, "0");
        }

        /// <summary>
        /// Gets the working count.
        /// </summary>
        /// <returns></returns>
        private SystemEntry GetWorkingCount()
        {
            const string name = "Working";
            if (_options.Value.EnableStatusTable)
            {
                try
                {
                    return new SystemEntry(name, _queries.WorkingQueryHandler.Handle(new GetWorkingCountQuery()).ToString());
                }
                catch (Exception error)
                {
                    SetError(error);
                    return new SystemEntry(name, error.ToString());
                }
            }
            return new SystemEntry(name, "The status table must be enabled to get a count; set EnableStatusTable to true when creating queues to enable this.");
        }

        /// <summary>
        /// Gets the error count.
        /// </summary>
        /// <returns></returns>
        private SystemEntry GetErrorCount()
        {
            const string name = "Error";
            if (_options.Value.EnableStatusTable)
            {
                try
                {
                    return new SystemEntry(name, _queries.ErrorQueryHandler.Handle(new GetErrorCountQuery()).ToString());
                }
                catch (Exception error)
                {
                    SetError(error);
                    return new SystemEntry(name, error.ToString());
                }
            }
            return new SystemEntry(name, "The status table must be enabled to get a count; set EnableStatusTable to true when creating queues to enable this.");
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        private SystemEntry GetConfiguration()
        {
            const string name = "Configuration";
            try
            {
                return new SystemEntry(name, _serializer.ConvertToString(_options.Value));
            }
            catch (Exception error)
            {
                SetError(error);
                return new SystemEntry(name, error.ToString());
            }
        }
    }
}
