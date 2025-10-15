using System;
using System.Linq;
using UnityEngine;

namespace Template
{
    public static class SpecOptionDict
    {
        // USAGE EXAMPLE
        // private static List<int> inventoryCurrencyItem;
        // public static List<int> GetInventoryCurrencyItem()
        // {
        //     inventoryCurrencyItem ??= GetOptionFromSpec_IntArray("INVENTORY_CURRENCY", new int[] { 1, 2, 12 }).ToList();
        //     return inventoryCurrencyItem;
        // }
        //
        // private static int? openMidasPackage;
        // public static int OpenMidasPackage => openMidasPackage ??= GetOptionFromSpec_Int("OPEN_MIDAS_PACKAGE", 100);
        //
        // private static float?  dropNormalItemDropProb;
        // public static float DropNormalItemDropProb => dropNormalItemDropProb ??= GetOptionFromSpec_Float("DROP_NORMAL_EQUIP_ITEM_PROB", 0.01f);
        //
        // private static string  CsKrLink;
        // public static string csKrLink => CsKrLink ??= GetOptionFromSpec_String("CS_LINK", "https://playgrounds.oqupie.com/portals/2650");
        //
        // private static int[] heroUnlockUnlockGuideLevels;
        // public static int GetHeroUnlockGuideLevel(int slotIdx)
        // {
        //     heroUnlockUnlockGuideLevels ??= GetOptionFromSpec_IntArray("HERO_UNLOCK", new int[] { 10, 20, 30, 40, 50,514,1036 });
        //     return heroUnlockUnlockGuideLevels[slotIdx];
        // }
        //
        // private static float[] postMonitoringIntervals;
        // public static float[] PostMonitoringIntervals => postMonitoringIntervals ??= GetOptionFromSpec_FloatArray("POST_MONITORING_INTERVALS", new float[] { 0.5f, 1, 2, 4, 8, 15, 30, 60 });
        
        private static int? heartRecoverSecond;
        public static int HeartRecoverSecond => heartRecoverSecond ??= GetOptionFromSpec_Int("HEART_RECOVER_SECOND", 30 * 60);

        private static int? maxHeart;
        public static int MaxHeart => maxHeart ??= GetOptionFromSpec_Int("HEART_MAX_COUNT", 5);


        #region GetMethods
        private static string GetOptionFromSpec_String(string optionName, string defaultData)
        {
            var data = SpecDataManager.Instance.GetOption(optionName);
            return data?.Value ?? defaultData;
        }

        private static int GetOptionFromSpec_Int(string optionName, int defaultData)
        {
            var data = SpecDataManager.Instance.GetOption(optionName);
            
            if (int.TryParse(data?.Value, out var result))
                return result;
        
            return defaultData;
        }

        private static int[] GetOptionFromSpec_IntArray(string optionName, int[] defaultData, char separator = '|')
        {
            var data = SpecDataManager.Instance.GetOption(optionName);
            
            if (data == null)
                return defaultData;
            
            try
            {
                var arr = data.Value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToArray();
                return arr;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return defaultData;
        }

        private static float GetOptionFromSpec_Float(string optionName, float defaultData)
        {
            var data = SpecDataManager.Instance.GetOption(optionName);
            
            if (float.TryParse(data?.Value, out var result))
                return result;
        
            return defaultData;
        }
        
        private static float[] GetOptionFromSpec_FloatArray(string optionName, float[] defaultData, char separator = '|')
        {
            var data = SpecDataManager.Instance.GetOption(optionName);
            
            if (data == null)
                return defaultData;
            
            try
            {
                var arr = data.Value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(float.Parse).ToArray();
                return arr;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return defaultData;
        }
        #endregion
    }   
}
