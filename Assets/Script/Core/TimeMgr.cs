using UnityEngine;
using System.Collections;
using System;

public class TimeMgr: ZBSingleton<TimeMgr>
{
    private const uint ONE_DAY_SEC = 24 * 3600; //一天的秒数
    private const uint ACROSS_SEC = 0 * 3600;   //跨天时间（3点）

    private long clientRealTick = DateTime.Now.Ticks; //（单位为万分之一毫秒）
    private uint serverMSSinceStartup = 0;
    private uint serverRealTick = 0;
    private uint serverUpTick = 0;

    private uint timeZoneSec = 0;
    private bool timeZonePositive = true;
    private uint timeBindPhone = 0;

    private DateTime dateUTCBase;

    public TimeMgr()
    {
        dateUTCBase = new DateTime(1970, 1, 1, 0, 0, 0);
    }
    

    public void RefreshTime(uint svrTick, uint svrUTCTime)
    {
        serverMSSinceStartup = svrTick;
        clientRealTick = DateTime.Now.Ticks;

        //Debug.LogFormat("start_duration[{0}] sec_tick[{1}]", svrTick, svrUTCTime);
        serverRealTick = svrUTCTime;
    }

    public void SetTimeZone(int serverTimeZone)
    {
        timeZonePositive = (serverTimeZone < 0) ? false : true;
        timeZoneSec = (uint)Mathf.Abs(serverTimeZone) * 3600;
    }

    public void SetCellBindTime(uint bindTime)
    {
        timeBindPhone = bindTime;
    }

    //上次同步时间至当前时间的偏移量(毫秒)
    protected long lastSyncOffset
    {
        get
        {
            return (DateTime.Now.Ticks - clientRealTick) / 10000;
        }
    }

    //以服务器本地时间为基础
    public string NowServerDate
    {
        get
        {
            return GetServerDateFormat(GetZoneSecond(ServerUTCTime), "yyyy-MM-dd HH:mm:ss");
        }
    }

    //服务器启服以来的毫秒数
    public uint MSSinceServerStartup
    {
        get
        {
            return (uint)(serverMSSinceStartup + lastSyncOffset);
        }
    }
    public string ServerUpDate
    {
        get
        {
            return GetServerDateFormat(GetZoneSecond(ServerUpTime), "yyyy-MM-dd HH:mm:ss");
        }
    }

    // UTC秒数
    public uint ServerUTCTime 
    {
        get 
        {
            if (serverRealTick > 0)
            {
                return serverRealTick + ((uint)(lastSyncOffset / 1000));
            }
            else
            {
                return 0;
            }
        }
    }

    public uint ServerUpTime
    {
        get
        {
            return serverUpTick;
        }
        set
        {
            serverUpTick = value;
        }
    }
    
    //指定UTC秒数加上时区的秒数
    public uint GetZoneSecond(uint utcSec)
    {
        if (timeZonePositive)
        {
            return utcSec + timeZoneSec;
        }
        else
        {
            return utcSec - timeZoneSec;
        }
    }

    public uint HowManyDaysInYear(uint zoneTime)
    {
        DateTime nowDate = dateUTCBase.AddSeconds(zoneTime); //时区时间的日期
        if (DateTime.IsLeapYear(nowDate.Year))
        {
            return 366u;  
        }
        return 365u;
    }

    /*======================================================格式互转===========================================================*/
    public uint GetSecondsByString(string timeStr)
    {
        try {
            DateTime startTime = new DateTime(1970, 1, 1);
            TimeSpan d = DateTime.Parse(timeStr) - startTime;
            long seconddiff = d.Ticks / 10000000;
            return (uint)seconddiff;
        }
        catch (Exception) {
            Log.Error("you input a valid DateTime the value is [{0}]",timeStr);
            return 0;
        }
    }

    public string GetServerDateFormat(uint sec, string formatStr)
    {
        DateTime d = new DateTime(1970, 1, 1).AddSeconds(sec);
        return d.ToString(formatStr);
        //DateTime d = new DateTime(clientRealTick + LoginOffsetTick);
        //return d.ToString("yyyy-MM-dd HH:mm:ss.") + d.Millisecond.ToString();
    }

    /*======================================================重置时间计算===========================================================*/
    /*====================跨天====================*/
    //获得指定UTC时间的上一次跨天的时间点
    public uint GetSpecifiedTimeLastAcrossTime(uint timeInSecond)
    {
        uint zoneTime = GetZoneSecond(timeInSecond);  //指定时间加上时区

        uint zeroOffset = zoneTime % ONE_DAY_SEC; //指定时间到零点的差值
        uint zero = zoneTime - zeroOffset; //得到今日凌晨（算时区）

        //如果在0点到跨天时间之间，算上一天
        if (zoneTime - zero < ACROSS_SEC)
        {
            return timeInSecond - zeroOffset + ACROSS_SEC - ONE_DAY_SEC;
        }

        //跨天时间到下一个24点，算今天的三点
        return timeInSecond - zeroOffset + ACROSS_SEC;
    }

