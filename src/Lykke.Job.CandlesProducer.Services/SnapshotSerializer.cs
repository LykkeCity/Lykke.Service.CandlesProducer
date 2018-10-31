using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Services;

namespace Lykke.Job.CandlesProducer.Services
{
    public class SnapshotSerializer<TState> : ISnapshotSerializer
    {
        private readonly IHaveState<TState> _stateHolder;
        private readonly ISnapshotRepository<TState> _repository;
        private readonly ILog _log;

        public SnapshotSerializer(
            IHaveState<TState> stateHolder,
            ISnapshotRepository<TState> repository,
            ILogFactory logFactory)
        {
            _stateHolder = stateHolder;
            _repository = repository;
            _log = logFactory.CreateLog($"{nameof(SnapshotSerializer<TState>)}[{_stateHolder.GetType().Name}]");
        }

        public async Task SerializeAsync()
        {
            _log.Info(nameof(SerializeAsync), "Gettings state...");

            var state = _stateHolder.GetState();

            _log.Info(nameof(SerializeAsync), "Saving state...", _stateHolder.DescribeState(state));

            await _repository.SaveAsync(state);

            _log.Info(nameof(SerializeAsync), "State saved");
        }

        public async Task DeserializeAsync()
        {
            _log.Info(nameof(DeserializeAsync), "Loading state...");
            
            var state = await _repository.TryGetAsync();

            if (state == null)
            {
                _log.Warning(nameof(DeserializeAsync),
                    "No snapshot found to deserialize", context: _stateHolder.GetType().Name);

                return;
            }

            _log.Info(nameof(DeserializeAsync), "Settings state...", _stateHolder.DescribeState(state));
            
            _stateHolder.SetState(state);

            _log.Info(nameof(DeserializeAsync), "State was set");
        }
    }
}
