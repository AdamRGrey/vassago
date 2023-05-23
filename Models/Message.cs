using System;

public abstract class Message
{
    public Guid Id{get;set;}
    public Guid InChannel{get;set;}
    public string Content{get;set;}
    public bool TagsMe{get;set;}
}