using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Updater
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateData
    {

        [JsonProperty]
        public List<string> FilesToDelete { get; set; } = new List<string>();

        [JsonProperty]
        readonly static object _fileLock = new object();

        public static UpdateData LoadFromFile(string path)
        {
            UpdateData updateData = null;

            lock (_fileLock)
            {

                try
                {
                    string serializedJson = File.ReadAllText(path);
                    updateData = (UpdateData)JsonConvert.DeserializeObject<UpdateData>(serializedJson, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                    });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return updateData;
        }

        public static void SaveUpdateData(UpdateData updateData, string path)
        {

            lock (_fileLock)
            {
                try
                {
                    string serializedJson = JsonConvert.SerializeObject(updateData, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                    });
                    File.WriteAllText(path, serializedJson);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }

    public static class UpdateDataExtension
    {

        public static void SaveUpdateData(this UpdateData updateData, string path)
        {

            UpdateData.SaveUpdateData(updateData, path);
        }

    }
}
