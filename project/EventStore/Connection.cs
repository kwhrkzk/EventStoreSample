using System;
using System.Text;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System.Threading.Tasks;
using Utf8Json;
using Microsoft.Extensions.Logging;
using RentalUsecase;
using Unity;

namespace EventStore
{
    public static class Connection
    {
#if DEBUG
        public static UserCredentials UserCredentials() => new UserCredentials("admin", "changeit");
        public static Uri EventStoreUri() => new Uri("tcp://localhost:1113");
#else
        public static UserCredentials UserCredentials() => new UserCredentials("admin", "changeit");
        public static Uri EventStoreUri() => new Uri("tcp://eventstore:1113");
#endif
    }
}
