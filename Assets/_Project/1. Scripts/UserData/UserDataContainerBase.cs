using System;
using MemoryPack;
using RabbitDog.CypherPrefs;
using UnityEngine;

namespace Template
{
    public abstract class UserDataContainerBase : Preference, IDisposable
    {
        protected UserDataContainerBase(IPreferenceGetterSetter getterSetter) : base(getterSetter)
        {
        }

        public virtual void InitData() => Load();

        public virtual void Dispose() { }
    }
    
    public abstract class UserDataContainerBase<T> : UserDataContainerBase where T : class, new()
    {
        protected UserDataContainerBase() : base(PreferenceGetterSetter.Default)
        {
        }

        protected T data = new();

        private string encryptedKey = null;

        private string EncryptedKey => encryptedKey ??= Convert.ToBase64String(EncryptData(PreferenceKey));

        protected bool isDirty = false;
        
        public void SetIsDirty(bool dirty)
        {
            isDirty = dirty;
        }

        private void Clear()
        {
            data = new();
            isDirty = true;
        }

        protected virtual bool Deserialize(string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
                return false;
            
            try
            {
                var bytes = EncryptData(serializedData);
                MemoryPackSerializer.Deserialize(bytes, ref data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                data = new ();
                return false;
            }

            if (data == null)
                data = new ();
            return true;
        }

        protected virtual string Serialize()
        {
            var bytes = MemoryPackSerializer.Serialize(data);
            ObfuscateBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public override void Save()
        {
            if (!isDirty)
                return;

            var serialized = Serialize();
            getterSetter.Set(EncryptedKey, serialized);
            isDirty = false;
        }

        public override void Load()
        {
            var serialized = getterSetter.Get(EncryptedKey, string.Empty);
            if (string.IsNullOrEmpty(serialized))
            {
                data = new ();
                return;
            }
            
            Deserialize(serialized);
        }

        public override void Delete()
        {
            data = new ();
            getterSetter.Delete(EncryptedKey);
            isDirty = false;
        }
    }
}