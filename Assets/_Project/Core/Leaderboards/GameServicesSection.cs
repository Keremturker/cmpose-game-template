using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kturker.UnityBase.Core.Presentation;

namespace Kturker.UnityBase.Core.Leaderboards
{
    // The "Game services" row in Settings: "Not set up" while the service is off (editor, placeholder IDs), "Sign in"
    // while the player is signed out (a press asks to sign in), "Signed in" otherwise. One option at a time, so a
    // press always lands on index 0.
    public sealed class GameServicesSection : ISettingsSection
    {
        private static readonly SettingsOption[] NotSetUp = { SettingsOption.Key(LeaderboardsKeys.NotSetUp) };
        private static readonly SettingsOption[] SignIn = { SettingsOption.Key(LeaderboardsKeys.SignIn) };
        private static readonly SettingsOption[] SignedIn = { SettingsOption.Key(LeaderboardsKeys.SignedIn) };

        private readonly IGameServices services;

        public GameServicesSection(IGameServices services)
        {
            this.services = services;
            services.Changed += OnChanged;
        }

        public string TitleKey => LeaderboardsKeys.Title;
        public IReadOnlyList<SettingsOption> Options => !services.IsAvailable ? NotSetUp : services.IsSignedIn ? SignedIn : SignIn;
        public int Selected => 0;

        public event Action Changed;

        public void Select(int index)
        {
            if (index != 0 || !services.IsAvailable || services.IsSignedIn) return;
            services.SignInAsync(CancellationToken.None).Forget();
        }

        private void OnChanged() => Changed?.Invoke();
    }
}
