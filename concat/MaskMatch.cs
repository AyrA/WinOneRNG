using System;
using System.Collections.Generic;
using System.IO;

[Flags]
public enum MatchType : int
{
    File = 1,
    Directory = 2,
    All = File | Directory
}

public static class MaskMatch
{
    private static readonly char[] Masks = new char[] { '*', '?' };

    /// <summary>
    /// Durchsucht Eine Dateistruktur nach der Maske entsprechenden Dateien/Ordner
    /// </summary>
    /// <param name="Mask">Suchmaske Beispiel: C:\te?t\*\q12???8*\*.xls*</param>
    /// <param name="Condition">Suchbedingungen (Dateien, Ordner)</param>
    /// <returns>Gefundene Dateien/Ordner</returns>
    public static string[] Match(string Mask, MatchType Condition)
    {
        List<string> Matches = new List<string>();
        string[] Patterns = getPatterns(Mask);
        for (int i = 0; i < Patterns.Length; i++)
        {
            if (i == 0)
            {
                if ((Condition & MatchType.Directory) > 0 || i < Patterns.Length - 1)
                {
                    Matches.AddRange(getDirectories(Patterns[i]));
                }
                if ((Condition & MatchType.File) > 0)
                {
                    Matches.AddRange(getFiles(Patterns[i]));
                }
            }
            else
            {
                string[] temp = Matches.ToArray();
                Matches.Clear();
                foreach (string d in temp)
                {
                    if ((Condition & MatchType.Directory) > 0 || i < Patterns.Length - 1)
                    {
                        Matches.AddRange(getDirectories(Path.Combine(d, Patterns[i])));
                    }
                    if ((Condition & MatchType.File) > 0)
                    {
                        Matches.AddRange(getFiles(Path.Combine(d, Patterns[i])));
                    }
                }
            }
        }

        return Matches.ToArray();
    }

    /// <summary>
    /// Durchsucht eine Dateistructur nach der Masken entsprechenden Dateien/Ordner
    /// </summary>
    /// <param name="Masks">Masken</param>
    /// <param name="Condition">Suchbedingungen (Dateien, Ordner)</param>
    /// <param name="RemoveDuplicates">Entfernen von Duplikaten</param>
    /// <returns>Gefundene Dateien/Ordner</returns>
    public static string[] Match(string[] Masks, MatchType Condition, bool RemoveDuplicates)
    {
        List<string> L = new List<string>();
        foreach (string Mask in Masks)
        {
            string[] result = Match(Mask, Condition);
            if (RemoveDuplicates)
            {
                foreach (string entry in result)
                {
                    if (!L.Contains(entry))
                    {
                        L.Add(entry);
                    }
                }
            }
            else
            {
                L.AddRange(result);
            }
        }
        return L.ToArray();
    }

    /// <summary>
    /// Teilt eine Suchmaske in Teile auf, die von System.IO Klassen verwendet werden können
    /// </summary>
    /// <param name="Mask">Komplette Suchmaske</param>
    /// <returns>Aufgeteilte Suchmaske</returns>
    private static string[] getPatterns(string Mask)
    {
        List<string> Patterns = new List<string>();
        string[] temp = Mask.Split(Path.DirectorySeparatorChar);
        string current = temp[0] + (temp[0].Contains(":") ? Path.DirectorySeparatorChar.ToString() : string.Empty);
        for (int i = 1; i < temp.Length; i++)
        {
            if (temp[i].IndexOfAny(Masks) >= 0)
            {
                Patterns.Add(Path.Combine(current, temp[i]));
                current = string.Empty;
            }
            else
            {
                current = Path.Combine(current, temp[i]);
            }
        }
        return Patterns.ToArray();
    }

    /// <summary>
    /// Durchsucht Verzeichnisse nach Verzeichnissen mit der entsprechenden Maske
    /// </summary>
    /// <param name="Mask">Suchmaske (Pfad\Maske)</param>
    /// <returns>Gefundene Verzeichnisse</returns>
    private static string[] getDirectories(string Mask)
    {
        string Dir = Mask.Substring(0, Mask.LastIndexOf(Path.DirectorySeparatorChar));
        string Arg = Mask.Substring(Mask.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        try
        {
            return Directory.GetDirectories(Dir, Arg);
        }
        catch
        {
        }
        return new string[0];
    }

    /// <summary>
    /// Durchsucht Verzeichnisse nach Dateien mit der entsprechenden Maske
    /// </summary>
    /// <param name="Mask">Suchmaske (Pfad\Maske)</param>
    /// <returns>Gefundene Dateien</returns>
    private static string[] getFiles(string Mask)
    {
        string Dir = Mask.Substring(0, Mask.LastIndexOf(Path.DirectorySeparatorChar));
        string Arg = Mask.Substring(Mask.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        try
        {
            return Directory.GetFiles(Dir, Arg);
        }
        catch
        {
        }
        return new string[0];
    }
}