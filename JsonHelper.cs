using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace VoiceRecorder
{
    public static class JsonHelper
    {

        public static void SaveObject(object obj)
        {
            try
            {
                string file = GetFullPath("data.bin");
                using (StreamWriter wrt = new StreamWriter(file, false))
                {
                    wrt.Write(JsonConvert.SerializeObject(obj));
                }

            }
            catch (Exception)
            {
                throw;
            }
        }


        public static object LoadObject<T>()
        {
            if (File.Exists(GetFullPath("data.bin")))
            {
                string str = string.Empty;
                try
                {
                    using (StreamReader rdr = new StreamReader(GetFullPath("data.bin"), Encoding.GetEncoding(1251)))
                    {
                        str = rdr.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                return JsonConvert.DeserializeObject<T>(str);
            }
            else 
            {
              //  File.Create(GetFullPath("data.bin"));
                FonemMatrix obj = new FonemMatrix();
                SaveObject(obj);
                return obj;
            }
                
        }

        private static string GetFullPath(string fileName)
        {
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var relativePath = @fileName;
         return Path.Combine(appDir, relativePath);
        }
    }
}
