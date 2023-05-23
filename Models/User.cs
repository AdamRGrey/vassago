using System;

public abstract class User
{
    public Guid Id{get;set;}
    public bool IsBot {get;set;} //webhook counts
}