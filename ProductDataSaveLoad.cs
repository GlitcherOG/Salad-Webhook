using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WindowsFormsApp1
{
    [Serializable]
    class ProductDataSaveLoad
    {
        public List<GameData> AllProducts;
        public List<GameData> TrackedProducts;
        public void Save()
        {
            TrackedProducts = Program.ProductTracking;
            AllProducts = Program.StoreProducts;
            BinaryFormatter formatter = new BinaryFormatter();
            string paths = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\Products.bin");
            var stream = new FileStream(paths, FileMode.Create);
            ProductDataSaveLoad data = this;
            formatter.Serialize(stream, data);
            stream.Close();
        }

        public static ProductDataSaveLoad Load()
        {
            string paths = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaladWebHook\\Products.bin");
            if (File.Exists(paths))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                var stream = new FileStream(paths, FileMode.Open);
                ProductDataSaveLoad data = formatter.Deserialize(stream) as ProductDataSaveLoad;
                stream.Close();
                return data;
            }
            else
            {
                return null;
            }
        }
    }
}
