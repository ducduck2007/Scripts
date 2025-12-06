using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = System.Object;
using Random = System.Random;

public class AgentCSharp
{
    private const string CHUOI_CHECK_VALIDATE = "1234567890_QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";//Các kí tự đăng nhập
    /// <summary>
    /// Trả về FALSE nếu không hợp lệ
    /// </summary>
    internal static bool CheckUsernameValidate(string username)
    {
        foreach (char kiTu in username)
        {
            bool check = false;
            foreach (char kitu2 in CHUOI_CHECK_VALIDATE)
            {
                if (kiTu == kitu2)
                {
                    check = true;
                    break;
                }
            }
            if (!check)
            { return false; }
        }
        return true;
    }
    private const string CHUOI_CHECK_PASSWORD = "1234567890";
    /// <summary>
    /// Check pasword có cả chữ và số
    /// </summary>
    internal static bool CheckPasswordValidate(string password)
    {
        bool checkNumber = false;
        bool checkText = false;
        foreach (char kiTu in password)
        {
            foreach (char kitu2 in CHUOI_CHECK_PASSWORD)
            {
                if (!checkNumber)
                {
                    if (kiTu == kitu2)
                    {
                        checkNumber = true;
                        break;
                    }
                }
            }
        }
        foreach (char kiTu in password)
        {
            if (!char.IsDigit(kiTu))
            {
                checkText = true; break;
            }
        }
        if (checkNumber && checkText)
        {
            return true;
        }
        return false;
    }
    internal static bool ValidEmail(string inputEmail)
    {
        string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
              @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
              @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        Regex re = new Regex(strRegex);
        return re.IsMatch(inputEmail);
    }
//    internal static string ShowMoney(int money)
//    {
//        return ConverMoney(long.Parse(money.ToString()));
//    }
//
//    internal static string ShowMoney(string money)
//    {
//        return ConverMoney(long.Parse(money));
//    }
//    
//    internal static string ShowMoney(float money)
//    {
//        return ConverMoney(Convert.ToInt32(money));
//    }
//    
//    internal static string ShowMoney(long money)
//    {
//        return ConverMoney(money);
//    }
    
    internal static string ShowMoneyFull(int money)
    {
        return ConverMoneyFull(long.Parse(money.ToString()));
    }

    internal static string ShowMoneyFull(long money)
    {
        return ConverMoneyFull(money);
    }
    
    static string ConverMoneyFull(long money)
    {
        string ss = "";
        
        if (money >= -100L && money <= 100L) return money.ToString();
        if (money < -100 && money > -1000000)
        {
            ss = money.ToString("0,0");
        }
        else if (money > 100 && money < 1000000)
        {
            ss = money.ToString("0,0");
        }
        else if (money >= 1000000 && money <= 999999999)
        {
            ss = money.ToString("0,0,0");
        }
        else if (money >= 1000000000)
        {
            ss = (money / 1000).ToString("0,0,0") + "K";
//            ss = money.ToString("0,0,0,0");
        }
        return ss != "00" ? ss : "0";
    }

//    static string ConverMoneyFull(long money)
//    {
//        string ss = "";
//        
//        if (money >= -100L && money <= 100L) return money.ToString();
//        else if (money > 100 && money < 1000000)
//        {
//            ss = money.ToString("0,0");
//        }
//        else if (money >= 1000000 && money <= 100000000)
//        {
//            ss = money.ToString("0,0,0");
//        }
//        else if (money > 100000000)
//        {
//            ss = money.ToString("0,0,0,0");
//        }
//        return ss != "00" ? ss : "0";
//    }

    static string ConverMoney(long money)
    {
        string ss = "";
        if (money >= -100L && money <= 100L) return money.ToString();
        else if (money > 100 && money < 1000000)
        {
            ss = money.ToString("0,0");
        }
        else if (money >= 1000000 && money <= 100000000)
        {
            ss = (money / 1000).ToString("0,0") + "K";
        }
        else if (money > 100000000)
        {
            ss = (money / 1000000).ToString("0,0") + "M";
        }
        return ss != "00" ? ss : "0";
    }
    /// <summary>
    /// Hiện tiền có dấu chấm phân cách
    /// </summary>
    /// <returns></returns>
    internal static string ShowMoneyDotDetail(double money)
    {
        return ConverMoneyDotDetail(money);
    }
    private static string ConverMoneyDotDetail(double money)
    {
        string ss = "";
        if (money < 1000L) 
            ss =  string.Format("{0:0.00}", money);
        else if (money >= 1000 && money < 1000000)
        {
            ss = string.Format("{0:0,0.00}", money);
        }
        else if (money >= 1000000)
        {
            ss = string.Format("{0:0,0,0.00}", money);
        }
        return ss != "00" ? ss : "0";
    }

