namespace HL7TestingTool.Interop
{
    /// <summary>
    /// Represents an MLLP message sender.
    /// </summary>
    public interface IMllpMessageSender
    {
        /// <summary>
        /// Sends and receives a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Returns the response message.</returns>
        string SendAndReceive(string message);
    }
}
