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

using System.Collections.Generic;
using DotNetWorkQueue.Transport.SQLite.Basic.Query;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit;
namespace DotNetWorkQueue.Transport.SQLite.Tests.Basic.Query
{
    public class ReceiveMessageQueryTests
    {
        [Fact]
        public void Create_Null_MessageID_Ok()
        {
            var test = new ReceiveMessageQuery(null, new List<string>());
            Assert.Null(test.MessageId);
        }
        [Fact]
        public void Create_Null_Routes_Ok()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var messageid = fixture.Create<IMessageId>();
            var test = new ReceiveMessageQuery(messageid, null);
            Assert.NotNull(test.MessageId);
        }
    }
}
