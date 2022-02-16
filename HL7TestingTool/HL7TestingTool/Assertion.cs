namespace HL7TestingTool
{
    /// <summary>
    /// 
    /// </summary>
    public class Assertion
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Assertion()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terserString"></param>
        /// <param name="value"></param>
        public Assertion(string terserString, string value)
        {
            this.TerserString = terserString;
            this.Value = value;
            this.Alternate = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terserString"></param>
        /// <param name="missing"></param>
        public Assertion(string terserString, bool missing)
        {
            this.TerserString = terserString;
            this.Missing = missing;
            this.Alternate = null;
            this.Value = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terserString"></param>
        /// <param name="value"></param>
        /// <param name="alternate"></param>
        public Assertion(string terserString, string value, string alternate)
        {
            this.TerserString = terserString;
            this.Value = value;
            this.Alternate = alternate;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Alternate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? Outcome { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TerserString { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Alternate != null ? $"Assert alternate value '{this.Value}' at '{this.TerserString}' has outcome of '{this.Outcome}'"
                : this.Missing ? $"Assert missing value at '{this.TerserString}' has outcome of '{this.Outcome}'"
                : $"Assert mandatory value '{this.Value}' at '{this.TerserString}' has outcome of '{this.Outcome}'";
        }
    }
}