using System;
using TheBestTxt.SocketAbout.RPC.gRPCService;
using Grpc.Net.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using Grpc.Core;

namespace TheBestTxt.SocketAbout.RPC.gRPCClient
{
    class Program
    {
        private static List<string> _attackMethod = new List<string>()
        {
            "飞踢",
            "重拳",
            "精神"
        };

        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                new HelloRequest { Name = "Txt" });
            Console.WriteLine("调用Greeter服务 : " + reply.Message);

            var fight = new Fight.FightClient(channel);
            var fightReply = await fight.AttackAsync(new Google.Protobuf.WellKnownTypes.Empty());
            Console.WriteLine("调用Fight服务 : " + fightReply.Message);

            var fightTogether = fight.AttackTogetoher();
            for (int i = 0; i < 10; i++)
            {
                Random random = new Random();
                await fightTogether.RequestStream.WriteAsync(new AttackTogetoherRequest { Method = _attackMethod[random.Next(0, 3)] });
            }

            var fightResponseRead = Task.Run(async () =>
            {
                try
                {
                    await foreach (var resp in fightTogether.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"受到了来自Fight服务端的{resp.Method}攻击");
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Stream cancelled.");
                }
            });

            await fightTogether.RequestStream.CompleteAsync();
            Console.WriteLine("发起攻击完毕");
            await fightResponseRead;

            Console.ReadKey();

        }
    }
}
