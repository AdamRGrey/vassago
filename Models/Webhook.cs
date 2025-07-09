namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using vassago.Models;

public class Webhook
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public UAC Uac {get;set;}
       public string Trigger { get; set; }
    public Uri Uri { get; set; }
    public Enumerations.HttpVerb Method { get; set; }
    public List<string> Headers { get; set; }
    public string Content { get; set; }
    public string Description { get; set; }
}
