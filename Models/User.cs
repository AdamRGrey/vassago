namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public List<Account> Accounts { get; set; }

    //if I ever get lots and lots of tags, or some automatic way to register a feature's arbitrary tags, then I can move this off.
    public bool Tag_CanTwitchSummon { get; set; }

    public string DisplayName
    {
        get
        {
            return Accounts.Select(a => a.DisplayName).Distinct()
                .MaxBy(distinctName =>
                    Accounts.Select(a => a.DisplayName)
                    .Where(selectedName => selectedName == distinctName).Count()
                );
        }
    }
}