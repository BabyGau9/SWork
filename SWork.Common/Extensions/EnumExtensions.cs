using System;
using SWork.Data.Enum;

namespace SWork.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string ToString(this InterviewStatus status)
        {
            return status switch
            {
                InterviewStatus.SCHEDULED => "Scheduled",
                InterviewStatus.ACCEPTED => "Accepted",
                InterviewStatus.REJECTED => "Rejected",
                InterviewStatus.CANCELLED => "Cancelled",
                InterviewStatus.COMPLETED => "Completed",
                InterviewStatus.PENDING => "Pending",
                _ => throw new ArgumentException($"Invalid interview status: {status}")
            };
        }
    }
} 