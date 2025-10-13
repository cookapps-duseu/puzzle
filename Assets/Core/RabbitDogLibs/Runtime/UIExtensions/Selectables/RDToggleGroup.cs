using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RabbitDog.UIExtensions
{
    [AddComponentMenu("UI/RabbitDog Toggle Group")]
    [DisallowMultipleComponent]
    public class RDToggleGroup : UIBehaviour
    {
        public bool allowSwitchOff;

        private List<RDToggle> m_Toggles = new ();

        [Serializable]
        public class ToggleGroupEvent : UnityEvent<bool>
        {
        }

        public ToggleGroupEvent onToggleGroupChanged = new ();
        public ToggleGroupEvent onToggleGroupToggleChanged = new ();

        public RDToggle selectedToggle;

        protected RDToggleGroup()
        {
        }

        private void ValidateToggleIsInGroup(RDToggle toggle)
        {
            if (toggle == null || !m_Toggles.Contains(toggle))
            {
                throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] {toggle, this}));
            }
        }

        public void NotifyToggleOn(RDToggle toggle)
        {
            ValidateToggleIsInGroup(toggle);

            // disable all toggles in the group
            for (var i = 0; i < m_Toggles.Count; i++)
            {
                if (m_Toggles[i] == toggle)
                {
                    selectedToggle = toggle;
                    continue;
                }

                m_Toggles[i].isOn = false;
            }

            onToggleGroupChanged.Invoke(AnyTogglesOn());
        }

        public void UnregisterToggle(RDToggle toggle)
        {
            if (m_Toggles.Contains(toggle))
            {
                m_Toggles.Remove(toggle);
                toggle.onValueChanged.RemoveListener(NotifyToggleChanged);
            }
        }

        private void NotifyToggleChanged(bool isOn)
        {
            onToggleGroupToggleChanged.Invoke(isOn);
        }

        public void RegisterToggle(RDToggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
            {
                m_Toggles.Add(toggle);
                toggle.onValueChanged.AddListener(NotifyToggleChanged);
            }
        }

        public bool AnyTogglesOn()
        {
            return m_Toggles.Find(x => x.isOn) != null;
        }

        public IEnumerable<RDToggle> ActiveToggles()
        {
            return m_Toggles.Where(x => x.isOn);
        }

        public void SetAllTogglesOff()
        {
            bool oldAllowSwitchOff = allowSwitchOff;
            allowSwitchOff = true;

            for (var i = 0; i < m_Toggles.Count; i++)
            {
                m_Toggles[i].isOn = false;
            }

            allowSwitchOff = oldAllowSwitchOff;
        }

        public void HasTheGroupToggle(bool value)
        {
            RDDebug.Log("Testing, the group has toggled [" + value + "]");
        }

        public void HasAToggleFlipped(bool value)
        {
            RDDebug.Log("Testing, a toggle has toggled [" + value + "]");
        }
    }
}
