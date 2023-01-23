using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Extreal.Core.Test
{
    public class DisposableSubClass : DisposableBase
    {
        public MemoryStream Stream { get; }

        public UnityWebRequest Uwr { get; }

        public DisposableSubClass()
        {
            Stream = new MemoryStream(65536);
            Uwr = new UnityWebRequest();
        }

        protected override void FreeResources()
        {
            Debug.Log("Free Resources");
            Stream?.Dispose();
            Uwr?.Dispose();
        }
    }
}
