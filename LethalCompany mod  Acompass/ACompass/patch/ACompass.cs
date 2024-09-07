using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;


namespace Friskzips.patch
{
    internal class ACompass : MonoBehaviour
    {
        private static CompassUpdater updater;

        public static AssetBundle CompassAssets;
        public static bool loaded = false;

        public static bool oldTexture=false;

        public static bool firstTimeTexture = false;

        public static int position = 69;

        public static void loadAssets()
        {
            if (loaded == false)
            {
                string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                CompassAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "compass"));
                if (CompassAssets == null)
                {
                    Plugin.Log.LogError("Failed to load AssetBundle!");
                    return;
                }
                else
                {
                    Plugin.Log.LogInfo("AsssetBundle loaded!");
                    loaded = true;
                    return;
                }
            }
        }
        
        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]

        
        public static void AddCompass(HUDManager __instance)
        {


            Transform transform = __instance.HUDContainer.transform;
            Debug.Log((object)("Attaching compass to :" + (object)transform));

            

            var prefab = CompassAssets.LoadAsset<GameObject>("assets/compass/mask.prefab");
            GameObject val = GameObject.Instantiate(prefab);

            val = UnityEngine.Object.Instantiate<GameObject>(val, transform);
            updater = val.AddComponent<CompassUpdater>();
            updater.CompassImage = ((Component)val.transform.GetChild(0)).GetComponentInChildren<RawImage>();
            
            //Adjust the mask position
            //val.transform.position = new UnityEngine.Vector3(val.transform.position.x, val.transform.position.y-0.021f, val.transform.position.z);
            
            //Adjust the indicator position
            Plugin.Log.LogInfo(val.transform.position.y);
            Transform Compass = val.transform.Find("Compass");
            Transform Indicator = Compass.transform.Find("Indicator");
            Indicator.position = new UnityEngine.Vector3(Indicator.position.x, Indicator.position.y-0.017f, Indicator.position.z);
            Indicator.localScale = new UnityEngine.Vector3(Indicator.localScale.x - 0.5f, Indicator.localScale.y - 0.7f, Indicator.localScale.z);

            position = 69;
            firstTimeTexture = false;
        }

        public class CompassUpdater : MonoBehaviour
        {

            public RawImage CompassImage;

            public Transform toFollow;

            RawImage m_RawImage;

            readonly Texture m_TextureOld = CompassAssets.LoadAsset<Texture2D>("compass_shifted");
            readonly Texture m_Texture=CompassAssets.LoadAsset<Texture2D>("compass_shifted2");

            public void setFollow(Transform user)
            {
                toFollow = user;
            }

            private void LateUpdate()
            {
                UpdateCompassHeading();

                
                
                if(oldTexture!=Plugin.oldTexture.Value || firstTimeTexture==false)
                {
                    ACompass.oldTexture = Plugin.oldTexture.Value;
                    firstTimeTexture = true;

                    if(!Plugin.oldTexture.Value)
                    {
                        //Change the Texture to be the one you define in the Inspector
                        CompassImage.texture = m_Texture;
                    }
                    if (Plugin.oldTexture.Value)
                    {
                        //Change the Texture to be the one you define in the Inspector
                        CompassImage.texture = m_TextureOld;

                        

                    }

                }

                
                if(position != (int)Plugin.position.Value)
                {
                    position = (int)Plugin.position.Value;
                    if (position == 0)
                    {
                        transform.localPosition = new UnityEngine.Vector3(0, -224.4849f, 0);
                        //transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y - 0.021f, transform.position.z);
                        
                    }

                    if (position == 1)
                    {
                        transform.localPosition = new UnityEngine.Vector3(0, 218.5156f, 0);
                    }
                    Plugin.Log.LogInfo("pos " + transform.localPosition.x +", "+ transform.localPosition.y+", "+ transform.localPosition.z);
                    Plugin.Log.LogInfo("pos enum " + position);
                    
                }
            }


            private void UpdateCompassHeading()
            {
                if (!((Object)(object)GameNetworkManager.Instance == (Object)null) && !((Object)(object)GameNetworkManager.Instance.localPlayerController?.turnCompassCamera == (Object)null))
                {
                    toFollow = ((Component)GameNetworkManager.Instance.localPlayerController.turnCompassCamera).transform;
                    if (!((Object)(object)CompassImage == (Object)null) && !((Object)(object)toFollow == (Object)null))
                    {                       
                        
                        if(Plugin.alignToShipRadar.Value)
                        {
                            Vector2 right = Vector2.right;
                            Quaternion rotation = toFollow.rotation;
                            Vector2 val = right * ((((Quaternion)(rotation)).eulerAngles.y + 45f - 45f) / 360f);
                            CompassImage.uvRect = new Rect(val, Vector2.one);
                        }                            
                        else
                        { 
                            Vector2 right = Vector2.right;
                            Quaternion rotation = toFollow.rotation;
                            Vector2 val = right * ((((Quaternion)(rotation)).eulerAngles.y - 45f) / 360f);
                            CompassImage.uvRect = new Rect(val, Vector2.one);
                        }
                        
                    }
                }
            }
        }
    }
}
