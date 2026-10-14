using Kturker.UnityBase.Tests.EditMode;
using NUnit.Framework;

namespace Kturker.UnityBase.Core.Leaderboards.Tests
{
    public sealed class GatewayTests
    {
        [Test]
        public void The_no_op_gateway_answers_false_to_everything()
        {
            var gateway = new NoGameServicesGateway();

            Assert.That(gateway.StartAsync().GetAwaiter().GetResult(), Is.False);
            Assert.That(gateway.SignInAsync().GetAwaiter().GetResult(), Is.False);
            Assert.That(gateway.SubmitScoreAsync(1).GetAwaiter().GetResult(), Is.False);
            Assert.That(gateway.UnlockAsync(AchievementKeys.FirstCompletion).GetAwaiter().GetResult(), Is.False);
            Assert.That(gateway.IncrementAsync(AchievementKeys.TenCompletions, 1).GetAwaiter().GetResult(), Is.False);
            Assert.That(gateway.ShowLeaderboardAsync().GetAwaiter().GetResult(), Is.False);
            Assert.That(gateway.ShowAchievementsAsync().GetAwaiter().GetResult(), Is.False);
        }

        [Test]
        public void Create_returns_the_no_op_gateway_without_configuration()
        {
            var ids = LeaderboardsConfig.Ids(false);

            Assert.That(GameServicesGateways.Create(false, ids, new FakeLog()), Is.InstanceOf<NoGameServicesGateway>());
            // The editor has no platform behind it, configured or not
            Assert.That(GameServicesGateways.Create(true, ids, new FakeLog()), Is.InstanceOf<NoGameServicesGateway>());
        }
    }
}
