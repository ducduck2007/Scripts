using System.Collections.Generic;

public class AgentUIPool
{
    internal static List<object> GetListObject<T>(List<T> list)
    {
         List<object> listTemp = new List<object>();
         foreach (var item in list)
         {
             listTemp.Add(item);
         }
         return listTemp;
         //list.OfType<object>().ToList();
    }
    internal static List<object> GetListObject<T>(IEnumerable<T> collection)
    {
        List<object> listTemp = new List<object>();
        foreach (var item in collection)
        {
            listTemp.Add(item);
        }
        return listTemp;
        //return collection.OfType<object>().ToList();
    }

}
