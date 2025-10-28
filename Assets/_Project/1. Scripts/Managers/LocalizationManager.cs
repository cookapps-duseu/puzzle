using Cysharp.Text;
using CookApps;
using CookApps.Utility;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Template
{
    public class LocalizationManager : SingletonMonoBehaviour<LocalizationManager>
    {
        private StringTable _selectedStringTable;
        private Locale _selectedLocale;
        private string _tableCollectionName;
        private SystemLanguage _currentLanguage = SystemLanguage.Unknown;

        public SystemLanguage CurrentLanguage
        {
            get
            {
                CheckLanguage();
                return _currentLanguage;
            }
        }
        
        private void CheckLanguage()
        {
            if (_currentLanguage != SystemLanguage.Unknown)
                return;
            _currentLanguage = (SystemLanguage)PlayerPrefs.GetInt("CurrentLanguage", (int)Application.systemLanguage);
            (_selectedLocale, _currentLanguage) = GetFallbackLanguage(_currentLanguage);
        }

        private (Locale locale, SystemLanguage language) GetFallbackLanguage(SystemLanguage language)
        {
            var newLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(language));
            if (newLocale == null)
            {
                Debug.LogWarning(ZString.Format("{0} 언어가 세팅되어 있지 않아서 English 로 fallback 합니다.", language));
                newLocale = LocalizationSettings.AvailableLocales.GetLocale(
                    new LocaleIdentifier(SystemLanguage.English));
                return (newLocale, SystemLanguage.English);
            }

            return (newLocale, language);
        }

        public async Awaitable Initialize(string tableCollectionName)
        {
            CheckLanguage();
            _tableCollectionName = tableCollectionName;
            var h = LocalizationSettings.StringDatabase.PreloadTables(_tableCollectionName, _selectedLocale);
            await h.WaitUntilDone();
            if (h.Result is LocalizedStringDatabase lsd)
                _selectedStringTable = lsd.GetTable(_tableCollectionName, _selectedLocale);
            
            // CookApps.BadWordFilter.CookAppsBadWordFilter.SetLanguage(_currentLanguage);
        }

        public async void ChangeLanguage(SystemLanguage language)
        {
            var localeList = LocalizationSettings.AvailableLocales.Locales;
            var (newLocale, newLanguage)= GetFallbackLanguage(language);
            if (_currentLanguage != newLanguage)
            {
                PlayerPrefs.GetInt("CurrentLanguage", (int)_currentLanguage);
                PlayerPrefs.Save();
            }

            if (_selectedLocale && _selectedLocale == newLocale)
                return;
            
            if (_selectedLocale)
                LocalizationSettings.StringDatabase.ReleaseTable(_tableCollectionName, _selectedLocale);
            _selectedLocale = newLocale;
            LocalizationSettings.SelectedLocale = _selectedLocale;
            var h = LocalizationSettings.StringDatabase.PreloadTables(_tableCollectionName, _selectedLocale);
            await h.WaitUntilDone();
            if (h.Result is LocalizedStringDatabase lsd)
                _selectedStringTable = lsd.GetTable(_tableCollectionName, _selectedLocale);
        }

        public string GetString(string key, params object[] args)
        {
            var entry = _selectedStringTable.GetEntry(key);
            if (entry != null)
                return entry.GetLocalizedString(args);
            
            Debug.LogWarning(ZString.Format("{0} 는 string table 에 없습니다.", key));
            return ZString.Format("#{0}", key);
        }

        public void ChangeLocalizeStringEvent(LocalizeStringEvent lse, string key)
        {
            if (!lse)
                return;
            ChangeLocalizeStringReference(lse.StringReference, key);
        }
        
        public void ChangeLocalizeStringReference(LocalizedString ls, string key)
        {
            ls?.SetReference(_tableCollectionName, key);
        }
    }
}
