using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace fileGen
{
    public class FileWriter : IDisposable
    {
        public string FullName
        {
            get
            {
                return Path.Combine(Directory, CurrentFile);
            }
        }
        public int CurrentNumber
        { get; private set; }
        public string CurrentFile
        {
            get
            {
                return Mask.Replace("!", CurrentNumber.ToString());
            }
        }
        public long CurrentPosition
        {
            get
            {
                return CurrentStream == null ? -1 : CurrentStream.Position;
            }
        }
        public string Directory
        { get; private set; }
        public string Mask
        { get; private set; }
        public long Size
        { get; private set; }
        public int Count
        { get; private set; }

        private FileStream CurrentStream = null;

        public FileWriter(string Directory, string Mask, long Size, int Count)
        {
            this.Directory = Directory;
            this.Mask = Mask;
            this.Size = Size;
            this.Count = Mask.Contains("!") ? Count : 0;
            this.CurrentNumber = 0;
        }

        public bool Write(byte[] b)
        {
            return Write(b, 0, b.Length);
        }

        public bool Write(byte[] b, int Index, int Count)
        {
            if (CurrentStream == null)
            {
                OpenNext();
            }
            if (CurrentPosition + Count > Size && Size > 0)
            {
                int FirstPart = (int)(Size - CurrentPosition);
                if (FirstPart > 0)
                {
                    CurrentStream.Write(b, Index, FirstPart);
                }
                //in case of existing oversized files
                while (CurrentPosition >= Size)
                {
                    if (!OpenNext())
                    {
                        return false;
                    }
                }
                //write the rest
                Write(b, Index + FirstPart, Count - FirstPart);
            }
            else
            {
                CurrentStream.Write(b, Index, Count);
            }
            return true;
        }

        private bool OpenNext()
        {
            lock (this)
            {
                if (CurrentStream != null)
                {
                    CurrentStream.Close();
                    CurrentStream.Dispose();
                    CurrentStream = null;
                }
                if (++CurrentNumber > Count && Count > 0)
                {
                    return false;
                }
                if (File.Exists(FullName))
                {
                    CurrentStream = File.OpenWrite(FullName);
                    CurrentStream.Seek(0, SeekOrigin.End);
                }
                else
                {
                    CurrentStream = File.Create(FullName);
                }
            }
            return true;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (CurrentStream != null)
                {
                    CurrentStream.Close();
                    CurrentStream.Dispose();
                    CurrentStream = null;
                }
            }
        }
    }
}
