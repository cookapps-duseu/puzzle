using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace RabbitDog
{
    public class ImageFromAtlas : Image
    {
        private CancellationTokenSource cts;
        private string spriteName;
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!Application.isPlaying)
                return;
            cts?.Cancel();
            cts = null;
            if (string.IsNullOrEmpty(spriteName))
                return;
            AtlasManager.Instance.UnloadSprite(spriteName);
            this.spriteName = null;
        }

        public async Awaitable SetSprite(string spriteName)
        {
            if (this.spriteName == spriteName)
                return;

            enabled = false;
            if (!string.IsNullOrEmpty(this.spriteName))
            {
                AtlasManager.Instance.UnloadSprite(this.spriteName);
                this.spriteName = null;
            }
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;
            
            var task = AtlasManager.Instance.GetSprite(spriteName);
            var sprite = await task;
            if (token.IsCancellationRequested)
            {
                AtlasManager.Instance.UnloadSprite(spriteName);
                return;
            }

            this.spriteName = spriteName;
            this.sprite = sprite;
            enabled = true;
        }
    }
}
