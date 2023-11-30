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