using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;

namespace AyrA.IO
{
    /// <summary>
    /// Represents a part from an INI file with settings and its name
    /// </summary>
    public class INIPart : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the section
        /// </summary>
        public string Section
        { get; set; }
        /// <summary>
        /// Gets or sets the values from the section
        /// </summary>
        public NameValueCollection Settings
        { get; set; }

        /// <summary>
        /// Creates a new INI part from known values
        /// </summary>
        /// <param name="Name">Section name</param>
        /// <param name="settings">Settings</param>
        public INIPart(string Name, NameValueCollection settings)
        {
            Section = Name;
            Settings = settings;
        }

        /// <summary>
        /// Creates a new INI part with a given name
        /// </summary>
        /// <param name="Name">Section name</param>
        public INIPart(string Name) : this(Name,new NameValueCollection())
        {
        }

        /// <summary>
        /// creates a blank INI part
        /// </summary>
        public INIPart() : this(string.Empty)
        {
        }

        ~INIPart()
        {
            Dispose();
        }

        /// <summary>
        /// Gets rid of the settings
        /// </summary>
        public void Dispose()
        {
            if (Settings != null)
            {
                Settings.Clear();
                Settings = null;
            }
        }
    }

    /// <summary>
    /// provides INI File Handling.
    /// All values are case sensitive
    /// </summary>
    public static class INI
    {
        /// <summary>
        /// Reads all sections from a file
        /// </summary>
        /// <param name="FileName">File Name</param>
        /// <returns>list of sections, null if file not found.</returns>
        public static string[] getSections(string FileName)
        {
            if (File.Exists(FileName))
            {
                string[] Lines = File.ReadAllLines(FileName);
                List<string> sections = new List<string>();
                foreach (string line in Lines)
                {
                    if (line.Length > 0 && !line.StartsWith(";"))
                    {
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            sections.Add(line.Substring(1, line.Length - 2));
                        }
                    }
                }
                return sections.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Returns all settings from a section
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Section">Section name</param>
        /// <returns>List of settings (or null if file not found)</returns>
        public static NameValueCollection getSettings(string FileName, string Section)
        {
            if (File.Exists(FileName))
            {
                bool inSec = false;
                NameValueCollection nc = new NameValueCollection();
                string[] Lines = File.ReadAllLines(FileName);
                foreach (string line in Lines)
                {
                    if (line.Length > 0 && !line.StartsWith(";") && !line.StartsWith("#"))
                    {
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            inSec = (line.Substring(1, line.Length - 2) == Section);
                        }
                        else if (inSec && line.Contains("="))
                        {
                            nc.Add(line.Split('=')[0].Trim(), line.Split(new char[] { '=' }, 2)[1]);
                        }
                    }
                }
                return nc;
            }
            return null;

        }

        /// <summary>
        /// returns a single setting from the ini file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Section">Section name</param>
        /// <param name="Setting">Setting name</param>
        /// <returns>setting, or null if file, section or setting not found</returns>
        public static string getSetting(string FileName, string Section, string Setting)
        {
            try
            {
                return getSettings(FileName, Section)[Setting];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// gets an integer from the INI file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Section">Section name</param>
        /// <param name="Setting">Setting name</param>
        /// <param name="Default">Default, if value is not an integer</param>
        /// <returns>integer</returns>
        public static int getInt(string FileName, string Section, string Setting, int Default)
        {
            int i = 0;
            if (!int.TryParse(getSetting(FileName, Section, Setting), out i))
            {
                i = Default;
            }
            return i;
        }

        /// <summary>
        /// returns all sections with settings from an INI file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <returns>list of parts, null if file not found</returns>
        public static INIPart[] completeINI(string FileName)
        {
            List<INIPart> parts = new List<INIPart>();
            if (File.Exists(FileName))
            {
                INIPart p = new INIPart();
                foreach (string Line in File.ReadAllLines(FileName))
                {
                    //Filter invalid Lines
                    if (Line.Length > 0 && !Line.StartsWith(";"))
                    {
                        //New Section
                        if (Line.StartsWith("[") && Line.EndsWith("]"))
                        {
                            if (!string.IsNullOrEmpty(p.Section))
                            {
                                parts.Add(p);
                            }
                            p = new INIPart();
                            p.Section = Line.Substring(1, Line.Length - 2);
                            p.Settings = new NameValueCollection();
                        }
                        else
                        {
                            //New Setting
                            if (!string.IsNullOrEmpty(p.Section) && Line.Contains("="))
                            {
                                p.Settings.Add(Line.Split('=')[0].Trim(), Line.Split(new char[] { '=' }, 2)[1]);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(p.Section))
                {
                    parts.Add(p);
                }
                return parts.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Saves a single setting to the file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Section">Section name</param>
        /// <param name="Setting">Setting name</param>
        /// <param name="Value">Setting value</param>
        /// <param name="Purge">true, to delete all other settings in that section, false to keep them</param>
        public static void Save(string FileName, string Section, string Setting, string Value, bool Purge)
        {
            if (Purge)
            {
                INIPart P = new INIPart(Section);
                P.Settings.Add(Setting, Value);
                Save(FileName, P, true);
            }
            else
            {
                NameValueCollection nvc = getSettings(FileName, Section);
                if (nvc == null)
                {
                    nvc = new NameValueCollection();
                }
                bool added = false;
                for (int i = 0; i < nvc.Keys.Count; i++)
                {
                    if (nvc.Keys[i].ToLower() == Setting.ToLower())
                    {
                        nvc[Setting] = Value;
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    nvc.Add(Setting, Value);
                }
                Save(FileName, new INIPart(Section, nvc), true);
            }
        }

        /// <summary>
        /// Saves a complete section to the INI file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Part">Section</param>
        /// <param name="KeepOthers">if true, other sections are kept, otherwise all others are removed</param>
        public static void Save(string FileName, INIPart Part, bool KeepOthers)
        {
            if (KeepOthers)
            {
                List<INIPart> ipa = null;
                if (completeINI(FileName) != null)
                {
                    ipa = new List<INIPart>(completeINI(FileName));
                }
                
                bool added = false;
                if (ipa == null)
                {
                    ipa = new List<INIPart>();
                }
                for (int i = 0; i < ipa.Count; i++)
                {
                    if (ipa[i].Section.ToLower() == Part.Section.ToLower())
                    {
                        ipa[i] = Part;
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    ipa.Add(Part);
                }
                Save(FileName, ipa.ToArray());
                ipa.Clear();
            }
            else
            {
                Save(FileName, new INIPart[] { Part });
            }
        }

        /// <summary>
        /// Saves multiple sections to an INI file, replacing it completely
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Parts">List of sections</param>
        public static void Save(string FileName, INIPart[] Parts)
        {
            Delete(FileName);
            StreamWriter FS = File.CreateText(FileName);
            FS.NewLine = "\r\n";

            foreach (INIPart ip in Parts)
            {
                if (ip.Settings == null)
                {
                    ip.Settings = new NameValueCollection();
                }
                if (string.IsNullOrEmpty(ip.Section))
                {
                    throw new ArgumentException("Section contains no name");
                }
                else
                {
                    FS.WriteLine("[{0}]", ip.Section);
                    foreach (string ipsk in ip.Settings.AllKeys)
                    {
                        if (!ipsk.StartsWith(";"))
                        {
                            FS.WriteLine("{0}={1}", ipsk, ip.Settings[ipsk]);
                        }
                        else
                        {
                            FS.WriteLine(ipsk);
                        }
                    }
                    FS.WriteLine();
                }
            }
            FS.Close();
        }

        /// <summary>
        /// Deletes a setting from the INI File
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Section">Section name</param>
        /// <param name="Setting">Setting name</param>
        public static void Delete(string FileName, string Section, string Setting)
        {
            NameValueCollection nvc = getSettings(FileName, Section);
            if (nvc[Setting] != null)
            {
                nvc.Remove(Setting);
                Save(FileName, new INIPart(Section, nvc), true);
            }
        }

        /// <summary>
        /// Deletes a complete section
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="Section">Section name</param>
        public static void Delete(string FileName, string Section)
        {
            List<INIPart> Parts = new List<INIPart>(completeINI(FileName));
            bool removed = false;

            for (int i = 0; i < Parts.Count; i++)
            {
                if (Parts[i].Section == Section)
                {
                    Parts.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            if (removed)
            {
                Save(FileName, Parts.ToArray());
            }
        }

        /// <summary>
        /// Deletes the INI file
        /// </summary>
        /// <param name="FileName">File name</param>
        public static void Delete(string FileName)
        {
            if(File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
    }
}
