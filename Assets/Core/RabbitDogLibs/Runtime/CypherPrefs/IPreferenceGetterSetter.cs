using UnityEngine;

namespace RabbitDog.CypherPrefs
{
    public interface IPreferenceGetterSetter
    {
        string Get(string key, string defaultValue);
        void Set(string key, string value);
        void Delete(string key);
    }

    public class PreferenceGetterSetter : IPreferenceGetterSetter
    {
        public static PreferenceGetterSetter Default { get; } = new PreferenceGetterSetter();
        
        public string Get(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void Set(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
