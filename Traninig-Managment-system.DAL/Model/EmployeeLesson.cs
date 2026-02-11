using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Model
{
    public class EmployeeLesson
    {
        public int Id { get; set; }

        // 🔹 Foreign Keys
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        // 🔹 Tracking
        public bool IsCompleted { get; set; } = false;

        public double WatchedPercentage { get; set; } = 0;  

        public double LastWatchedSecond { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.Now;
    }

}
