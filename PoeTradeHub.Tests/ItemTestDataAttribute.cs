using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace PoeTradeHub.Tests
{
    public class ItemTestDataAttribute : DataAttribute
    {
        private readonly string _filePath;

        public ItemTestDataAttribute(string filePath)
        {
            _filePath = filePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            var parameters = testMethod.GetParameters();
            var path = GetFilePath(_filePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find test data at: {path}");
            }

            string testContent = File.ReadAllText(path, Encoding.UTF8);
            var testData = JsonConvert.DeserializeObject<IList<ItemTestData>>(testContent);
            IList<string> dataFiles = LoadDataFiles(testData);

            return GetData(testData, dataFiles);
        }

        private IEnumerable<object[]> GetData(IList<ItemTestData> testData, IList<string> dataFiles)
        {
            var objectList = new List<object[]>();

            for (int i = 0; i < testData.Count; i++)
            {
                objectList.Add(new object[]
                {
                    dataFiles[i],
                    testData[i].Test,
                });
            }

            return objectList;
        }

        private string GetFilePath(string filePath)
        {
            return Path.IsPathRooted(filePath) ? filePath : Path.Combine(Directory.GetCurrentDirectory(), filePath);
        }

        private IList<string> LoadDataFiles(IList<ItemTestData> testData)
        {
            var dataFiles = new List<string>(testData.Count);
            foreach (var test in testData)
            {
                var filePath =  GetFilePath($"Data/{test.File}");
                if (!File.Exists(filePath))
                {
                    throw new ArgumentException($"Could not find file: {filePath}");
                }

                dataFiles.Add(File.ReadAllText(filePath, Encoding.UTF8));
            }

            return dataFiles;
        }
    }
}
