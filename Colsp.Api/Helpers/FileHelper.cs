using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace Colsp.Api.Helpers
{
    public static class FileHelper
    {
        public static Size GetImageDimention(string path)
        {
            Image img = System.Drawing.Image.FromFile(path);
            Size size = img.Size;
            img.Dispose();
            return size;
        }

        public static FileInfo GetNextImageFileName(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);

            for (int i = 1; ; ++i)
            {
                if (!File.Exists(path))
                    return new FileInfo(path);

                path = Path.Combine(dir, fileName + "_" + i + fileExt);
            }
        }


    }
}