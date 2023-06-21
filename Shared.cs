namespace vassago;

using System;
using System.Net.Http;
using vassago.Models;


public static class Shared
{
    public static Random r = new Random();
    public static string DBConnectionString { get; set; }
    public static HttpClient HttpClient { get; internal set; } = new HttpClient();
}
