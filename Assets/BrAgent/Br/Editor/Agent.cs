using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

namespace Br
{
    public class Agent
    {
        private const string MENU_CURRENT = T.MENU + "/Agent/";
        [MenuItem(MENU_CURRENT + "Find Component<T>")]
        private static void FindAllComponent()
        {
            ClearLogConsole();
            UnityEngine.UI.Outline[] arrObject = GameObject.FindObjectsOfType<UnityEngine.UI.Outline>();
            if (arrObject != null && arrObject.Length > 0)
            {
                List<string> listObject = new List<string>();
                StringBuilder sb = null;
                List<string> tmp = null;
                for (int i = 0; i < arrObject.Length; i++)
                {
                    sb = new StringBuilder();
                    tmp = new List<string>();
                    tmp.Add(arrObject[i].transform.name);
                    Transform tran = arrObject[i].transform;
                    string split = "/";
                    for (int j = 0; j < 11; j++)
                    {
                        if (j == 0)
                            if (tran.parent != null)
                                tmp.Add(tran.parent.name);
                            else break;
                        if (j == 1)
                            if (tran.parent.parent != null)
                                tmp.Add(tran.parent.parent.name);
                            else break;
                        if (j == 2)
                            if (tran.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.name);
                            else break;
                        if (j == 3)
                            if (tran.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.name);
                            else break;
                        if (j == 4)
                            if (tran.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.name);
                            else break;
                        if (j == 5)
                            if (tran.parent.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.name);
                            else break;
                        if (j == 6)
                            if (tran.parent.parent.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.parent.parent.name);
                            else break;
                        if (j == 7)
                            if (tran.parent.parent.parent.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.parent.parent.parent.name);
                            else break;
                        if (j == 8)
                            if (tran.parent.parent.parent.parent.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.parent.parent.parent.parent.name);
                            else break;
                        if (j == 9)
                            if (tran.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.name);
                            else break;
                        if (j == 10)
                            if (tran.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent != null)
                                tmp.Add(tran.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.parent.name);
                            else break;
                    }
                    for (int k = tmp.Count - 1; k > 0; k--)
                    {
                        sb.Append(tmp[k] + split);
                    }

                    listObject.Add(sb.ToString());
                }

                if (listObject.Count > 0)
                {
                    Debug.Log("---Start---");
                    for (int i = 0; i < listObject.Count; i++)
                    {
                        Debug.Log(listObject[i]);
                    }
                    Debug.Log("---End--- Quantity: " + listObject.Count);
                }
            }
            else Debug.Log("Không có đối tượng cần GetComponent<>");
        }
        private static void ClearLogConsole()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        [MenuItem(MENU_CURRENT + "GetDataPath")]
        private static void GetDataPath()
        {
            Debug.Log("Data path: " + Application.dataPath);
            Debug.Log("PersistentDataPath: " + Application.persistentDataPath);
        }
        [MenuItem(MENU_CURRENT + "Delete All Key PlayerPref")]
        private static void DeleteAllKeyPlayerPref()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}