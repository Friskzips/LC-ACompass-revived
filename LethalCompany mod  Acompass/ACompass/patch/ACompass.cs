using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UIElements.StylePropertyAnimationSystem;


namespace Friskzips.patch
{
    internal class ACompass : MonoBehaviour
    {
        private static CompassUpdater updater;

        public static AssetBundle CompassAssets;
        public static bool loaded = false;

        public static bool oldTexture=false;

        public static bool firstTimeTexture = false;

        public static bool spectating = false;
        public static bool spectatingFirstTime = false;

        public static bool inside = false;

        public static int position = 69;

        public static int default_x_value = 0;

        public static GameNetworkManager GameNetworkInstance;

        public static int x_value = 0;

        public static RectTransform rt;

        public static GameObject compassObject;

        public static bool inTerminal = false;
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

        [HarmonyPatch(typeof(MenuManager), "OnEnable")]
        [HarmonyPostfix]
        public static void resetStuff()
        {
            spectatingFirstTime = false;
        }

        [HarmonyPatch(typeof(GameNetworkManager), "ConnectionApproval")]
        [HarmonyPostfix]
        public static void getNetworkInstance(GameNetworkManager __instance)
        {
            GameNetworkInstance = __instance;
        }


        

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void getPlayerData(GameNetcodeStuff.PlayerControllerB __instance)
        {
            inTerminal = __instance.inTerminalMenu;
            //spectating = __instance.isPlayerDead;
            inside = __instance.isInsideFactory;
            //Plugin.Log.LogWarning("InTerminal: " + inTerminal + "\nSpectacting: " + spectating + "\n Inside: " + inside);
            //Plugin.Log.LogDebug("Inside: " + inside);
            //Plugin.Log.LogWarning("Spectacting: " + spectating);

            if( __instance.IsClient)
            {
                spectating = __instance.isPlayerControlled;
                if (!spectatingFirstTime)
                {
                    Plugin.Log.LogInfo("got the spectating variable");
                    spectatingFirstTime = true;
                }

                
            }



            if (GameNetworkInstance != null)
            {
                if(GameNetworkInstance.localPlayerController != null)
                {
                    if(!spectatingFirstTime)
                    {
                        Plugin.Log.LogInfo("Found GameNetworkInstance.localPlayerController!");
                        spectatingFirstTime = true;
                    }
                    
                    spectating = GameNetworkInstance.localPlayerController.isPlayerDead;
                }
                else
                {
                    Plugin.Log.LogWarning("Couldn't get GameNetworkInstance.localPlayerController, the mod can't detect when you are in spectator");
                }
                    
            }
            else if(!__instance.IsClient)
            {
                Plugin.Log.LogWarning("Couldn't get GameNetworkInstance, the mod can't detect when you are in spectator");
            }


        }



        [HarmonyPatch(typeof(StartOfRound), "Update")]
        [HarmonyPostfix]
        public static void hideCompass(StartOfRound __instance)
        {
            //Plugin.Log.LogWarning(__instance.inShipPhase);

            //debug
            /*
            if (!__instance.inShipPhase)
            {
                Plugin.Log.LogWarning("spectating: " + spectating);
            }
            */

            if (compassObject != null)
            {
                if ((__instance.inShipPhase && Plugin.hideWhenInOrbit.Value) || (inTerminal) || (spectating == true) || (inside && Plugin.hideWhenInside.Value))
                {
                    compassObject.SetActive(false);
                }

                else
                {
                    compassObject.SetActive(true);
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]
        
        public static void AddCompass(HUDManager __instance)
        {
            //HIDE ZEEKER COMPASS                                      
            
                Transform compassObj = __instance.HUDContainer.transform.Find("CompassImage (1)");
                if( compassObj != null )
                {
                    compassObj.gameObject.SetActive(false);
                }                           
                else
                {
                    Plugin.Log.LogWarning("Couldn't find in \"" + __instance.HUDContainer + "\" the CompassImage");
                }



                //ADD MY COMPASS
                Transform transform = __instance.HUDContainer.transform;
            Debug.Log((object)("Attaching compass to :" + (object)transform));

            

            var prefab = CompassAssets.LoadAsset<GameObject>("assets/compass/mask.prefab");
            GameObject val = GameObject.Instantiate(prefab);

            val = UnityEngine.Object.Instantiate<GameObject>(val, transform);
            compassObject = val;
            updater = val.AddComponent<CompassUpdater>();
            updater.CompassImage = ((Component)val.transform.GetChild(0)).GetComponentInChildren<RawImage>();
            
            //Adjust the mask position
            //val.transform.position = new UnityEngine.Vector3(val.transform.position.x, val.transform.position.y-0.021f, val.transform.position.z);
            
            //Adjust the indicator position
            Plugin.Log.LogDebug(val.transform.position.y);
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

                //OFFSET VALUE
                rt = GetComponent<RectTransform>();               
                if (default_x_value == 0)
                {
                    default_x_value = (int)transform.localPosition.x;
                }
                float clampedX = Mathf.Clamp(Plugin.x_offset.Value, -100f, 100f); 
                Vector2 pos = rt.anchoredPosition;
                pos.x = clampedX;

                // Ottieni la larghezza effettiva del parent (Canvas)
                float canvasWidth = ((RectTransform)rt.parent).rect.width;

                // Calcola posizione in pixel: da -canvasWidth/2 a +canvasWidth/2
                float pixelX = (clampedX / 100f) * (canvasWidth / 2f);

                // Imposta la posizione rispetto al centro
                Vector2 pos2 = rt.anchoredPosition;
                pos2.x = pixelX;
                rt.anchoredPosition = pos2;
            

                if (position != (int)Plugin.position.Value)
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
                    Plugin.Log.LogDebug("pos " + transform.localPosition.x +", "+ transform.localPosition.y+", "+ transform.localPosition.z);
                    Plugin.Log.LogDebug("pos enum " + position);
                    
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
