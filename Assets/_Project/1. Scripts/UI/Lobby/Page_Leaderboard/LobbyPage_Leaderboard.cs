using RabbitDog.UIExtensions;
using UnityEngine;
using UnityEngine.Pool;

namespace Template
{
    public class LobbyPage_Leaderboard : LobbyPageBase
    {
        ObjectPool<LeaderboardTableCellItem> cellItemPool;
        
        [SerializeField] private LeaderboardTableCellItem cellItemPrefab;
        [SerializeField] private TableView tableView;
        [SerializeField] private GameObject quickTopButton;
        [SerializeField] private GameObject quickBottomButton;
        [SerializeField] private LeaderboardTableCellItem myLeaderboardCellItemPrefab;

        protected void Awake()
        {
            cellItemPool = new ObjectPool<LeaderboardTableCellItem>(
                () =>
                {
                    var go = Instantiate(cellItemPrefab.gameObject, tableView.content);
                    var item = go.GetComponent<LeaderboardTableCellItem>();
                    return item;
                },
                obj => obj.CachedGo.SetActive(true),
                obj => obj.CachedGo.SetActive(false),
                obj => Destroy(obj.CachedGo),
                false
            );
            
            tableView.OnScrolling += pos =>
            {
                quickTopButton.SetActive(pos.y > 0);
                quickBottomButton.SetActive(pos.y < tableView.content.sizeDelta.y - tableView.viewport.sizeDelta.y);
            };
            tableView.OnGetCellItem += index =>
            {
                var cellItem = cellItemPool.Get();
                cellItem.SetIndex(index);
                return cellItem.CachedGo;
            };
            tableView.OnGetCellItemSize += index => cellItemPrefab.CachedGo.GetComponent<RectTransform>().sizeDelta;
            tableView.OnReleaseCellItem += (index, cellItem) =>
            {
                var item = cellItem.GetComponent<LeaderboardTableCellItem>();
                if (item == null)
                    Destroy(cellItem);
                cellItemPool.Release(item);
            };
            // tableView.OnGetTotalCellItemCount += () => SpecDataManager.Instance.SpecFakeRank.All.Count;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            cellItemPool.Dispose();
            cellItemPool = null;
        }

        protected override void OnEnter()
        {
            tableView.RefreshAll();
            var basicInfo = UserDataManager.Instance.GetBasicData();
            // myLeaderboardCellItemPrefab.SetData(true, 1000, basicInfo.GetNickName(), stageInfo.ClearedStage);
        }


        protected override void OnExit()
        {
            tableView.ClearAllCells();
        }
        
        public void OnClickQuickTopButton()
        {
            tableView.FocusItem(0);
        }
        
        public void OnClickQuickBottomButton()
        {
            // tableView.FocusItem(SpecDataManager.Instance.SpecFakeRank.All.Count - 1);
        }
    }
}