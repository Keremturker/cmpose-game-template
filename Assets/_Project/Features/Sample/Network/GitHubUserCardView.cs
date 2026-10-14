using System;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Network;
using Kturker.UnityBase.Core.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Sample.Network
{
    // Prefab under Resources (SceneBuilder builds it); Spawn puts it into the sample's extra slot and binds it
    public sealed class GitHubUserCardView : MonoBehaviour, IGitHubUserCardView
    {
        public const string ResourcePath = "SampleNetwork/GitHubUserCard";

        [SerializeField] private LocalizedText nameText;
        [SerializeField] private LocalizedText loginText;
        [SerializeField] private LocalizedText statusText;
        [SerializeField] private Button retryButton;
        [SerializeField] private RawImage avatar;

        public event Action RetryClicked;

        public static GitHubUserCardView Spawn(Transform slot, UiBinder binder)
        {
            var prefab = Resources.Load<GitHubUserCardView>(ResourcePath);
            if (prefab == null) throw new InvalidOperationException($"missing Resources/{ResourcePath}");
            var view = Instantiate(prefab, slot, false);
            binder.BindTree(view.gameObject);
            return view;
        }

        private void Awake() => retryButton.onClick.AddListener(OnRetry);

        private void OnDestroy() => retryButton.onClick.RemoveListener(OnRetry);

        public void ShowLoading()
        {
            ShowUserRows(false);
            avatar.enabled = false;
            statusText.gameObject.SetActive(true);
            statusText.SetKey(NetworkKeys.Loading);
            retryButton.gameObject.SetActive(false);
        }

        public void ShowUser(string displayName, string login)
        {
            ShowUserRows(true);
            nameText.SetLiteral(displayName);
            loginText.SetLiteral("@" + login);
            statusText.gameObject.SetActive(false);
            retryButton.gameObject.SetActive(false);
        }

        public void ShowAvatar(Texture2D texture)
        {
            avatar.texture = texture;
            avatar.enabled = true;
        }

        public void ShowError(string key, long statusCode)
        {
            ShowUserRows(false);
            statusText.gameObject.SetActive(true);
            statusText.SetKey(key, statusCode);
            retryButton.gameObject.SetActive(true);
        }

        private void ShowUserRows(bool visible)
        {
            nameText.gameObject.SetActive(visible);
            loginText.gameObject.SetActive(visible);
        }

        private void OnRetry() => RetryClicked?.Invoke();
    }
}
