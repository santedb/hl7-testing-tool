using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HL7TestingTool
{
    /// <summary>
    /// 
    /// </summary>
    public class TestSuiteBuilder
    {
        
        public TestSuiteBuilder()
        {
            TestSteps = new List<TestStep>();
        }
        /// <summary>
        /// 
        /// </summary>
        protected List<TestStep> TestSteps { get; set; }

        
        public void Build(List<string> testStepPaths)
        {
            var serializer = new XmlSerializer(typeof(TestStep));

            foreach (var path in testStepPaths)
            {
                var splitPath = path.Split('\\');
                int.TryParse(splitPath[splitPath.Length - 1].Split('-')[2], out var testCaseNumber); // parse case number as int
                int.TryParse(splitPath[splitPath.Length - 1].Split('-')[3].Split('.')[0], out var testStepNumber); // parse step number as int

                TestStep testStep;
                using (Stream stream = new FileStream(path, FileMode.Open))
                {
                    testStep = (TestStep)serializer.Deserialize(stream);
                    testStep.CaseNumber = testCaseNumber;
                    testStep.StepNumber = testStepNumber;

                };

                this.TestSteps.Add(testStep);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public List<TestStep> GetTestCase(int caseNumber)
        {
            try
            {
                return this.TestSteps.Where(ts => ts.CaseNumber == caseNumber).ToList();
            }
            catch
            {
                return new List<TestStep>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <returns></returns>
        public TestStep GetTestStep(int caseNumber, int stepNumber)
        {
            try
            {
                return this.TestSteps.Find(ts => ts.CaseNumber == caseNumber && ts.StepNumber == stepNumber);
            }
            catch
            {
                return new TestStep();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TestStep> GetTestSuite()
        {
            return this.TestSteps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<string> Import(string filePath)
        {
            return Directory.EnumerateFiles(filePath).OrderBy(Path.GetFileName).ToList();
        }
    }
}