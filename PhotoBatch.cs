using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace photoSorter
{
    public class PhotoBatch
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public String BatchName
        {
            get
            {
                string startDateString = StartDate.ToString("yyyy-MM-dd");
                if (StartDate == EndDate)
                    return startDateString;
                string endDateString = EndDate.ToString("dd");
                return $"{startDateString} - {endDateString}";
            }
        }

        public List<String> FileNames
        {
            get
            {
                return _files.Select(f => f.Name).ToList();
            }
        }

        public List<FileInfo> Files
        {
            get
            {
                return _files;
            }
        }




        private List<FileInfo> _files;

        public PhotoBatch(FileInfo file, DateTime date)
        {
            StartDate = date;
            EndDate = date;
            _files = new List<FileInfo>();
            _files.Add(file);
        }

        public void AddFile(FileInfo file, DateTime date)
        {
            if (!(date >= StartDate.AddDays(-1) || date <= EndDate.AddDays(1)))
                throw new PhotoBatchException("Date is too far off to be added to this photobatch");

            if (StartDate.AddDays(-1) == date)
                StartDate = date;

            if (EndDate.AddDays(1) == date)
                EndDate = date;

            _files.Add(file);
        }

        public List<String> GetFileNames()
        {
            return _files.Select(f => f.Name).ToList();
        }
    }

    public class PhotoBatchException : Exception
    {
        public PhotoBatchException()
        {
        }

        public PhotoBatchException(string message) : base(message)
        {
        }

        public PhotoBatchException(string message, Exception inner) : base(message, inner)
        {

        }

    }


}
