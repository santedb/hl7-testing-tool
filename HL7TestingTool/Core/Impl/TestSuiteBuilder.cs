﻿/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Shihab Khan, Nityan Khanna
 * Date: 2022-03-16
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents a test suite builder.
    /// </summary>
    public class TestSuiteBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSuiteBuilder"/> class.
        /// </summary>
        public TestSuiteBuilder()
        {
            this.TestSteps = new List<TestStep>();
        }
        /// <summary>
        /// The test steps.
        /// </summary>
        protected List<TestStep> TestSteps { get; set; }

        /// <summary>
        /// Deserializes test steps from xml files into  <see cref="TestStep"/> test steps.<c>-hr</c>
        /// </summary>
        /// <param name="testStepPaths">Paths to test step files.</param>
        public void Build(List<string> testStepPaths)
        {
            var serializer = new XmlSerializer(typeof(TestStep));

            foreach (var path in testStepPaths)
            {
                var splitPath = path.Split('\\');
                int.TryParse(splitPath[^1].Split('-')[2], out var testCaseNumber); // parse case number as int
                int.TryParse(splitPath[^1].Split('-')[3].Split('.')[0], out var testStepNumber); // parse step number as int

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
        /// Gets the test step(s) by case number.
        /// </summary>
        /// <param name="caseNumber">The case number.</param>
        /// <returns>the matching test step(s).</returns>
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
        /// Gets the test step by case number and step number.
        /// </summary>
        /// <param name="caseNumber">The case number.</param>
        /// <param name="stepNumber">The step number.</param>
        /// <returns>the matching test step</returns>
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
        /// Gets the test steps.
        /// </summary>
        /// <returns></returns>
        public List<TestStep> GetTestSuite()
        {
            return this.TestSteps;
        }

        /// <summary>
        /// Gets the list of full names of files from the path where all the tests are located
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>the full names of files (including paths)</returns>
        public List<string> Import(string filePath)
        {
            return Directory.EnumerateFiles(filePath).OrderBy(Path.GetFileName).ToList();
        }
    }
}