using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hasher.Messages
{
    /// <summary>
    /// Message interface for thread communication.
    /// </summary>
    /// <typeparam name="PayloadData">The Data type for the message data</typeparam>
    public interface IThreadMessage<PayloadData> :IThreadMessage
    {
        /// <summary>
        /// Gets the payload data associated with the current operation.
        /// </summary>
        new PayloadData Data { get; }
    }

    /// <summary>
    /// Generic message interface for thread communication.
    /// </summary>
    public interface IThreadMessage
    {
        /// <summary>
        /// The message ID
        /// </summary>
        int MessageId { get; }
        
        /// <summary>
        /// Gets the payload data associated with the current operation.
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Delegate function to send a response message.
        /// </summary>
        /// <returns>True if the message was sent successfully, otherwise false.</returns>
        Func<IThreadMessage, bool>? SendResponseMessage { get; }

    }
}
