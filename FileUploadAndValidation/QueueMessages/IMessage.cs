using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.QueueMessages
{
    public interface IMessage
    {
        DateTime CreatedAt { get; }

    }
}
