using Client.Core.Data.Entities;

namespace Client.Core.Services.StratumService;

public interface IStratumService : IWriteable<Stratum>, IReadable<Stratum>;