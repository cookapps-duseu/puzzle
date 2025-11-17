using System;
using UnityEngine;

namespace CookApps.Utility
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionField;

        public ShowIfAttribute(string conditionField)
        {
            ConditionField = conditionField;
        }
    }
}
