using System;
using System.Collections.Generic;
using Deer_o_matic.Services;
using SharpKml.Base;
using SharpKml.Dom;

namespace Deer_o_matic.Models;


public static class DateTimeConversionUtility
{
    /// <summary>
    /// Takes in a timePrimitive object and converts it to a datetime in NZST.
    /// TODO: Test this method rigerously to ensure that it is concerting time & date properly, and check daylight savings time aswell
    /// </summary>
    public static DateTime GetPlacemarkNzstTime(TimePrimitive timePrimitive)
    {
        DateTime utcTime = TimePrimitiveToDateTime(timePrimitive);

        return ConvertUtcToNzst(utcTime);
    }

    public static DateTime TimePrimitiveToDateTime(TimePrimitive timePrimitive)
    {
        Timestamp timeStamp = (SharpKml.Dom.Timestamp) timePrimitive;   
        
        if (timeStamp.When == null)
        {
            throw new NullReferenceException();
        }
        
        return (DateTime) timeStamp.When;
    }

    public static DateTime ConvertUtcToNzst(DateTime utc)
    {
        TimeZoneInfo nzst = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time"); 

        return TimeZoneInfo.ConvertTime(utc, nzst);
    }

    public static DateTime ConvertNzstToUtc(DateTime nzst)
    {
        TimeZoneInfo utc = TimeZoneInfo.Utc; 

        return TimeZoneInfo.ConvertTime(nzst, utc);
    }



}