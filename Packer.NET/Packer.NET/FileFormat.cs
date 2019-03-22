using System;
using System.Collections.Generic;
using System.Text;

namespace PackerNET
{
    [Serializable]
    public class FileFormat
    {
        public string FilePath = null;
        public string FileName = null;
        public byte[] Data = null;
    }
}
