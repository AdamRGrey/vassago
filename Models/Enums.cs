namespace vassago.Models;

public static class Enumerations
{
    public static string LewdnessFilterLevel(int level)
    {
        switch (level)
        {
            case 0:
                return "this is a christian minecraft server 🙏";
            case 1:
                return "G-Rated";
            case 2:
                return "polite company";
            case 3:
                return ";) ;) ;)";
            default:
                return "ERROR";
        }
    }
    public static string MeannessFilterLevel(int level)
    {
        switch (level)
        {
            case 0:
                return "good vibes only";
            case 1:
                return "387.44 million miles of printed circuits, etc";
            default:
                return "ERROR";
        }
    }
}