    //当前时间上一次的跨天时间点
    public uint GetLastAcrossTime()
    {
        return GetSpecifiedTimeLastAcrossTime(ServerUTCTime);
    }

    public uint GetBindPhoneTime()
    {
        return timeBindPhone;
    }

    /*====================跨周====================*/
    //指定时间上一跨周时间点(targetWeekDay的跨天时间点)
    public uint GetSpecifiedTimeLastWeekAcrossTime(uint timeInSecond, uint targetWeekDay)
    {
        uint tWeekDay = targetWeekDay;
        if (targetWeekDay > 7)
        {
            tWeekDay = 7;
        }
  
        uint dayAcrossUTC = GetSpecifiedTimeLastAcrossTime(timeInSecond); //指定时间的跨天UTC时间点
        uint dayAcrossZoneTime = GetZoneSecond(dayAcrossUTC);


        DateTime date = dateUTCBase.AddSeconds(dayAcrossZoneTime);
        
        uint weekDay = (uint)date.DayOfWeek;

        //如果目标跨天点大于当前跨天点，则减去一个周期，算在上一周跨天
        uint cycleOffset = 0;
        if (tWeekDay > weekDay)
        {
            cycleOffset = ONE_DAY_SEC * 7;     
        }

        return dayAcrossUTC - weekDay * ONE_DAY_SEC + tWeekDay * ONE_DAY_SEC - cycleOffset;   //先统一到上周日的跨天时间点，再加上目标跨天日的时间
        
    }

    //当前时间上一跨周时间点（周一的跨天时间点）
    public uint GetLastWeekAcrossTime()
    {
        return GetSpecifiedTimeLastWeekAcrossTime(ServerUTCTime, 1);
    }

    /*====================跨月====================*/
    public uint GetSpecifiedTimeLastMonthAcrossTime(uint timeInSecond, uint targetMonthDay)
    {
        uint dayAcrossUTC = GetSpecifiedTimeLastAcrossTime(timeInSecond); //指定时间的跨天UTC时间点
        uint dayAcrossZoneTime = GetZoneSecond(dayAcrossUTC);

        DateTime date = dateUTCBase.AddSeconds(dayAcrossZoneTime);

        //如果指定的跨月日大于当前月的总日数，则以当前月的一号的跨天时间点作为跨月时间点
        uint monthDay = (uint)date.Day;
        uint thisMonthTotalDays = (uint)DateTime.DaysInMonth(date.Year, date.Month);
        uint dayOffset = (targetMonthDay > thisMonthTotalDays) ? thisMonthTotalDays : targetMonthDay;

        uint cycleOffset = 0;
        if (dayOffset > monthDay)
        {
            DateTime lastMonthLastDayDate = dateUTCBase.AddSeconds(dayAcrossUTC - monthDay * ONE_DAY_SEC); //上个月的最后一天的Date
            cycleOffset = (uint)DateTime.DaysInMonth(lastMonthLastDayDate.Year, lastMonthLastDayDate.Month);
        }

        return dayAcrossUTC - monthDay * ONE_DAY_SEC + dayOffset * ONE_DAY_SEC - cycleOffset * ONE_DAY_SEC;
    }

    //当前时间上一跨月时间点（本月的第一天的跨天时间点）
    public uint GetLastMonthAcrossTime()
    {
        return GetSpecifiedTimeLastMonthAcrossTime(ServerUTCTime, 1);
    }
    
    /*====================跨年====================*/
    public uint GetSpecifiedTimeLastYearAcrossTime(uint timeInSecond, uint targetYearDay)
    {
        uint dayAcrossUTC = GetSpecifiedTimeLastAcrossTime(timeInSecond); //指定时间的跨天UTC时间点
        uint dayAcrossZoneTime = GetZoneSecond(dayAcrossUTC);

        DateTime date = dateUTCBase.AddSeconds(dayAcrossZoneTime);

        //如果指定的跨年日，大于今年的天数，则指定今年的最后一天为跨年日
        uint totalYearDays = HowManyDaysInYear(dayAcrossZoneTime);
        uint dayOffset = (targetYearDay > totalYearDays) ? totalYearDays : targetYearDay;
        uint dayofYear = (uint)date.DayOfYear;

        uint cycleOffset = 0;
        //如果指定日在今天之后，则需要减去一个周期
        if (dayOffset > dayofYear)
        {
            cycleOffset = ONE_DAY_SEC * 365;
            //如果去年是闰年，则多减去一天
            if (DateTime.IsLeapYear(date.Year - 1))
            {
                cycleOffset = cycleOffset + ONE_DAY_SEC;
            }                   
        }

        return dayAcrossUTC - dayofYear * ONE_DAY_SEC + targetYearDay * ONE_DAY_SEC - cycleOffset;
    }

    //当前时间上一跨年时间点（本年的跨天时间点）
    public uint GetLastYearAcrossTime()
    {
        return GetSpecifiedTimeLastYearAcrossTime(ServerUTCTime, 1);
    } 
}

