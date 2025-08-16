namespace vassago;

using System;
using System.Net.Http;
using vassago.Models;
using vassago.ProtocolInterfaces;

public static class Shared
{
    public static Random r = new Random();
    public static string DBConnectionString { get; set; }
    public static HttpClient HttpClient { get; internal set; } = new HttpClient();
    public static List<ProtocolInterface> ProtocolList { get; set; } = new();
    public static WebApplication App { get; set; }
}
