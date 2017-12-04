
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PokemonGoGUI.Extensions
{
    public static class Compression 
    {
        private static MemoryStream msi = null;
        private static MemoryStream mso = null;

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (msi = new MemoryStream(bytes))
            using (mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (msi = new MemoryStream(bytes))
            using (mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void Dispose()
        {
            Dispose(true);
        }

        internal static void Dispose(bool disposing)
        {
            if (!disposing) return;

            msi.Dispose();
            mso.Dispose();
        }
    }
}
