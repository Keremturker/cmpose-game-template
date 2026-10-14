using System;
using System.Collections.Generic;
using Kturker.UnityBase.Core.Localization;
using Kturker.UnityBase.Core.Presentation;
using Kturker.UnityBase.Core.Purchases;
using UnityEngine;
using UnityEngine.UI;

namespace Kturker.UnityBase.Features.Shell.Purchases
{
    // SceneBuilder builds the prefab (Resources/ShellPurchases/Paywall): a full-screen panel with the title, two
    // feature lines, Rows package rows, a status line, Continue, Restore, Close and the legal links
    public sealed class PaywallView : MonoBehaviour, IPaywallView
    {
        public const int Rows = 4;

        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] rows;
        [SerializeField] private GameObject[] rowSelected;
        [SerializeField] private LocalizedText[] rowPeriods;
        [SerializeField] private LocalizedText[] rowPrices;
        [SerializeField] private LocalizedText[] rowDetails;
        [SerializeField] private LocalizedText[] rowBadges;
        [SerializeField] private LocalizedText status;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button termsButton;

        public event Action CloseClicked;
        public event Action BuyClicked;
        public event Action RestoreClicked;
        public event Action PrivacyClicked;
        public event Action TermsClicked;
        public event Action<int> PackageClicked;

        public int MaxPackages => rows.Length;

        private void Awake()
        {
            panel.SetActive(false);
            closeButton.onClick.AddListener(OnClose);
            buyButton.onClick.AddListener(OnBuy);
            restoreButton.onClick.AddListener(OnRestore);
            privacyButton.onClick.AddListener(OnPrivacy);
            termsButton.onClick.AddListener(OnTerms);
            for (var i = 0; i < rows.Length; i++)
            {
                var index = i;
                rows[i].onClick.AddListener(() => PackageClicked?.Invoke(index));
            }
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(OnClose);
            buyButton.onClick.RemoveListener(OnBuy);
            restoreButton.onClick.RemoveListener(OnRestore);
            privacyButton.onClick.RemoveListener(OnPrivacy);
            termsButton.onClick.RemoveListener(OnTerms);
            foreach (var row in rows) row.onClick.RemoveAllListeners();
        }

        public void SetOpen(bool open) => panel.SetActive(open);

        public void SetLegal(bool privacy, bool terms)
        {
            privacyButton.gameObject.SetActive(privacy);
            termsButton.gameObject.SetActive(terms);
        }

        public void ShowLoading() => ShowStatus("common.loading");

        public void ShowPackages(IReadOnlyList<PaywallRow> list, int selected)
        {
            status.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(true);
            for (var i = 0; i < rows.Length; i++)
            {
                var visible = i < list.Count;
                rows[i].gameObject.SetActive(visible);
                if (!visible) continue;
                var row = list[i];
                rowPeriods[i].SetKey(row.PeriodKey);
                rowPrices[i].SetLiteral(row.Price);
                rowDetails[i].gameObject.SetActive(row.DetailKey != null);
                if (row.DetailKey != null) rowDetails[i].SetKey(row.DetailKey, row.DetailArg);
                rowBadges[i].gameObject.SetActive(row.SavingsPercent.HasValue);
                if (row.SavingsPercent.HasValue) rowBadges[i].SetKey(PurchasesKeys.Savings, row.SavingsPercent.Value);
            }
            Select(selected);
        }

        // The chosen package's row takes the track colour and shows its side bar; the others go back to the background
        public void Select(int index)
        {
            for (var i = 0; i < rowSelected.Length; i++)
            {
                rowSelected[i].SetActive(i == index);
                rows[i].GetComponent<ThemedGraphic>().SetRole(i == index ? ThemeRole.Track : ThemeRole.Background);
            }
        }

        public void ShowUnavailable() => ShowStatus(PurchasesKeys.Unavailable);

        public void ShowMessage(string key)
        {
            status.gameObject.SetActive(true);
            status.SetKey(key);
        }

        public void SetBusy(bool busy)
        {
            buyButton.interactable = !busy;
            restoreButton.interactable = !busy;
            closeButton.interactable = !busy;
        }

        private void ShowStatus(string key)
        {
            foreach (var row in rows) row.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(false);
            ShowMessage(key);
        }

        private void OnClose() => CloseClicked?.Invoke();

        private void OnBuy() => BuyClicked?.Invoke();

        private void OnRestore() => RestoreClicked?.Invoke();

        private void OnPrivacy() => PrivacyClicked?.Invoke();

        private void OnTerms() => TermsClicked?.Invoke();
    }
}
