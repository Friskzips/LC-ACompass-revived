using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using GameNetcodeStuff;
using System.Threading;


namespace Friskzips.patch
{
    internal class ACompass : MonoBehaviour
    {
        private static CompassUpdater updater;

        public static AssetBundle MyCustomAssets;
        public static bool loaded = false;

        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]
        public static void AddCompass(HUDManager __instance)
        {


            Transform transform = __instance.HUDContainer.transform;
            Debug.Log((object)("Attaching compass to :" + (object)transform));

            if (loaded == false)
            {
                string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                MyCustomAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "compass"));
                if (MyCustomAssets == null)
                {
                    Plugin.Log.LogError("Failed to load AssetBundle!");
                    return;
                }
                else
                {
                    Plugin.Log.LogInfo("ACompass loaded!");
                    loaded = true;
                }
            }

            var prefab = MyCustomAssets.LoadAsset<GameObject>("assets/compass/mask.prefab");
            GameObject val = GameObject.Instantiate(prefab);

            val = UnityEngine.Object.Instantiate<GameObject>(val, transform);
            updater = val.AddComponent<CompassUpdater>();
            updater.CompassImage = ((Component)val.transform.GetChild(0)).GetComponentInChildren<RawImage>();
            
            val.transform.position = new UnityEngine.Vector3(val.transform.position.x, val.transform.position.y-0.021f, val.transform.position.z);


            Plugin.Log.LogInfo(val.transform.position.y);
            Transform Compass = val.transform.Find("Compass");
            Transform Indicator = Compass.transform.Find("Indicator");
            Indicator.position = new UnityEngine.Vector3(Indicator.position.x, Indicator.position.y-0.0173f, Indicator.position.z);
            Indicator.localScale = new UnityEngine.Vector3(Indicator.localScale.x - 0.5f, Indicator.localScale.y - 0.7f, Indicator.localScale.z);

        }

        public class CompassUpdater : MonoBehaviour

        {

            public RawImage CompassImage;

            public Transform toFollow;

            public void setFollow(Transform user)
            {
                toFollow = user;
            }

            private void LateUpdate()
            {
                UpdateCompassHeading();
            }


            private void UpdateCompassHeading()
            {
                if (!((Object)(object)GameNetworkManager.Instance == (Object)null) && !((Object)(object)GameNetworkManager.Instance.localPlayerController?.turnCompassCamera == (Object)null))
                {
                    toFollow = ((Component)GameNetworkManager.Instance.localPlayerController.turnCompassCamera).transform;
                    if (!((Object)(object)CompassImage == (Object)null) && !((Object)(object)toFollow == (Object)null))
                    {
                        Vector2 right = Vector2.right;
                        Quaternion rotation = toFollow.rotation;
                        Vector2 val = right * ((((Quaternion)(rotation)).eulerAngles.y + 45f - 45f) / 360f);
                        CompassImage.uvRect = new Rect(val, Vector2.one);

                    }
                }
            }
        }
    }
}
