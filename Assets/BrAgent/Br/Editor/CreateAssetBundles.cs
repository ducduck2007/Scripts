using UnityEditor;
using UnityEngine;

namespace Br
{
    public class CreateAssetBundles : MonoBehaviour
    {
        private const string MENU_CURENT = T.MENU + "/AssetBundles/";

        [MenuItem(MENU_CURENT + "Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildPipeline.BuildAssetBundles("Assets/AssetBundles/" + target.ToString(), BuildAssetBundleOptions.None, target);
        }

        [MenuItem(MENU_CURENT + "Get All AssetBundle names")]
        static void GetNames()
        {
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (var name in names)
            {
                Debug.Log("AssetBundle: " + name);
                //var assetsFromPath = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                //foreach (string a in assetsFromPath)
                //{
                //    Debug.Log(a);
                //}
            }
        }

        [MenuItem(MENU_CURENT + "Clean Cache")]
        static void CleanCache()
        {
            Caching.ClearCache();
        }

        /// <summary>
        /// Debug thông tin khi AssetBundle thay đổi
        /// </summary>
        public class MyPostprocessor : AssetPostprocessor
        {

            void OnPostprocessAssetbundleNameChanged(string path,
                    string previous, string next)
            {
                Debug.Log("AssetBundles: " + path + " old: " + previous + " new: " + next);
            }
        }

        //public static void BuildPlayer()

        //{
        //    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

        //    // example hard-coded platform manifest path

        //    buildPlayerOptions.assetBundleManifestPath = "AssetBundles/iOS/iOS.manifest";

        //    // build the Player ensuring engine code is included for 

        //    // AssetBundles in the manifest.

        //    BuildPipeline.BuildPlayer(buildPlayerOptions);

        //}
    }
}
