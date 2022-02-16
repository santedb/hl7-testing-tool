namespace HL7TestingTool
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TestCase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TestCase()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        public TestCase(int caseNumber)
        {
            this.CaseNumber = caseNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        public int CaseNumber { get; }
    }
}