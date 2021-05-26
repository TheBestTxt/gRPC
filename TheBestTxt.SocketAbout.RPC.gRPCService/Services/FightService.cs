using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheBestTxt.SocketAbout.RPC.gRPCService
{
    public class FightService : Fight.FightBase
    {
        private readonly List<string> _attackMethod = new List<string>()
        {
            "飞踢",
            "重拳",
            "精神"
        };

        private readonly ILogger<FightService> _logger;

        public FightService(ILogger<FightService> logger)
        {
            _logger = logger;
        }

        public override Task<AttackResponse> Attack(Empty request, ServerCallContext context)
        {
            Random random = new Random();
            return Task.FromResult(new AttackResponse()
            {
                Message = $"发起了{_attackMethod[random.Next(0, 3)]}攻击"
            });
        }

        public async override Task AttackTogetoher(IAsyncStreamReader<AttackTogetoherRequest> requestStream, IServerStreamWriter<AttackTogetoherResponse> responseStream, ServerCallContext context)
        {
            var list = new List<AttackTogetoherRequest>();
            int i = 1;
            while (await requestStream.MoveNext())
            {
                list.Add(requestStream.Current);
                _logger.LogInformation($"添加第{i.ToString()}次操作完成");
                i++;
            }
            for (int j = 0; j < list.Count; j++)
            {
                _logger.LogInformation($"受到来自客户端的{list[j].Method}攻击");
                Random random = new Random();
                await responseStream.WriteAsync(new AttackTogetoherResponse() { Method = _attackMethod[random.Next(0, 3)] });

                await Task.Delay(1000);
            }
        }
    }
}
