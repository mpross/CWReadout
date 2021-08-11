using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
    
    /**
     *  DayNr class 
     *  The GetDayNr class takes a DateTime object returns the number of days from the beginning of a year to some data
     */
    public class DayNr
    {
        public DayNr()
        {
        }

        public static double GetDayNr(DateTime Input)
        {
            double a, frac;
            int mon;
            int leapyear;
            mon = Input.Month;
            a = 0;
            leapyear = 0;
            if ((Input.Year % 4 == 0) && (Input.Year % 100 != 0)) leapyear = 1;
            if (mon > 1) a = a + 31;
            if ((mon > 2) && (leapyear == 0)) a = a + 28;
            if ((mon > 2) && (leapyear == 1)) a = a + 29;
            if (mon > 3) a = a + 31;
            if (mon > 4) a = a + 30;
            if (mon > 5) a = a + 31;
            if (mon > 6) a = a + 30;
            if (mon > 7) a = a + 31;
            if (mon > 8) a = a + 31;
            if (mon > 9) a = a + 30;
            if (mon > 10) a = a + 30;
            if (mon > 11) a = a + 31;
            a = a + Input.Day;
            frac = (Input.Hour + Input.Minute / 60.0 + Input.Second / 3600.0 + Input.Millisecond / 1000 / 3600) / 24.0;
            return a + frac;
        }

    }

