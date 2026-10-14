using System;
using UnityEngine;

namespace Kturker.UnityBase.Features.Sample.Network
{
    public interface IGitHubUserCardView
    {
        event Action RetryClicked;
        void ShowLoading();
        void ShowUser(string displayName, string login);
        void ShowAvatar(Texture2D avatar);
        void ShowError(string key, long statusCode);
    }
}
