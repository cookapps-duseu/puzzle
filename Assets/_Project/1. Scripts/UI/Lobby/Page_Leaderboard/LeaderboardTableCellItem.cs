using Cysharp.Text;
using RabbitDog;
using TMPro;
using UnityEngine;

namespace Template
{
    public class LeaderboardTableCellItem : MonoBehaviour
    {
        private GameObject cachedGo;

        public GameObject CachedGo
        {
            get
            {
                cachedGo ??= gameObject;
                return cachedGo;
            }
        }

        private Transform cachedTr;
        public Transform CachedTr
        {
            get
            {
                cachedTr ??= transform;
                return cachedTr;
            }
        }

        [SerializeField] private SimpleSwapper[] myRankingSwappers;
        [SerializeField] private GameObject medal;
        [SerializeField] private SimpleSwapper[] medalSwappers;
        [SerializeField] private TMP_Text rankingText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;
        
        public void SetIndex(int index)
        {
            // var spec = SpecDataManager.Instance.SpecFakeRank.All[index];
            // SetData(false, spec.rank, spec.user_id, spec.score);
        }
        
        public void SetData(bool isMe, int ranking, string name, int score)
        {
            myRankingSwappers.Swap(isMe ? SimpleSwapType.Normal : SimpleSwapType.Disabled);
            
            if (ranking <= 3)
            {
                medal.SetActive(true);
                medalSwappers.Swap(SimpleSwapType.Custom_0 + ranking);
            }
            else
            {
                medal.SetActive(false);
                medalSwappers.Swap(SimpleSwapType.Normal);
            }
            
            if (ranking > 250)
            {
                rankingText.SetText("250+");
            }
            else
            {
                rankingText.SetText(ranking);
            }
            nameText.text = name;
            scoreText.SetText(score);
        }
    }
}