    /// <summary>
    /// Get time miliseconds
    /// </summary>
    /// <returns></returns>
    internal static long GetCurrentMilli()
    {
        TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        long millis = (long)ts.TotalMilliseconds;
        return millis;
    }
    internal static string GetTimestamp(long milliseconds)
    {
        return (new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(milliseconds + 25200000L/*(7 * 60 * 60 * 1000)=25200000L*/)).ToString("yyyy-MM-dd HH:mm:ss");
    }
    internal static string GetTimestampCurrent
    {
        get { return GetTimestamp(GetCurrentMilli()); }
    }
    internal static string ShowTextTimeFromMiliseconds(long miliSeconds)
    {
        return ShowTextTimeFromSeconds(miliSeconds / 1000L);
    }
    internal static string ShowTextTimeFromHours(float hour)
    {
        return ShowTextTimeFromSeconds((long)(hour * 60F * 60F));
    }
    internal static string ShowTextTimeFromSeconds(long seconds)
    {
        long giay = seconds;
        long phut;
        long gio;
        long ngay;
        long onePhutToGiay = 60;
        long oneGioToGiay = 3600;
        long oneNgayToGiay = 86400;
        if (giay < onePhutToGiay)
        {
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return ("00:" + _giay);
        }
        else if (giay >= onePhutToGiay && giay < oneGioToGiay)
        {
            phut = (giay - giay % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay %= onePhutToGiay;
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return (_minus + ":" + _giay);
        }
        else if (giay >= oneGioToGiay && giay < oneNgayToGiay)
        {
            gio = (giay - giay % oneGioToGiay) / oneGioToGiay;
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay;
            return (gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay));
        }
        else
        {
            ngay = (giay - giay % oneNgayToGiay) / oneNgayToGiay;
//            Debug.LogError("ngày "+ ngay);
            gio = ((giay % oneNgayToGiay) - (giay % oneNgayToGiay) % oneGioToGiay) / oneGioToGiay; 
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay - ngay * oneNgayToGiay;
//            Debug.LogError("=== "+(ngay + "ngày:" + gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay)));
            if (giay == 0 && phut == 0 && gio == 0)
            {
                return ngay + " ngày";
            }
            return (ngay + " ngày  " + gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay));
        }
    }
    internal static string ShowTextTimeFromSeconds2(long seconds)
    {
        long giay = seconds;
        long phut;
        long gio;
        long ngay;
        long onePhutToGiay = 60;
        long oneGioToGiay = 3600;
        long oneNgayToGiay = 86400;
        if (giay < onePhutToGiay)
        {
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return ("00p " + _giay+"s");
        }
        else if (giay >= onePhutToGiay && giay < oneGioToGiay)
        {
            phut = (giay - giay % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay %= onePhutToGiay;
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return (_minus + "p " + _giay+ "s");
        }
        else if (giay >= oneGioToGiay && giay < oneNgayToGiay)
        {
            gio = (giay - giay % oneGioToGiay) / oneGioToGiay;
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay;
            return (gio + "h " + _minus + "p " + (giay > 9 ? "" + giay+"s" : "0" + giay+"s"));
        }
        else
        {
            ngay = (giay - giay % oneNgayToGiay) / oneNgayToGiay;
//            Debug.LogError("ngày "+ ngay);
            gio = ((giay % oneNgayToGiay) - (giay % oneNgayToGiay) % oneGioToGiay) / oneGioToGiay; 
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay - ngay * oneNgayToGiay;
//            Debug.LogError("=== "+(ngay + "ngày:" + gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay)));
            return (ngay + "ngày :" + gio + "h " + _minus + "p " + (giay > 9 ? "" + giay+"s" : "0" + giay+"s"));
        }
    }
    
    internal static long GetSeconds(long seconds)
    {
        long giay = seconds;
        long phut;
        long gio;
        long ngay;
        long onePhutToGiay = 60;
        long oneGioToGiay = 3600;
        long oneNgayToGiay = 86400;
        if (giay >= onePhutToGiay && giay < oneGioToGiay)
        {
            phut = (giay - giay % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay %= onePhutToGiay;
            return giay;
        }
        else if (giay >= oneGioToGiay && giay < oneNgayToGiay)
        {
            gio = (giay - giay % oneGioToGiay) / oneGioToGiay;
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay;
            return giay;
        }
        else
        {
            ngay = (giay - giay % oneNgayToGiay) / oneNgayToGiay;
            gio = ((giay % oneNgayToGiay) - (giay % oneNgayToGiay) % oneGioToGiay) / oneGioToGiay; 
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay - ngay * oneNgayToGiay;
            return giay;
        }
    }
    
    
    internal static string ShowTextTimeFromSecondsDay(long seconds)
    {
        long giay = seconds;
        long phut;
        long gio;
        long ngay;
        long onePhutToGiay = 60;
        long oneGioToGiay = 3600;
        long oneNgayToGiay = 86400;
        if (giay < onePhutToGiay)
        {
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return ("00:" + _giay);
        }
        else if (giay >= onePhutToGiay && giay < oneGioToGiay)
        {
            phut = (giay - giay % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay %= onePhutToGiay;
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return (_minus + ":" + _giay);
        }
        else if (giay >= oneGioToGiay && giay < oneNgayToGiay)
        {
            gio = (giay - giay % oneGioToGiay) / oneGioToGiay;
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay;
            return (gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay));
        }
        else
        {
            ngay = (giay - giay % oneNgayToGiay) / oneNgayToGiay;
//            Debug.LogError("ngày "+ ngay);
            gio = ((giay % oneNgayToGiay) - (giay % oneNgayToGiay) % oneGioToGiay) / oneGioToGiay; 
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay - ngay * oneNgayToGiay;
            if (ngay > 0)
            {
                return ngay + " ngày";
            }
            else return (gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay));
        }
    }
    
    internal static string ShowTextTimeFromSecondsByOur(long seconds)
    {
        long giay = seconds;
        long phut;
        long gio;
        long ngay;
        long onePhutToGiay = 60;
        long oneGioToGiay = 3600;
        long oneNgayToGiay = 86400;
        if (giay < onePhutToGiay)
        {
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return ("00:" + _giay);
        }
        else if (giay >= onePhutToGiay && giay < oneGioToGiay)
        {
            phut = (giay - giay % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay %= onePhutToGiay;
            string _giay = giay > 9 ? "" + giay : "0" + giay;
            return (_minus + ":" + _giay);
        }
        else 
        {
            gio = (giay - giay % oneGioToGiay) / oneGioToGiay;
            phut = ((giay % oneGioToGiay) - (giay % oneGioToGiay) % onePhutToGiay) / onePhutToGiay;
            string _minus = phut > 9 ? "" + phut : "0" + phut;
            giay = giay - phut * onePhutToGiay - gio * oneGioToGiay;
            return (gio + ":" + _minus + ":" + (giay > 9 ? "" + giay : "0" + giay));
        }
      
    }
    /// <summary>
    /// example: codeColor=#40F828FF
    /// </summary>
    /// <returns></returns>
    internal static string GetTextColor(string codeColor, object data)
    {
        return "<color=" + codeColor + ">" + data + "</color>";
    }
    
    /// <summary>
    /// example: codeColor=#40F828FF
    /// </summary>
    /// <returns></returns>
    internal static string GetTextColorBold(string codeColor, object data)
    {
        return "<b><color=" + codeColor + ">" + data + "</color></b>";
    }
    internal static string GetDayOfTimestamp(long timestamp)
    {
        return GetTimestamp(timestamp).Split(' ')[0];
    }
    internal static string GetTimeOfTimestamp(long timestamp)
    {
        return GetTimestamp(timestamp).Split(' ')[1];
    }

    
    internal static string GetDateFromMillisecond(long time){
       DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(time).ToLocalTime();
       return dtDateTime.ToString("HH:mm tt dd/MM/yyyy");
    }

    // Lấy giá trị cao nhất trong các giá trị
    internal static int GetMaxIntValue(List<int> listValue, int defaultValue)
    {
        int max = 0;
        for (int i = 0; i < listValue.Count; i++)
        {
            if (listValue[i] > max)
                max = listValue[i];
        }
        if (max != 0)
            return max;
        return defaultValue;
    }
    private static Random _rd = new Random();
    internal static int GetRandom(int max)
    {
        return _rd.Next(max);
    }
    internal static List<T> GetList<T>(IEnumerable<T> collection)
    {
        return new List<T>(collection);
    }
    public static int Round(float value)
    {
        return Convert.ToInt32(Math.Round(value));
    }
    public static int RoundUp(float value)
    {
        string[] raws = value.ToString().Split('.');
        if (raws.Length == 2)
        {
            if (int.Parse(raws[1].Trim()) > 0)
                return int.Parse(raws[0]) + 1;
            else
                return int.Parse(raws[0]);
        }
        return (int)value;
    }
    public static int RoundDown(float value)
    {
        string[] raws = value.ToString().Split('.');
        if (raws.Length == 2)
        {
            return int.Parse(raws[0]);
        }
        return (int)value;
    }

    public static string GetHiddenPercent(float value)
    {
        return Math.Round(value,4) * 100 + "%";
    }

    public static double GetPercent(float value)
    {
        return Math.Round(value,4) * 100;
    }
    
    /// <summary>
    /// Cắt các ký tự cuối của chuỗi 
    /// </summary>
    /// <param name="str"></param>
    /// <param name="lastSymbol"></param>
    /// <returns></returns>
    public static string CutTheLastSymbol(string str , int lastSymbol)
    {
       return str.Substring(0, str.Length - lastSymbol);
    }

    public static T[] RevertArray<T>(T[] array)
    {
        T[] tempArr = array;
        int n = tempArr.Length;
        for (int i = 0; i < n / 2; i++)
        {
            T temp = tempArr[i];
            tempArr[i] = tempArr[n - 1 - i];
            tempArr[n - 1 - i] = temp;
        }
        return tempArr;
    }

    /// <summary>
    /// LOG tất cả giá trị của 1 class
    /// </summary>
    /// <param name="obj"></param>
    public static void DebugFullClass(object obj)
    {
        // string log = ObjectDumper.Dump(obj);
        // Debug.LogError("CLASS : "+obj.GetType()+".cs" + "\n" +log);
    }


    public static long GetTimeConLaiToDay()
    {
        string _date;
        _date = System.DateTime.Now.ToString("HH:mm:ss: dd MMMM, yyyy");
        string[] arrTachChuoi = _date.Split(':');
        int gio = int.Parse(arrTachChuoi[0]);
        int phut = int.Parse(arrTachChuoi[1]);
        int giay = int.Parse(arrTachChuoi[2]);
        long timeConLaiToDay = 86400 - ((gio * 60 * 60) + (phut * 60) + giay);
        return timeConLaiToDay;
    }
    
    internal static string GetTextTimeInSecond(int second)
    {
        if (second > 0)
        {
            if (second < 3600)
            {
                int phut = second / 60;
                int giay = second - (phut * 60);
                string textPhut = phut + "p";
                string textGiay = giay + "s";

                if (phut < 10) textPhut = "0" + phut + "p";
                if (giay < 10) textGiay = "0" + giay + "s";
                return textPhut + textGiay;
            }
            else if (second < 86400)
            {
                int gio = second / 60 / 60;
                int phut = (second - (gio * 60 * 60))/60;
                string textGio = gio + "h";
                string textPhut = phut + "p";

                if (gio < 10) textGio = "0" + gio + "h";
                if (phut < 10) textPhut = "0" + phut + "p";
                return textGio + textPhut;
            }
            else
            {
                int ngay = second / 24 / 60 / 60;
                int gio = (second - (ngay * 24 * 60 * 60)) / (60*60);
                int phut = (second - ((ngay * 24 + gio) * 60 * 60)) / 60;
                string textNgay = ngay + "d";
                string textGio = gio + "h";
                string textPhut = phut + "p";

                if (ngay < 10) textNgay = "0" + ngay + "d";
                if (gio < 10) textGio = "0" + gio + "h";
                if (phut < 10) textPhut = "0" + phut + "p";
                return textNgay + textGio + textPhut;
            }
        }

        return "00p00s";
    }


}
